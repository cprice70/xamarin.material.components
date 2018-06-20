using System;
using CoreGraphics;

namespace Xamarin.Material.Components.Math
{
    public static class MDCMath
    {
        public static nfloat MDCSin(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return sin(value);
#else
            return Sin(value);
#endif
        }

        static nfloat MDCCos(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return cos(value);
#else
            return cosf(value);
#endif
        }

        static nfloat MDCAtan2(nfloat y, nfloat x)
        {
#if CGFLOAT_IS_DOUBLE
  return atan2(y, x);
#else
            return atan2f(y, x);
#endif
        }

        static nfloat MDCCeil(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return ceil(value);
#else
            return ceilf(value);
#endif
        }

        static nfloat MDCFabs(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return fabs(value);
#else
            return fabsf(value);
#endif
        }

        static nfloat MDCDegreesToRadians(nfloat degrees)
        {
#if CGFLOAT_IS_DOUBLE
  return degrees * (CGFloat)M_PI / 180.0;
#else
            return degrees * (nfloat)M_PI / 180.f;
#endif
        }

        static bool MDCCGFloatEqual(nfloat a, nfloat b)
        {
            const nfloat constantK = 3;
#if CGFLOAT_IS_DOUBLE
  const CGFloat epsilon = DBL_EPSILON;
  const CGFloat min = DBL_MIN;
#else
            const nfloat epsilon = FLT_EPSILON;
            const nfloat min = FLT_MIN;
#endif
            return (MDCFabs(a - b) < constantK * epsilon * MDCFabs(a + b) || MDCFabs(a - b) < min);
        }

        static CGFloat MDCFloor(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return floor(value);
#else
            return floorf(value);
#endif
        }

        static nfloat MDCHypot(nfloat x, nfloat y)
        {
#if CGFLOAT_IS_DOUBLE
  return hypot(x, y);
#else
            return hypotf(x, y);
#endif
        }

        // Checks whether the provided floating point number is exactly zero.
        static bool MDCCGFloatIsExactlyZero(nfloat value)
        {
            return (System.Math.Abs(value) < Single.Epsilon);
        }

        static nfloat MDCPow(nfloat value, nfloat power)
        {
#if CGFLOAT_IS_DOUBLE
  return pow(value, power);
#else
            return powf(value, power);
#endif
        }

        static nfloat MDCRint(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return rint(value);
#else
            return rintf(value);
#endif
        }

        static nfloat MDCRound(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return round(value);
#else
            return roundf(value);
#endif
        }

        static nfloat MDCSqrt(nfloat value)
        {
#if CGFLOAT_IS_DOUBLE
  return sqrt(value);
#else
            return Sqrtf(value);
#endif
        }

        /***
         Expand `rect' to the smallest standardized rect containing it with pixel-aligned origin and size.
         If @c scale is zero, then a scale of 1 will be used instead.

         @param rect the rectangle to align.
         @param scale the scale factor to use for pixel alignment.

         @return the input rectangle aligned to the nearest pixels using the provided scale factor.

         @see CGRectIntegral
         */
        static CGRect MDCRectAlignToScale(CGRect rect, nfloat scale)
        {
            if (CGRectIsNull(rect))
            {
                return CGRectNull;
            }
            if (MDCCGFloatEqual(scale, 0))
            {
                scale = 1;
            }

            if (MDCCGFloatEqual(scale, 1))
            {
                return CGRectIntegral(rect);
            }

            CGPoint originalMinimumPoint = CGPointMake(CGRectGetMinX(rect), CGRectGetMinY(rect));
            CGPoint newOrigin = CGPointMake(MDCFloor(originalMinimumPoint.x * scale) / scale,
                                            MDCFloor(originalMinimumPoint.y * scale) / scale);
            CGSize adjustWidthHeight =
                CGSizeMake(originalMinimumPoint.x - newOrigin.x, originalMinimumPoint.y - newOrigin.y);
            return CGRectMake(newOrigin.x, newOrigin.y,
                              MDCCeil((CGRectGetWidth(rect) + adjustWidthHeight.width) * scale) / scale,
                              MDCCeil((CGRectGetHeight(rect) + adjustWidthHeight.height) * scale) / scale);
        }

        static CGPoint MDCPointRoundWithScale(CGPoint point, nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0))
            {
                return CGPointZero;
            }

            return CGPointMake(MDCRound(point.x * scale) / scale, MDCRound(point.y * scale) / scale);
        }

        /**
         Expand `size' to the closest larger pixel-aligned value.
         If @c scale is zero, then a CGSizeZero will be returned.

         @param size the size to align.
         @param scale the scale factor to use for pixel alignment.

         @return the size aligned to the closest larger pixel-aligned value using the provided scale factor.
         */
        static CGSize MDCSizeCeilWithScale(CGSize size, nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0))
            {
                return CGSizeZero;
            }

            return CGSizeMake(MDCCeil(size.Width * scale) / scale, MDCCeil(size.Height * scale) / scale);
        }

        /**
         Align the centerPoint of a view so that its origin is pixel-aligned to the nearest pixel.
         Returns @c CGRectZero if @c scale is zero or @c bounds is @c CGRectNull.

         @param center the unaligned center of the view.
         @param bounds the bounds of the view.
         @param scale the native scaling factor for pixel alignment.

         @return the center point of the view such that its origin will be pixel-aligned.
         */
        static CGPoint MDCRoundCenterWithBoundsAndScale(CGPoint center,
                                                               CGRect bounds,
                                                        nfloat scale)
        {
            if (MDCCGFloatEqual(scale, 0) || CGRectIsNull(bounds))
            {
                return CGPointZero;
            }

            nfloat halfWidth = CGRectGetWidth(bounds) / 2;
            nfloat halfHeight = CGRectGetHeight(bounds) / 2;
            CGPoint origin = CGPointMake(center.x - halfWidth, center.y - halfHeight);
            origin = MDCPointRoundWithScale(origin, scale);
            return CGPointMake(origin.X + halfWidth, origin.Y + halfHeight);
        }
    }
}
