using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Xamarin.Material.Components.components.Ink.Legacy
{
    // State tracking for ink.
    public enum MDCInkRippleState
    {
        kInkRippleNone,
        kInkRippleSpreading,
        kInkRippleComplete,
        kInkRippleCancelled,
    };

    public class MDCLegacyInkLayerRipple
    {
        private bool _animationCleared;
        public bool AnimationCleared { get => _animationCleared; }

        public MDCLegacyInkLayerRippleDelegate animationDelegate;
        public bool Bounded { get; set; }
        public CALayer InkLayer { get; set; }
        public nfloat Radius { get; set; }
        public CGPoint Point { get; set; }
        public CGRect TargetFrame { get; set; }
        public MDCInkRippleState RippleState { get; set; }
        public UIColor Color { get; set; }

        public MDCLegacyInkLayerRipple()
        {
        }
    }
}
