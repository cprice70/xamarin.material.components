using System;
using CoreAnimation;

namespace Xamarin.Material.Components.components.Ink
{
    /***
        Delegate protocol for the MDCInkLayer. Clients may implement this protocol to receive updates when
        ink layer animations start and end.
    */
    public interface IMDCInkLayerDelegate : ICALayerDelegate
    {
        /***
        Called when the ink ripple animation begins.

        @param inkLayer The MDCInkLayer that starts animating.
        */
        void InkLayerAnimationDidStart(MDCInkLayer inkLayer);

        /***
        Called when the ink ripple animation ends.

        @param inkLayer The MDCInkLayer that ends animating.
        */
        void InkLayerAnimationDidEnd(MDCInkLayer inkLayer);

    }
}