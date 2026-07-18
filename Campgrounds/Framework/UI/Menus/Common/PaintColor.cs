using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Campgrounds.Framework.UI.Menus.Common
{
    public class PaintColor
    {
        public const int MIN_HUE = 0;
        public const int MAX_HUE = 360;
        public const int MIN_SATURATION = 0;
        public const int MAX_SATURATION = 75;
        public const int MIN_BRIGHTNESS = -100;
        public const int MAX_BRIGHTNESS = 100;

        /// <summary>The HSL lightness mapped to <see cref="MIN_BRIGHTNESS"/>. Vanilla's squeeze, kept so paint never blows out to pure black or white.</summary>
        public const double LIGHTNESS_FLOOR = 0.25;

        /// <summary>The HSL lightness mapped to <see cref="MAX_BRIGHTNESS"/>.</summary>
        public const double LIGHTNESS_CEILING = 0.5;

        /// <summary>Whether a color overlay is applied. When false, <see cref="GetColor"/> returns null and the HSL fields are inert.</summary>
        public bool HasColor;

        public int Hue = MIN_HUE;
        public int Saturation = MAX_SATURATION;
        public int Lightness = (MIN_BRIGHTNESS + MAX_BRIGHTNESS) / 2;

        public PaintColor() { }

        public PaintColor(Color? color)
        {
            SetColor(color);
        }

        public Color? GetColor()
        {
            if (!HasColor)
            {
                return null;
            }

            float normalizedLightness = (Lightness - (float)MIN_BRIGHTNESS) / (MAX_BRIGHTNESS - MIN_BRIGHTNESS);
            double lightness = Utility.Lerp((float)LIGHTNESS_FLOOR, (float)LIGHTNESS_CEILING, normalizedLightness);
            Utility.HSLtoRGB(Hue, Saturation / 100f, lightness, out var red, out var green, out var blue);
            return new Color((byte)red, green, blue);
        }

        public void SetColor(Color? color)
        {
            if (color == null)
            {
                HasColor = false;
                return;
            }

            Utility.RGBtoHSL(color.Value.R, color.Value.G, color.Value.B, out double hue, out double saturation, out double lightness);

            HasColor = true;
            Hue = Math.Clamp((int)Math.Round(hue), MIN_HUE, MAX_HUE);
            Saturation = Math.Clamp((int)Math.Round(saturation * 100.0), MIN_SATURATION, MAX_SATURATION);

            double normalizedLightness = (lightness - LIGHTNESS_FLOOR) / (LIGHTNESS_CEILING - LIGHTNESS_FLOOR);
            Lightness = Math.Clamp((int)Math.Round(MIN_BRIGHTNESS + normalizedLightness * (MAX_BRIGHTNESS - MIN_BRIGHTNESS)), MIN_BRIGHTNESS, MAX_BRIGHTNESS);
        }
    }
}