using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Material.Components.components.Ink.Legacy
{
    public class MDCLegacyInkLayerForegroundRipple : MDCLegacyInkLayerRipple, ICAAnimationDelegate
    {
        //public bool UseCustomInkCenter { get; set; }
        //public CGPoint CustomInkCenter { get; set; }
        public CAKeyFrameAnimation ForegroundOpacityAnim { get; set; }
        public CAKeyFrameAnimation ForegroundPositionAnim { get; set; }
        public CAKeyFrameAnimation ForegroundScaleAnim { get; set; }

        private readonly nfloat kInkLayerForegroundBoundedOpacityExitDuration = 0.4f;
        private readonly nfloat kInkLayerForegroundBoundedPositionExitDuration = 0.3f;
        private readonly nfloat kInkLayerForegroundBoundedRadiusExitDuration = 0.8f;
        private readonly nfloat kInkLayerForegroundRadiusGrowthMultiplier = 350.0f;
        private readonly nfloat kInkLayerForegroundUnboundedEnterDelay = 0.08f;
        private readonly nfloat kInkLayerForegroundUnboundedOpacityEnterDuration = 0.12f;
        private readonly nfloat kInkLayerForegroundWaveTouchDownAcceleration = 1024.0f;
        private readonly nfloat kInkLayerForegroundWaveTouchUpAcceleration = 3400.0f;
        private const string kInkLayerForegroundOpacityAnim = @"foregroundOpacityAnim";
        private const string kInkLayerForegroundPositionAnim = @"foregroundPositionAnim";
        private const string kInkLayerForegroundScaleAnim = @"foregroundScaleAnim";

        public MDCLegacyInkLayerForegroundRipple()
        {
        }

        protected override void SetupRipple()
        {
            nfloat random = MDCLegacyInkLayerRandom();
            Radius = (0.9f + random * 0.1f) * kInkLayerForegroundRadiusGrowthMultiplier;
            base.SetupRipple();
        }

        public nfloat MDCLegacyInkLayerRandom()
        {
            const UInt32 max_value = 10000;
            Random rnd = new Random();
            return (nfloat)rnd.Next((int)(max_value + 1)) / max_value;
        }

        protected override void Exit(bool animated)
        {
            RippleState = MDCInkRippleState.kInkRippleCancelled;
            Exit(animated, null);
        }
        protected void Exit(bool animated, Action completionBlock)
        {
            base.Exit(animated);

            if (!animated)
            {
                RemoveAllAnimations();
                CATransaction.Begin();
                CATransaction.DisableActions = true;
                Opacity = 0;
                //__weak MDCLegacyInkLayerForegroundRipple * weakSelf = self;
                CATransaction.CompletionBlock = new Action(() =>
                {

                    this.RemoveFromSuperLayer();

                    if (this.animationDelegate != null)
                        animationDelegate.AnimationDidStop(null, this, true);
                });
                CATransaction.Commit();
                return;
            }

            if (Bounded)
            {
                ForegroundOpacityAnim.Values = new NSNumber[] { 1, 0 };
                ForegroundOpacityAnim.Duration = kInkLayerForegroundBoundedOpacityExitDuration;

                // Bounded ripples move slightly towards the center of the tap target. Unbounded ripples
                // move to the center of the tap target.

                nfloat xOffset = (nfloat)(TargetFrame.X - InkLayer?.Frame.X);
                nfloat yOffset = (nfloat)(TargetFrame.Y - InkLayer?.Frame.Y);

                CGPoint startPoint = new CGPoint(Point.X + xOffset, Point.Y + yOffset);
                CGPoint endPoint = MDCLegacyInkLayerRectGetCenter(TargetFrame);

                if (UseCustomInkCenter)
                {
                    endPoint = new CGPoint(CustomInkCenter, CustomInkCenter);
                }
                endPoint = new CGPoint(endPoint.X + xOffset, endPoint.Y + yOffset);
                CGPoint centerOffsetPoint = MDCLegacyInkLayerInterpolatePoint(startPoint, endPoint, 0.3f);
                UIBezierPath movePath = new UIBezierPath();
                movePath.MoveTo(startPoint);
                movePath.AddLineTo(centerOffsetPoint);

                ForegroundPositionAnim =
                    PositionAnimWithPath(movePath.CGPath,
                          duration: kInkLayerForegroundBoundedPositionExitDuration,
                                         timingFunction: LogDecelerateEasing());
                ForegroundScaleAnim.Values = new NSNumber[] { 0, 1 };
                ForegroundScaleAnim.KeyTimes = new NSNumber[] { 0, 1 };
                ForegroundScaleAnim.Duration = kInkLayerForegroundBoundedRadiusExitDuration;
            }
            else
            {
                NSNumber opacityVal = (NSNumber)PresentationLayer.ValueForKey((NSString)kInkLayerOpacity);
                if (opacityVal == null)
                {
                    opacityVal = NSNumber.FromFloat(0.0f);
                }
                nfloat adjustedDuration = kInkLayerForegroundBoundedPositionExitDuration;
                nfloat normOpacityVal = opacityVal.FloatValue;
                nfloat opacityDuration = normOpacityVal / 3.0f;
                ForegroundOpacityAnim.Values = new NSNumber[] { opacityVal, 0 };
                ForegroundOpacityAnim.Duration = opacityDuration + adjustedDuration;

                NSNumber scaleVal = (NSNumber)PresentationLayer.ValueForKey((NSString)kInkLayerScale);
                if (scaleVal == null)
                {
                    scaleVal = NSNumber.FromFloat(0.0f);
                }
                nfloat unboundedDuration = (nfloat)Math.Sqrt(((1.0f - scaleVal.FloatValue) * Radius) /
                                              (kInkLayerForegroundWaveTouchDownAcceleration +
                                               kInkLayerForegroundWaveTouchUpAcceleration));
                ForegroundPositionAnim.Duration = unboundedDuration + adjustedDuration;
                ForegroundScaleAnim.Values = new NSNumber[] { scaleVal, 1 };
                ForegroundScaleAnim.Duration = unboundedDuration + adjustedDuration;
            }

            ForegroundOpacityAnim.KeyTimes = new NSNumber[] { 0, 1 };
            if (ForegroundOpacityAnim.Duration < ForegroundScaleAnim.Duration)
            {
                ForegroundScaleAnim.Delegate = this;
            }
            else
            {
                ForegroundOpacityAnim.Delegate = this;
            }

            ForegroundOpacityAnim.TimingFunction =
                                             CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);
            ForegroundPositionAnim.TimingFunction = LogDecelerateEasing();
            ForegroundScaleAnim.TimingFunction = LogDecelerateEasing();

            CATransaction.Begin();
            if (completionBlock != null)
            {
                CATransaction.CompletionBlock = new Action(() =>
                {
                    if (RippleState != MDCInkRippleState.kInkRippleCancelled)
                    {

                        completionBlock();
                    }
                });
            }

            AddAnimation(ForegroundOpacityAnim, kInkLayerForegroundOpacityAnim);
            AddAnimation(ForegroundPositionAnim, kInkLayerForegroundPositionAnim);
            AddAnimation(ForegroundScaleAnim, kInkLayerForegroundScaleAnim);
            CATransaction.Commit();
        }

        public override void RemoveAllAnimations()
        {
            base.RemoveAllAnimations();
            ForegroundOpacityAnim = null;
            ForegroundPositionAnim = null;
            ForegroundScaleAnim = null;
            AnimationCleared = true;
        }

        private CAMediaTimingFunction LogDecelerateEasing()
        {
            // This bezier curve is an approximation of a log curve.
            return CAMediaTimingFunction.FromControlPoints(0.157f,0.72f,0.386f,0.987f);
        }

        public static implicit operator CAAnimationDelegate(MDCLegacyInkLayerForegroundRipple v)
        {
            throw new NotImplementedException();
        }
    }
}
