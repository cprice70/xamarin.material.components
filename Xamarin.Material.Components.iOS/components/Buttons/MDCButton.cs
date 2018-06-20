using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Material.Components.components.Ink;

namespace Xamarin.Material.Components.components.Buttons
{
    public class MDCButton : UIButton
    {
        #region DictionaryKeys
        public static string MDCButtonEnabledBackgroundColorKey = "MDCButtonEnabledBackgroundColorKey";
        public static string MDCButtonDisabledBackgroundColorLightKey = "MDCButtonDisabledBackgroundColorLightKey";
        public static string MDCButtonDisabledBackgroundColorDarkKey = "MDCButtonDisabledBackgroundColorDarkKey";
        public static string MDCButtonInkViewInkStyleKey = "MDCButtonInkViewInkStyleKey";
        public static string MDCButtonInkViewInkColorKey = @"MDCButtonInkViewInkColorKey";
        public static string MDCButtonInkViewInkMaxRippleRadiusKey =
            @"MDCButtonInkViewInkMaxRippleRadiusKey";
        public static string MDCButtonShouldRaiseOnTouchKey = @"MDCButtonShouldRaiseOnTouchKey";
        // Previous value kept for backwards compatibility.
        public static string MDCButtonUppercaseTitleKey = @"MDCButtonShouldCapitalizeTitleKey";
        // Previous value kept for backwards compatibility.
        public static string MDCButtonUnderlyingColorHintKey = @"MDCButtonUnderlyingColorKey";
        public static string MDCButtonDisableAlphaKey = @"MDCButtonDisableAlphaKey";
        public static string MDCButtonEnableAlphaKey = @"MDCButtonEnableAlphaKey";
        public static string MDCButtonCustomTitleColorKey = @"MDCButtonCustomTitleColorKey";
        public static string MDCButtonAreaInsetKey = @"MDCButtonAreaInsetKey";
        public static string MDCButtonMinimumSizeKey = @"MDCButtonMinimumSizeKey";
        public static string MDCButtonMaximumSizeKey = @"MDCButtonMaximumSizeKey";

        public static string MDCButtonUserElevationsKey = @"MDCButtonUserElevationsKey";
        public static string MDCButtonBackgroundColorsKey = @"MDCButtonBackgroundColorsKey";
        // Previous value kept for backwards compatibility.
        public static string MDCButtonNontransformedTitlesKey = @"MDCButtonAccessibilityLabelsKey";

        public static string MDCButtonBorderColorsKey = @"MDCButtonBorderColorsKey";
        public static string MDCButtonBorderWidthsKey = @"MDCButtonBorderWidthsKey";

        public static string MDCButtonShadowColorsKey = @"MDCButtonShadowColorsKey";

        public static string MDCButtonFontsKey = @"MDCButtonFontsKey";
#endregion

        // Specified in Material Guidelines
        // https://material.io/guidelines/layout/metrics-keylines.html#metrics-keylines-touch-target-size
        public static nfloat MDCButtonMinimumTouchTargetHeight = 48;
        public static nfloat MDCButtonMinimumTouchTargetWidth = 48;
        public static nfloat MDCButtonDefaultCornerRadius = 2.0f;

        public static nfloat MDCButtonAnimationDuration = 0.2f;

        // https://material.io/go/design-buttons#buttons-main-buttons
        public static nfloat MDCButtonDisabledAlpha = 0.12f;

        // Blue 500 from https://material.io/go/design-color-theming#color-color-palette .
        public static UInt32 MDCButtonDefaultBackgroundColor = 0x191919;

        // Creates a UIColor from a 24-bit RGB color encoded as an integer.
        public UIColor MDCColorFromRGB(UInt32 rgbValue)
        {
            return UIColor.FromRGBA(red: ((nfloat)((rgbValue & 0xFF0000) >> 16)) / 255,
                                    green: ((nfloat)((rgbValue & 0x00FF00) >> 8)) / 255,
                                    blue: ((nfloat)((rgbValue & 0x0000FF) >> 0)) / 255,
                                    alpha: 1);
        }

        public static NSAttributedString UppercaseAttributedString(NSAttributedString inputString)
        {
            // Store the attributes.
            NSMutableArray<NSDictionary> attributes = new NSMutableArray<NSDictionary>();

            //[string enumerateAttributesInRange:NSMakeRange(0, [string length])
            //                 options:0
            //              usingBlock:^(NSDictionary* attrs, NSRange range, __unused BOOL * stop) {
            //                [attributes addObject:@{
            //                  @"attrs" : attrs,
            //                  @"range" : [NSValue valueWithRange:range]
            //}];
            //}];

            // Make the string uppercase.
            //          NSString* uppercaseString = [[string string] uppercaseStringWithLocale:[NSLocale currentLocale]];

            //          // Apply the text and attributes to a mutable copy of the title attributed string.
            //          NSMutableAttributedString* mutableString = [string mutableCopy];
            //[mutableString replaceCharactersInRange:NSMakeRange(0, [string length])
            //                             withString:uppercaseString];
            //for (NSDictionary* attribute in attributes) {
            //  [mutableString setAttributes:attribute[@"attrs"] range:[attribute[@"range"] rangeValue]];
            //}

            //return [mutableString copy];
            return null;
        }

        // For each UIControlState.
        private NSMutableDictionary<NSNumber, NSNumber> _userElevations;
        private NSMutableDictionary<NSNumber, UIColor> _backgroundColors;
        private NSMutableDictionary<NSNumber, UIColor> _borderColors;
        private NSMutableDictionary<NSNumber, NSNumber> _borderWidths;
        private NSMutableDictionary<NSNumber, UIColor> _shadowColors;
        private NSMutableDictionary<NSNumber, UIColor> _imageTintColors;
        private NSMutableDictionary<NSNumber, UIFont> _fonts;

        private nfloat _enabledAlpha;
        private bool _hasCustomDisabledTitleColor;
        private bool _imageTintStatefulAPIEnabled;

        // Cached accessibility settings.
        private NSMutableDictionary<NSNumber, NSString> _nontransformedTitles;
        private string _accessibilityLabelExplicitValue;

        private bool _mdc_adjustsFontForContentSizeCategory;

#region Properties
        public MDCInkView inkView { get; set; }
        public MDCShapedShadowLayer layer { get; set; }

        /***
            The alpha value that will be applied when the button is disabled. Most clients can leave this as
            the default value to get a semi-transparent button automatically.
        */
        public nfloat DisabledAlpha { get; set; }

        /***
         If true, converts the button title to uppercase. Changing this property to NO will update the
         current title string.

         Default is YES.
         */
        private bool _isUppercaseTitle = true;
        public bool UppercaseTitle => _isUppercaseTitle;

        /***
        The apparent background color as seen by the user, i.e. the color of the view behind the button.

        The underlying color hint is used by buttons to calculate accessible title text colors when in
        states with transparent background colors. The hint is used whenever the button changes state such
        that the background color changes, for example, setting the background color or disabling the
        button.
        
        For flat buttons, this is the color of both the surrounding area and the button's background.
        For raised and floating buttons, this is the color of view underneath the button.
        
        The default is nil.  If left unset, buttons will likely have an incorrect appearance when
        disabled. Additionally, flat buttons might have text colors with low accessibility.
        */
        public UIColor UnderlyingColorHint { get; set; }

        /***
        Insets to apply to the button’s hit area.

        Allows the button to detect touches outside of its bounds. A negative value indicates an
        extension past the bounds.

        Default is UIEdgeInsetsZero.
        */
        public UIEdgeInsets HitAreaInsets { get; set; } = UIEdgeInsets.Zero;

        /***
         The minimum size of the button’s alignment rect. If either the height or width are non-positive
         (negative or zero), they will be ignored and that axis will adjust to its content size.

         Defaults to CGSizeZero.
         */
        public CGSize MinimumSize { get; set; } = CGSize.Empty;

        /***
        The maximum size of the button’s alignment rect. If either the height or width are non-positive
        (negative or zero), they will be ignored and that axis will adjust to its content size. Setting a
        maximum size may result in image clipping or text truncation.

        Defaults to CGSizeZero.
        */
        public CGSize MaximumSize { get; set; } = CGSize.Empty;

#endregion

        #region Deprecated
        /***
            This property sets/gets the title color for UIControlStateNormal.
        */
        [ObsoleteAttribute("This property is obsolete. Use NewProperty instead.", false)] 
        public UIColor CustomTitleColor { get; set;}
            
        [ObsoleteAttribute("Use MDCFlatButton instead of shouldRaiseOnTouch = NO", false)]
        public bool ShouldRaiseOnTouch { get; set; }

        [ObsoleteAttribute("Use uppercaseTitle instead.", false)]
        public bool ShouldCapitalizeTitle;

        [ObsoleteAttribute("Use underlyingColorHint instead.", false)]
        UIColor UnderlyingColor { get; set; }
#endregion

        public MDCButton()
        {
            this(CGRect.Empty);
        }

        public MDCButton(CGRect frame) : base(frame)
        {
            // Set up title label attributes.
            // TODO(#2709): Have a single source of truth for fonts
            // Migrate to [UIFont standardFont] when possible
            TitleLabel.Font = MDCTypography.ButtonFont;

            CommonMDCButtonInit();
            UpdateBackgroundColor();
        }

        public MDCButton(NSCoder aDecoder) : base (aDecoder)
        {
            CommonMDCButtonInit();
            //if (TitleLabel.Font)
            //{
            //    _fonts = [@{ }
            //    mutableCopy];
            //    _fonts[@(UIControlStateNormal)] = self.titleLabel.font;
            //}

            if (aDecoder.ContainsKey(MDCButtonInkViewInkStyleKey))
            {
                inkView.inkStyle = aDecoder.DecodeInt(MDCButtonInkViewInkStyleKey);
            }

            if (aDecoder.ContainsKey(MDCButtonInkViewInkColorKey))
            {
                inkView.inkColor = aDecoder.DecodeObject(MDCButtonInkViewInkColorKey);
            }

            if (aDecoder.ContainsKey(MDCButtonInkViewInkMaxRippleRadiusKey))
            {
                inkView.maxRippleRadius = aDecoder.DecodeDouble(MDCButtonInkViewInkMaxRippleRadiusKey);
            }

            if (aDecoder.ContainsKey(MDCButtonShouldRaiseOnTouchKey))
                ShouldRaiseOnTouch = aDecoder.DecodeBool(MDCButtonShouldRaiseOnTouchKey);

            if (aDecoder.ContainsKey(MDCButtonUppercaseTitleKey))
            {
                _isUppercaseTitle = aDecoder.DecodeBool(MDCButtonUppercaseTitleKey);
            }

            if (aDecoder.ContainsKey(MDCButtonUnderlyingColorHintKey))
            {
                UnderlyingColorHint = (UIColor)aDecoder.DecodeObject(MDCButtonUnderlyingColorHintKey);
            }

            if (aDecoder.ContainsKey(MDCButtonDisableAlphaKey)) {
                DisabledAlpha = (nfloat)aDecoder.DecodeDouble(MDCButtonDisableAlphaKey);
            }

            if (aDecoder.ContainsKey(MDCButtonEnableAlphaKey)) {
                _enabledAlpha = (nfloat)aDecoder.DecodeDouble(MDCButtonEnableAlphaKey);
            }

            if (aDecoder.ContainsKey(MDCButtonAreaInsetKey)) {
                HitAreaInsets = aDecoder.DecodeUIEdgeInsets(MDCButtonAreaInsetKey);
            }

            if (aDecoder.ContainsKey(MDCButtonMinimumSizeKey))
            {
                MinimumSize = aDecoder.DecodeCGSize(MDCButtonMinimumSizeKey);
            }

            if (aDecoder.ContainsKey(MDCButtonMaximumSizeKey))
            {
                MaximumSize = aDecoder.DecodeCGSize(MDCButtonMaximumSizeKey);
            }

            if (aDecoder.ContainsKey(MDCButtonUserElevationsKey))
            {
                _userElevations = (NSMutableDictionary<NSNumber, NSNumber>)aDecoder.DecodeObject(MDCButtonUserElevationsKey);
            }

            if (aDecoder.ContainsKey(MDCButtonBorderColorsKey)) {
                _borderColors = (NSMutableDictionary<NSNumber, UIColor>)aDecoder.DecodeObject(MDCButtonBorderColorsKey);
            }

            if (aDecoder.ContainsKey(MDCButtonBorderWidthsKey)) {
                _borderWidths = (NSMutableDictionary<NSNumber, NSNumber>)aDecoder.DecodeObject(MDCButtonBorderWidthsKey);
            }

            if (aDecoder.ContainsKey(MDCButtonBackgroundColorsKey)) {
                _backgroundColors = (NSMutableDictionary<NSNumber, UIColor>)aDecoder.DecodeObject(MDCButtonBackgroundColorsKey);
            } else {
                // Storyboards will set the backgroundColor via the UIView backgroundColor setter, so we have
                // to write that in to our _backgroundColors dictionary.
                //TODO
               // _backgroundColors[UIControlState.Normal] = Layer.ShapedBackgroundColor;
            }
            UpdateBackgroundColor();

            if (aDecoder.ContainsKey(MDCButtonFontsKey))
            {
                _fonts = (NSMutableDictionary<NSNumber, UIFont>)aDecoder.DecodeObject(MDCButtonFontsKey);
            }

            if (aDecoder.ContainsKey(MDCButtonNontransformedTitlesKey)) {
                _nontransformedTitles = (NSMutableDictionary<NSNumber, NSString>)aDecoder.DecodeObject(MDCButtonNontransformedTitlesKey);
            }

            if (aDecoder.ContainsKey(MDCButtonShadowColorsKey)) {
                _shadowColors = (NSMutableDictionary<NSNumber, UIColor>)aDecoder.DecodeObject(MDCButtonShadowColorsKey);
            }
        }

        private void UpdateBackgroundColor()
        {
            throw new NotImplementedException();
        }

        private void CommonMDCButtonInit()
        {
            throw new NotImplementedException();
        }
    }
}
