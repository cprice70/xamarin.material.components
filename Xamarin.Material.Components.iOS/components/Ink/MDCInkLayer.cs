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
using Xamarin.Material.Components.MaterialMath;

namespace Xamarin.Material.Components.components.Ink
{
    public class MDCInkLayer : CAShapeLayer
    {
        private readonly string MDCInkLayerAnimationDelegateClassNameKey = @"MDCInkLayerAnimationDelegateClassNameKey";
        private readonly string MDCInkLayerAnimationDelegateKey = @"MDCInkLayerAnimationDelegateKey";
        private readonly string MDCInkLayerEndAnimationDelayKey = @"MDCInkLayerEndAnimationDelayKey";
        private readonly string MDCInkLayerFinalRadiusKey = @"MDCInkLayerFinalRadiusKey";
        private readonly string MDCInkLayerInitialRadiusKey = @"MDCInkLayerInitialRadiusKey";
        private readonly string MDCInkLayerMaxRippleRadiusKey = @"MDCInkLayerMaxRippleRadiusKey";
        private readonly string MDCInkLayerInkColorKey = @"MDCInkLayerInkColorKey";

        private readonly nfloat MDCInkLayerCommonDuration = 0.083f;
        private readonly nfloat MDCInkLayerEndFadeOutDuration = 0.15f;
        private readonly nfloat MDCInkLayerStartScalePositionDuration = 0.333f;
        private readonly nfloat MDCInkLayerStartFadeHalfDuration = 0.167f;
        private readonly nfloat MDCInkLayerStartFadeHalfBeginTimeFadeOutDuration = 0.25f;

        private readonly nfloat MDCInkLayerScaleStartMin = 0.2f;
        private readonly nfloat MDCInkLayerScaleStartMax = 0.6f;
        private readonly nfloat MDCInkLayerScaleDivisor = 300.0f;

        private readonly string MDCInkLayerOpacityString = @"opacity";
        private readonly string MDCInkLayerPositionString = @"position";
        private readonly string MDCInkLayerScaleString = @"transform.scale";

        #region Properties
        /***
        A Core Animation layer that draws and animates the ink effect.

        Quick summary of how the ink ripple works:

        1. On touch down, ink spreads from the touch point.
        2. On touch down hold, ink continues to spread, but will gravitate to the center point
         of the view.
        3. On touch up, the ink ripple opacity will start to decrease.
        */

        /***
         Ink layer animation delegate. Clients set this delegate to receive updates when ink layer
         animations start and end.
         */
        //public MDCInkLayerDelegate AnimationDelegate { get; set; }

        /***
         The start ink ripple spread animation has started and is active.
         */
        private bool _startAnimationActive;
        public bool StartAnimationActive => _startAnimationActive;

        /***
         Delay time in milliseconds before the end ink ripple spread animation begins.
         */
        public nfloat EndAnimationDelay { get; set; }

        /***
         The radius the ink ripple grows to when ink ripple ends.

         Default value is half the diagonal of the containing frame plus 10pt.
         */
        public nfloat FinalRadius { get; set; }

        /***
         The radius the ink ripple starts to grow from when the ink ripple begins.

         Default value is half the diagonal of the containing frame multiplied by 0.6.
         */
        public nfloat InitialRadius { get; set; }

        /***
         Maximum radius of the ink. If this is not set then the final radius value is used.
         */
        public nfloat MaxRippleRadius { get; set; }

        /***
         The color of the ink ripple.
         */
        public UIColor InkColor { get; set; }

        public bool SupportsSecureCoding => true;

        public IMDCInkLayerDelegate AnimationDelegate { get; set; }

        #endregion

        public MDCInkLayer()
        {
            InkColor = UIColor.FromWhiteAlpha(0, 0.08f);
        }

        public MDCInkLayer(CALayer layer)
        {
            EndAnimationDelay = 0;
            FinalRadius = 0;
            InitialRadius = 0;
            InkColor = UIColor.FromWhiteAlpha(0, 0.08f);
            _startAnimationActive = false;

            if (layer is MDCInkLayer)
            {
                MDCInkLayer inkLayer = (MDCInkLayer)layer;
                EndAnimationDelay = inkLayer.EndAnimationDelay;
                FinalRadius = inkLayer.FinalRadius;
                InitialRadius = inkLayer.InitialRadius;
                MaxRippleRadius = inkLayer.MaxRippleRadius;
                InkColor = inkLayer.InkColor;
                _startAnimationActive = false;
            }
        }

        public MDCInkLayer(NSCoder aDecoder) : base(aDecoder)
        {
            string delegateClassName = string.Empty;
            if (aDecoder.ContainsKey(MDCInkLayerAnimationDelegateClassNameKey))
            {
                delegateClassName = (NSString)aDecoder.DecodeObject(MDCInkLayerAnimationDelegateClassNameKey);
            }
            if (!string.IsNullOrEmpty(delegateClassName) &&
                aDecoder.ContainsKey(MDCInkLayerAnimationDelegateKey))
            {
                AnimationDelegate = (IMDCInkLayerDelegate)aDecoder.DecodeObject(MDCInkLayerAnimationDelegateKey);
            }
            if (aDecoder.ContainsKey(MDCInkLayerInkColorKey))
            {
                InkColor = (UIColor)aDecoder.DecodeObject(MDCInkLayerInkColorKey);
            }
            else
            {
                InkColor = UIColor.FromWhiteAlpha(0, 0.08f);
            }

            if (aDecoder.ContainsKey(MDCInkLayerEndAnimationDelayKey))
            {
                EndAnimationDelay = (nfloat)aDecoder.DecodeDouble(MDCInkLayerEndAnimationDelayKey);
            }

            if (aDecoder.ContainsKey(MDCInkLayerFinalRadiusKey))
            {
                FinalRadius = (nfloat)aDecoder.DecodeDouble(MDCInkLayerFinalRadiusKey);
            }
            if (aDecoder.ContainsKey(MDCInkLayerInitialRadiusKey))
            {
                InitialRadius = (nfloat)aDecoder.DecodeDouble(MDCInkLayerInitialRadiusKey);
            }
            if (aDecoder.ContainsKey(MDCInkLayerMaxRippleRadiusKey))
            {
                MaxRippleRadius = (nfloat)aDecoder.DecodeDouble(MDCInkLayerMaxRippleRadiusKey);
            }
        }

        public override void EncodeTo(NSCoder encoder)
        {
            base.EncodeTo(encoder);
            //TODO fix Conforms
            if (AnimationDelegate != null) //&& AnimationDelegate.ConformsToProtocol(NSCoding))
            {
                encoder.Encode((NSString)nameof(IMDCInkLayerDelegate), MDCInkLayerAnimationDelegateClassNameKey);
                encoder.Encode((NSObject)AnimationDelegate, MDCInkLayerAnimationDelegateKey);
            }

            encoder.Encode(EndAnimationDelay, MDCInkLayerEndAnimationDelayKey);
            encoder.Encode(FinalRadius, MDCInkLayerFinalRadiusKey);
            encoder.Encode(InitialRadius, MDCInkLayerInitialRadiusKey);
            encoder.Encode(MaxRippleRadius, MDCInkLayerMaxRippleRadiusKey);
            encoder.Encode(InkColor, MDCInkLayerInkColorKey);
        }

        public override void SetNeedsLayout()
        {
            base.SetNeedsLayout();
            SetRadiiWithRect(Bounds);
        }

        private void SetRadiiWithRect(CGRect rect)
        {
            InitialRadius = MDCMath.MDCHypot(rect.Height, rect.Width) / 2 * 0.6f;
            FinalRadius = MDCMath.MDCHypot(rect.Height, rect.Width) / 2 + 10.0f;
        }

        private void StartAnimationAtPoint(CGPoint point)
        {
            StartInkAtPoint(point, animated: true);
        }

        void StartInkAtPoint(CGPoint point, bool animated)
        {
            nfloat radius = FinalRadius;
            if (MaxRippleRadius > 0)
            {
                radius = MaxRippleRadius;
            }
            CGRect ovalRect = new CGRect(Bounds.Width / 2 - radius,
                               Bounds.Height / 2 - radius,
                               radius * 2,
                               radius * 2);
            UIBezierPath circlePath = UIBezierPath.FromOval(ovalRect);
            Path = circlePath.CGPath;
            FillColor = InkColor.CGColor;
            if (!animated)
            {
                Opacity = 1;
                Position = new CGPoint(Bounds.Width / 2, Bounds.Height / 2);
            }
            else
            {
                Opacity = 0;
                Position = point;
                _startAnimationActive = true;

                CAMediaTimingFunction materialTimingFunction = CAMediaTimingFunction.FromControlPoints(0.4f, 0, 0.2f, 1.0f);
                nfloat scaleStart = (nfloat)(Math.Min(Bounds.Width, Bounds.Height) / MDCInkLayerScaleDivisor);
                if (scaleStart < MDCInkLayerScaleStartMin)
                {
                    scaleStart = MDCInkLayerScaleStartMin;
                }
                else if (scaleStart > MDCInkLayerScaleStartMax)
                {
                    scaleStart = MDCInkLayerScaleStartMax;
                }

                CABasicAnimation scaleAnim = new CABasicAnimation
                {
                    KeyPath = MDCInkLayerScaleString,
                    From = new NSNumber(scaleStart),
                    To = new NSNumber(1.0f),
                    Duration = MDCInkLayerStartScalePositionDuration,
                    BeginTime = MDCInkLayerCommonDuration,
                    TimingFunction = materialTimingFunction,
                    FillMode = CAFillMode.Forwards,
                    RemovedOnCompletion = true
                };

                UIBezierPath centerPath = new UIBezierPath();
                CGPoint startPoint = point;
                CGPoint endPoint = new CGPoint(Bounds.Width / 2, Bounds.Height / 2);
                centerPath.MoveTo(startPoint);
                centerPath.AddLineTo(endPoint);
                centerPath.ClosePath();

                CAKeyFrameAnimation positionAnim = new CAKeyFrameAnimation
                {
                    KeyPath = MDCInkLayerPositionString,
                    Path = centerPath.CGPath,
                    KeyTimes = new NSNumber[] { 0, 1.0f },
                    Values = new NSNumber[] { 0, 1.0f },
                    Duration = MDCInkLayerStartScalePositionDuration,
                    BeginTime = MDCInkLayerCommonDuration,
                    TimingFunction = materialTimingFunction,
                    FillMode = CAFillMode.Forwards,
                    RemovedOnCompletion = false
                };

                CABasicAnimation fadeInAnim = new CABasicAnimation();
                fadeInAnim.KeyPath = MDCInkLayerOpacityString;
                fadeInAnim.From = new NSNumber(0);
                fadeInAnim.To = new NSNumber(1.0f);
                fadeInAnim.Duration = MDCInkLayerCommonDuration;
                fadeInAnim.BeginTime = MDCInkLayerCommonDuration;
                fadeInAnim.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
                fadeInAnim.FillMode = CAFillMode.Forwards;
                fadeInAnim.RemovedOnCompletion = false;

                CATransaction.Begin();
                CAAnimationGroup animGroup = new CAAnimationGroup();
                animGroup.Animations = new CAAnimation[] { scaleAnim, positionAnim, fadeInAnim };
                animGroup.Duration = MDCInkLayerStartScalePositionDuration;
                animGroup.FillMode = CAFillMode.Forwards;
                animGroup.RemovedOnCompletion = false;
                CATransaction.CompletionBlock = new Action(() => { _startAnimationActive = false; });

                AddAnimation(animGroup, null);
                CATransaction.Commit();
            }

            //if (AnimationDelegate respondsToSelector: @selector(inkLayerAnimationDidStart:)) 
            //{
            //    [self.animationDelegate inkLayerAnimationDidStart:self];
            //}
        }

        private void ChangeAnimationAtPoint(CGPoint point)
        {
            nfloat animationDelay = 0;
            if (_startAnimationActive) 
            {
                animationDelay = MDCInkLayerStartFadeHalfBeginTimeFadeOutDuration +
                MDCInkLayerStartFadeHalfDuration;
            }

            bool viewContainsPoint = Bounds.Contains(point);
            nfloat currOpacity = PresentationLayer.Opacity;
            nfloat updatedOpacity = 0;
            if (viewContainsPoint) 
            {
                updatedOpacity = 1.0f;
            }

            CABasicAnimation changeAnim = new CABasicAnimation();
            changeAnim.KeyPath = MDCInkLayerOpacityString;
            changeAnim.From = new NSNumber(currOpacity);
            changeAnim.To = new NSNumber(updatedOpacity);
            changeAnim.Duration = MDCInkLayerCommonDuration;
            changeAnim.BeginTime = ConvertTimeFromLayer(CACurrentMediaTime() + animationDelay, null);
            changeAnim.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            changeAnim.FillMode = CAFillMode.Forwards;
            changeAnim.RemovedOnCompletion = false;
            AddAnimation(changeAnim, null);
        }

        private nfloat CACurrentMediaTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void EndAnimationAtPoint(CGPoint point)
        {
            this.EndInkAtPoint(point, animated:true);
        }
        private void EndInkAtPoint(CGPoint point, bool animated)
        {
            if (StartAnimationActive) 
            {
                EndAnimationDelay = MDCInkLayerStartFadeHalfBeginTimeFadeOutDuration;
            }

            nfloat opacity = 1.0f;
            bool viewContainsPoint = Bounds.Contains(point);
            if (!viewContainsPoint) {
                opacity = 0;
            }

            if (!animated) 
            {
               Opacity = 0;
                AnimationDelegate?.InkLayerAnimationDidEnd(this);

                RemoveFromSuperLayer();
            }
            else
            {
                CATransaction.Begin();
                CABasicAnimation fadeOutAnim = new CABasicAnimation();
                fadeOutAnim.KeyPath = MDCInkLayerOpacityString;
                fadeOutAnim.From = new NSNumber(opacity);
                fadeOutAnim.To = new NSNumber(0);
                fadeOutAnim.Duration = MDCInkLayerEndFadeOutDuration;
                fadeOutAnim.BeginTime = ConvertTimeFromLayer(CACurrentMediaTime() + EndAnimationDelay, null);
                                               
                fadeOutAnim.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
                fadeOutAnim.FillMode = CAFillMode.Forwards;
                fadeOutAnim.RemovedOnCompletion = false;
                //CATransaction.CompletionBlock = new Action(() => {
                //    if ([self.animationDelegate respondsToSelector: @selector(inkLayerAnimationDidEnd:)])
                //    {
                //        AnimationDelegate inkLayerAnimationDidEnd: self];
                //    }
                //  RemoveFromSuperlayer();
                //});
                 
                AddAnimation(fadeOutAnim, null);
                CATransaction.Commit();
            }
        }
    }
}
