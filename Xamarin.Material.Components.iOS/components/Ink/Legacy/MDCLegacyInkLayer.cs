/*
 Copyright 2017-present the Material Components for iOS authors. All Rights Reserved.

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
using Xamarin.Material.Components.MaterialMath;

namespace Xamarin.Material.Components.components.Ink.Legacy
{
    public class MDCLegacyInkLayer : CALayer
    {
        // State tracking for ink.
        public enum MDCInkRippleState
        {
            kInkRippleNone,
            kInkRippleSpreading,
            kInkRippleComplete,
            kInkRippleCancelled,
        };

        private readonly string MDCLegacyInkLayerBoundedKey = @"MDCLegacyInkLayerBoundedKey";
        private readonly string MDCLegacyInkLayerMaxRippleRadiusKey = @"MDCLegacyInkLayerMaxRippleRadiusKey";
        private readonly string MDCLegacyInkLayerInkColorKey = @"MDCLegacyInkLayerInkColorKey";
        private readonly string MDCLegacyInkLayerSpreadDurationKey = @"MDCLegacyInkLayerSpreadDurationKey";
        private readonly string MDCLegacyInkLayerEvaporateDurationKey =
            @"MDCLegacyInkLayerEvaporateDurationKey";
        private readonly string MDCLegacyInkLayerUseCustomInkCenterKey =
            @"MDCLegacyInkLayerUseCustomInkCenterKey";
        private readonly string MDCLegacyInkLayerCustomInkCenterKey = @"MDCLegacyInkLayerCustomInkCenterKey";
        private readonly string MDCLegacyInkLayerUseLinearExpansionKey =
            @"MDCLegacyInkLayerUseLinearExpansionKey";

        protected CGPoint MDCLegacyInkLayerInterpolatePoint(CGPoint start,
                                                                CGPoint end,
                                                                nfloat offsetPercent)
        {
            CGPoint centerOffsetPoint = new CGPoint(start.X + (end.X - start.X) * offsetPercent,
                                                    start.Y + (end.Y - start.Y) * offsetPercent);
            return centerOffsetPoint;
        }

        private nfloat MDCLegacyInkLayerRadiusBounds(nfloat maxRippleRadius,
                                                     nfloat inkLayerRectHypotenuse,
                                                            bool bounded)
        {
            if (maxRippleRadius > 0)
            {
#if MDC_BOUNDED_INK_IGNORES_MAX_RIPPLE_RADIUS
                if (!bounded)
                {
                    return maxRippleRadius;
                }
                else
                {
                    //private dispatch_once_t onceToken;
                    //dispatch_once(&onceToken, ^{
                    //    NSLog(@"Implementation of MDCInkView with |MDCInkStyle| MDCInkStyleBounded and "
                
                    //          @"maxRippleRadius has changed.\n\n"
                
                    //          @"MDCInkStyleBounded ignores maxRippleRadius. "
                
                    //          @"Please use |MDCInkStyle| MDCInkStyleUnbounded to continue using maxRippleRadius.");
                    //});
                    return inkLayerRectHypotenuse;
                }
#else
                return maxRippleRadius;
#endif
            }
            else
            {
                return inkLayerRectHypotenuse;
            }
        }

        private nfloat MDCLegacyInkLayerRandom()
        {
            UInt32 max_value = 10000;
            Random rnd = new Random();
            return rnd.Next(1, (int)(max_value + 1)) / max_value;

        }

        protected CGPoint MDCLegacyInkLayerRectGetCenter(CGRect rect)
        {
            return new CGPoint(rect.GetMidX(), rect.GetMidY());
        }

        private nfloat MDCLegacyInkLayerRectHypotenuse(CGRect rect)
        {
            return MDCMath.MDCHypot(rect.Width, rect.Height);
        }

        protected readonly string kInkLayerOpacity = @"opacity";
        protected readonly string kInkLayerPosition = @"position";
        protected readonly string kInkLayerScale = @"transform.scale";

        //#if defined(__IPHONE_10_0) && (__IPHONE_OS_VERSION_MAX_ALLOWED >= __IPHONE_10_0)
        //    @interface MDCLegacyInkLayerRipple () <CAAnimationDelegate>
        //    @end
        //#endif

        //@interface MDCLegacyInkLayerRipple()
        #region Properties
        private bool _animationCleared;
        public bool AnimationCleared => _animationCleared;
        public MDCLegacyInkLayerRippleDelegate animationDelegate;
        public CALayer InkLayer { get; set; }
        public nfloat Radius { get; set; }
        public CGPoint Point { get; set; }
        public CGRect TargetFrame { get; set; }
        public MDCInkRippleState RippleState { get; set; }
        public UIColor Color { get; set; }

        /*** Clips the ripple to the bounds of the layer. */
        private bool _bounded;
        public bool Bounded => _bounded;

        /*** Maximum radius of the ink. No maximum if radius is 0 or less. This value is ignored if
         @c bounded is set to |YES|.*/
        public nfloat MaxRippleRadius { get; set; }

        /*** Set the foreground color of the ink. */
        public UIColor InkColor { get; set; }

        /*** Spread duration. */
        public TimeSpan SpreadDuration { get; set; }

        /*** Evaporate duration */
        public TimeSpan EvaporateDuration { get; set; }

        /***
         Set to YES if the ink layer should be using a custom center.
         */
        public bool UseCustomInkCenter { get; set; }

        /***
         Center point which ink gravitates towards.

         Ignored if useCustomInkCenter is not set.
         */
        public nfloat CustomInkCenter { get; set; }

        /***
         Whether linear expansion should be used for the ink, rather than a Quantum curve. Useful for
         ink which needs to fill the bounds of its view completely and leave those bounds at full speed.
         */
        public bool UserLinearExpansion { get; set; }

        #endregion


        public MDCLegacyInkLayer()
        {
            RippleState = MDCInkRippleState.kInkRippleNone;
            _animationCleared = true;
        }

        protected virtual void SetupRipple()
        {
            FillColor = Color.CGColor;
            nfloat dim = Radius * 2.0f;
            Frame = new CGRect(0, 0, dim, dim);
            UIBezierPath ripplePath = UIBezierPath.FromOval(new CGRect(0, 0, dim, dim));
            Path = ripplePath.CGPath;
        }

        protected virtual void Enter(bool animated)
        {
            RippleState = MDCInkRippleState.kInkRippleSpreading;
            InkLayer.AddSublayer(this);
            _animationCleared = false;
        }

        protected virtual void Exit(bool animated)
        {
            if (RippleState != MDCInkRippleState.kInkRippleCancelled)
                RippleState = MDCInkRippleState.kInkRippleComplete;
        }

        private CAKeyFrameAnimation OpacityAnimWithValues(NSNumber[] values,
                                                  NSNumber[] times)
        {
            CAKeyFrameAnimation anim = CAKeyFrameAnimation.FromKeyPath(kInkLayerOpacity);
            anim.FillMode = CAFillMode.Forwards;
            anim.KeyTimes = times;
            anim.RemovedOnCompletion = false;
            anim.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            anim.Values = values;
            return anim;
        }

        protected CAKeyFrameAnimation PositionAnimWithPath(CGPath path,
                                     nfloat duration,
                                                         CAMediaTimingFunction timingFunction)
        {
            CAKeyFrameAnimation anim = CAKeyFrameAnimation.FromKeyPath(kInkLayerPosition);
            anim.Duration = duration;
            anim.FillMode = CAFillMode.Forwards;
            anim.Path = path;
            anim.RemovedOnCompletion = false;
            anim.TimingFunction = timingFunction;
            return anim;
        }

        private CAKeyFrameAnimation ScaleAnimWithValues(NSNumber[] values,
                                                NSNumber[] times)
        {
            CAKeyFrameAnimation anim = CAKeyFrameAnimation.FromKeyPath(kInkLayerScale);
            anim.FillMode = CAFillMode.Forwards;
            anim.KeyTimes = times;
            anim.RemovedOnCompletion = false;
            anim.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            anim.Values = values;
             return anim;
        }

        private CAMediaTimingFunction LogDecelerateEasing()
        {
            // This bezier curve is an approximation of a log curve.
            return CAMediaTimingFunction.FromControlPoints(0.157f,0.72f,0.386f,0.987f);
        }

        private void AnimationDidStop(CAAnimation anim, bool finished)
        {
            if (!_animationCleared)
                AnimationDelegate animationDidStop:anim shapeLayer:self finished:finished];
        }

    }
}
