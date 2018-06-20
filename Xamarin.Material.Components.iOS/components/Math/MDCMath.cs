using System;
using CoreGraphics;

namespace Xamarin.Material.Components.MaterialMath
{
    public static class MDCMath
    {
        public static nfloat MDCSin(nfloat value)
        {
            return (nfloat)Math.Sin(value);
        }

        static nfloat MDCCos(nfloat value)
        {
            return (nfloat)Math.Cos(value);
        }

        public static nfloat MDCAtan2(nfloat y, nfloat x)
        {
            return (nfloat)Math.Atan2(y, x);
        }

        public static nfloat MDCCeil(nfloat value)
        {
            return (nfloat)Math.Ceiling(value);
        }

        public static nfloat MDCFabs(nfloat value)
        {
            return (nfloat)Math.Abs(value);
        }

        public static nfloat MDCDegreesToRadians(nfloat degrees)
        {
            return degrees * (nfloat)Math.PI / 180.0f;
        }

        public static bool MDCCGFloatEqual(nfloat a, nfloat b)
        {
            nfloat constantK = 3;
            nfloat epsilon = Single.Epsilon;
            nfloat min = Single.MinValue;

            return (MDCFabs(a - b) < constantK * epsilon * MDCFabs(a + b) || MDCFabs(a - b) < min);
        }

        public static nfloat MDCFloor(nfloat value)
        {
            return (nfloat)Math.Floor(value);
        }

        public static nfloat MDCHypot(nfloat x, nfloat y)
        {
            return (nfloat)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); 
        }

        // Checks whether the provided floating point number is exactly zero.
        public static bool MDCCGFloatIsExactlyZero(nfloat value)
        {
            return (System.Math.Abs(value) < Single.Epsilon);
        }

        public static nfloat MDCPow(nfloat value, nfloat power)
        {
            return (nfloat)Math.Pow(value, power);
        }

        public static nfloat MDCRint(nfloat value)
        {
            return (nfloat)Math.Round(value);
        }

        public static nfloat MDCRound(nfloat value)
        {
            return (nfloat)Math.Round(value);
        }

        public static nfloat MDCSqrt(nfloat value)
        {
            return (nfloat)Math.Sqrt(value);
        }

        /***
         Expand `rect' to the smallest standardized rect containing it with pixel-aligned origin and size.
         If @c scale is zero, then a scale of 1 will be used instead.

         @param rect the rectangle to align.
         @param scale the scale factor to use for pixel alignment.

         @return the input rectangle aligned to the nearest pixels using the provided scale factor.

         @see CGRectIntegral
         */
        public static CGRect MDCRectAlignToScale(CGRect rect, nfloat scale)
        {
            if (rect.IsNull())
            {
                return CGRect.Null;
            }
            if (MDCCGFloatEqual(scale, 0))
            {
                scale = 1;
            }

            if (MDCCGFloatEqual(scale, 1))
            {
                return rect.Integral();
            }

            CGPoint originalMinimumPoint = new CGPoint(rect.GetMinX(), rect.GetMinY());
            CGPoint newOrigin = new CGPoint(MDCFloor(originalMinimumPoint.X * scale) / scale,
                                            MDCFloor(originalMinimumPoint.Y * scale) / scale);
            CGSize adjustWidthHeight =
                new CGSize(originalMinimumPoint.X - newOrigin.X, originalMinimumPoint.Y - newOrigin.Y);
            return new CGRect(newOrigin.X, newOrigin.Y,
                              MDCCeil((rect.Width + adjustWidthHeight.Width) * scale) / scale,
                              MDCCeil((rect.Height + adjustWidthHeight.Height) * scale) / scale);
        }

        public static CGPoint MDCPointRoundWithScale(CGPoint point, nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0))
            {
                return CGPoint.Empty;
            }

            return new CGPoint(MDCRound(point.X * scale) / scale, MDCRound(point.Y * scale) / scale);
        }

        /***
         Expand `size' to the closest larger pixel-aligned value.
         If @c scale is zero, then a CGSizeZero will be returned.

         @param size the size to align.
         @param scale the scale factor to use for pixel alignment.

         @return the size aligned to the closest larger pixel-aligned value using the provided scale factor.
         */
        public static CGSize MDCSizeCeilWithScale(CGSize size, nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0))
            {
                return CGSize.Empty;
            }

            return new CGSize(MDCCeil(size.Width * scale) / scale, MDCCeil(size.Height * scale) / scale);
        }

        /***
         Align the centerPoint of a view so that its origin is pixel-aligned to the nearest pixel.
         Returns @c CGRectZero if @c scale is zero or @c bounds is @c CGRectNull.

         @param center the unaligned center of the view.
         @param bounds the bounds of the view.
         @param scale the native scaling factor for pixel alignment.

         @return the center point of the view such that its origin will be pixel-aligned.
         */
        public static CGPoint MDCRoundCenterWithBoundsAndScale(CGPoint center,
                                                               CGRect bounds,
                                                        nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0) || bounds.IsNull())
            {
                return CGPoint.Empty;
            }

            nfloat halfWidth = bounds.Width / 2;
            nfloat halfHeight = bounds.Height / 2;
            CGPoint origin = new CGPoint(center.X - halfWidth, center.Y - halfHeight);
            origin = MDCPointRoundWithScale(origin, scale);
            return new CGPoint(origin.X + halfWidth, origin.Y + halfHeight);
        }
    }
}
