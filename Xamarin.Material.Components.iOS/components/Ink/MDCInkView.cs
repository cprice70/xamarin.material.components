/*
 Copyright 2015-present the Material Components for iOS authors. All Rights Reserved.

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */

using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Material.Components.components.Ink.Legacy;

namespace Xamarin.Material.Components.components.Ink
{
    /*** Ink styles. */
    public enum MDCInkStyle
    {
        MDCInkStyleBounded,  /*** Ink is clipped to the view's bounds. */
        MDCInkStyleUnbounded /*** Ink is not clipped to the view's bounds. */
    }

    public class MDCInkView : UIView
    {
        private readonly string MDCInkViewAnimationDelegateKey = "MDCInkViewAnimationDelegateKey";
        private readonly string MDCInkViewInkStyleKey = "MDCInkViewInkStyleKey";
        private readonly string MDCInkViewUsesLegacyInkRippleKey = "MDCInkViewUsesLegacyInkRippleKey";
        private readonly string MDCInkViewMaskLayerKey = "MDCInkViewMaskLayerKey";
        private readonly string MDCInkViewUsesCustomInkCenterKey = "MDCInkViewUsesCustomInkCenterKey";
        private readonly string MDCInkViewCustomInkCenterKey = "MDCInkViewCustomInkCenterKey";
        private readonly string MDCInkViewInkColorKey = "MDCInkViewInkColorKey";
        private readonly string MDCInkViewMaxRippleRadiusKey = "MDCInkViewMaxRippleRadiusKey";

        /** Completion block signature for all ink animations. */
        public delegate void MDCInkCompletionBlock(); 
        #region Properties
        public CALayer AnimationSourceLayer { get; set; }
        public string KeyPath { get; set; }
        // public id FromValue;
        // public id ToValue;

        public CAShapeLayer MaskLayer { get; set; }
        public MDCInkCompletionBlock StartInkRippleCompletionBlock { get; set; }
        public MDCInkCompletionBlock EndInkRippleCompletionBlock { get; set; }
        public MDCInkLayer ActiveInkLayer { get; set; }

        /***
        Ink view animation delegate. Clients set this delegate to receive updates when ink animations
        start and end.
        */
        public IMDCInkViewDelegate AnimationDelegate { get; set; }

        // Legacy ink ripple
        public MDCLegacyInkLayer InkLayer { get; }
        /***
        Ink view animation delegate. Clients set this delegate to receive updates when ink animations
        start and end.
        */
        //public id<MDCInkViewDelegate> animationDelegate;

        /***
         The style of ink for this view. Defaults to MDCInkStyleBounded.

         Changes only affect subsequent animations, not animations in progress.
         */
        public MDCInkStyle InkStyle { get; set; }

        /** The foreground color of the ink. The default value is defaultInkColor. */
        public UIColor InkColor { get; set; }

        /** Default color used for ink if no color is specified. */
        public UIColor DefaultInkColor { get; set; }

        /***
         Maximum radius of the ink. If the radius <= 0 then half the length of the diagonal of self.bounds
         is used. This value is ignored if @c inkStyle is set to |MDCInkStyleBounded|.

         Ignored if updated ink is used.
         */
        public nfloat MaxRippleRadius { get; set; }

        /***
         Use the older legacy version of the ink ripple. Default is YES.
         */
        public bool UsesLegacyInkRipple { get; set; }

        /***
         Use a custom center for the ink splash. If YES, then customInkCenter is used, otherwise the
         center of self.bounds is used. Default is NO.

         Affects behavior only if usesLegacyInkRipple is enabled.
         */
        public bool UsesCustomInkCenter { get; set; }

        /***
         Custom center for the ink splash in the view’s coordinate system.

         Affects behavior only if both usesCustomInkCenter and usesLegacyInkRipple are enabled.
         */
        public CGPoint CustomInkCenter { get; set; }

        /***
         Start the first part of the "press and release" animation at a particular point.

         The "press and release" animation begins by fading in the ink ripple when this method is called.

         @param point The user interaction position in the view’s coordinate system.
         @param completionBlock Block called after the completion of the animation.
         */
         #endregion

        private nfloat _maxRippleRadius;

        public MDCInkView()
        {
        }

        public MDCInkView(CGRect frame) : base (frame)
        {
            CommonMDCInkViewInit();
        }

        MDCInkView(NSCoder aDecoder) : base(aDecoder)
        {
            if (aDecoder.ContainsKey(MDCInkViewAnimationDelegateKey))
            {
                AnimationDelegate = aDecoder.DecodeObject(MDCInkViewAnimationDelegateKey);
            }
            if (aDecoder.ContainsKey(MDCInkViewMaskLayerKey))
            {
                MaskLayer = (CAShapeLayer)aDecoder.DecodeObject(MDCInkViewMaskLayerKey);
                MaskLayer.Delegate = this;
            }
            else
            {
                MaskLayer = new CAShapeLayer();
                MaskLayer.Delegate = this;
            }

            if (aDecoder.ContainsKey(MDCInkViewUsesLegacyInkRippleKey))
            {
                UsesLegacyInkRipple = aDecoder.DecodeBool(MDCInkViewUsesLegacyInkRippleKey);
            }
            else
            {
                UsesLegacyInkRipple = true;
            }

            if (aDecoder.ContainsKey(MDCInkViewInkStyleKey))
            {
                InkStyle = (MDCInkStyle)aDecoder.DecodeInt(MDCInkViewInkStyleKey);
            }

            // The following are derived properties, but `layer` may not have been encoded
            if (aDecoder.ContainsKey(MDCInkViewUsesCustomInkCenterKey))
            {
                UsesCustomInkCenter = aDecoder.DecodeBool(MDCInkViewUsesCustomInkCenterKey);
            }
            if (aDecoder.ContainsKey(MDCInkViewCustomInkCenterKey))
            {
                CustomInkCenter = aDecoder.DecodeCGPoint(MDCInkViewCustomInkCenterKey);
            }
            if (aDecoder.ContainsKey(MDCInkViewMaxRippleRadiusKey))
            {
                MaxRippleRadius = (nfloat)aDecoder.DecodeDouble(MDCInkViewMaxRippleRadiusKey);
            }
            if (aDecoder.ContainsKey(MDCInkViewInkColorKey))
            {
                InkColor = (UIColor)aDecoder.DecodeObject(MDCInkViewInkColorKey);
            }
        }

        public override void EncodeTo(NSCoder encoder)
        {
            base.EncodeTo(encoder);
        
            //if (AnimationDelegate && AnimationDelegate.conformsToProtocol:@protocol(NSCoding)]) {
            //    [aCoder encodeObject:self.animationDelegate forKey:MDCInkViewAnimationDelegateKey];
            //}

            encoder.Encode((int)InkStyle, MDCInkViewInkStyleKey);
            encoder.Encode(UsesLegacyInkRipple, MDCInkViewUsesLegacyInkRippleKey);
            encoder.Encode(MaskLayer, MDCInkViewMaskLayerKey);

            // The following are derived properties, but `layer` may not get encoded by the superclass
            encoder.Encode(UsesCustomInkCenter, MDCInkViewUsesCustomInkCenterKey);
            encoder.Encode(CustomInkCenter, MDCInkViewCustomInkCenterKey);
            encoder.Encode(MaxRippleRadius, MDCInkViewMaxRippleRadiusKey);
            encoder.Encode(InkColor, MDCInkViewInkColorKey);
        }
    }
}
