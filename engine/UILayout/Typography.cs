using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  NimbusTextStyle - Predefined text styles (Material Typography)
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusTextStyle
    {
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontWeight { get; set; }
        public double LetterSpacing { get; set; }
        public double LineHeight { get; set; }
        public string Color { get; set; }

        public NimbusTextStyle()
        {
            FontFamily = "Segoe UI";
            FontSize = 14;
            FontWeight = "Normal";
            LetterSpacing = 0;
            LineHeight = 1.5;
            Color = "#E0E0E0";
        }

        // ─────────── MATERIAL DESIGN TYPE SCALE ───────────
        
        public static NimbusTextStyle DisplayLarge()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 57; s.FontWeight = "Bold"; s.LetterSpacing = -0.25; s.LineHeight = 1.12;
            return s;
        }

        public static NimbusTextStyle DisplayMedium()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 45; s.FontWeight = "Bold"; s.LineHeight = 1.16;
            return s;
        }

        public static NimbusTextStyle DisplaySmall()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 36; s.FontWeight = "Bold"; s.LineHeight = 1.22;
            return s;
        }

        public static NimbusTextStyle HeadlineLarge()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 32; s.FontWeight = "SemiBold"; s.LineHeight = 1.25;
            return s;
        }

        public static NimbusTextStyle HeadlineMedium()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 28; s.FontWeight = "SemiBold"; s.LineHeight = 1.29;
            return s;
        }

        public static NimbusTextStyle HeadlineSmall()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 24; s.FontWeight = "SemiBold"; s.LineHeight = 1.33;
            return s;
        }

        public static NimbusTextStyle TitleLarge()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 22; s.FontWeight = "SemiBold"; s.LineHeight = 1.27;
            return s;
        }

        public static NimbusTextStyle TitleMedium()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 16; s.FontWeight = "Medium"; s.LetterSpacing = 0.15; s.LineHeight = 1.5;
            return s;
        }

        public static NimbusTextStyle TitleSmall()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 14; s.FontWeight = "Medium"; s.LetterSpacing = 0.1; s.LineHeight = 1.43;
            return s;
        }

        public static NimbusTextStyle BodyLarge()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 16; s.FontWeight = "Normal"; s.LetterSpacing = 0.5; s.LineHeight = 1.5;
            return s;
        }

        public static NimbusTextStyle BodyMedium()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 14; s.FontWeight = "Normal"; s.LetterSpacing = 0.25; s.LineHeight = 1.43;
            return s;
        }

        public static NimbusTextStyle BodySmall()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 12; s.FontWeight = "Normal"; s.LetterSpacing = 0.4; s.LineHeight = 1.33;
            return s;
        }

        public static NimbusTextStyle LabelLarge()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 14; s.FontWeight = "Medium"; s.LetterSpacing = 0.1; s.LineHeight = 1.43;
            return s;
        }

        public static NimbusTextStyle LabelMedium()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 12; s.FontWeight = "Medium"; s.LetterSpacing = 0.5; s.LineHeight = 1.33;
            return s;
        }

        public static NimbusTextStyle LabelSmall()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 11; s.FontWeight = "Medium"; s.LetterSpacing = 0.5; s.LineHeight = 1.45;
            return s;
        }

        public static NimbusTextStyle Caption()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 10; s.FontWeight = "Normal"; s.LetterSpacing = 0.4; s.LineHeight = 1.4;
            s.Color = "#9E9E9E";
            return s;
        }

        public static NimbusTextStyle Overline()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontSize = 10; s.FontWeight = "Medium"; s.LetterSpacing = 1.5; s.LineHeight = 1.6;
            s.Color = "#9E9E9E";
            return s;
        }

        public static NimbusTextStyle Code()
        {
            NimbusTextStyle s = new NimbusTextStyle();
            s.FontFamily = "Consolas";
            s.FontSize = 13; s.FontWeight = "Normal"; s.LetterSpacing = 0; s.LineHeight = 1.5;
            s.Color = "#CE9178";
            return s;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusTypography - Helper to apply text styles to elements
    // ═══════════════════════════════════════════════════════════════════
    public static class NimbusTypography
    {
        /// <summary>Apply a NimbusTextStyle to a ModuleUIElement</summary>
        public static void Apply(ModuleUIElement element, NimbusTextStyle style)
        {
            if (element == null || style == null) return;
            element.FontFamily = style.FontFamily;
            element.FontSize = style.FontSize;
            element.FontWeight = style.FontWeight;
            element.Foreground = style.Color;
        }

        /// <summary>
        /// Resolve a typography style name to NimbusTextStyle
        /// Used with style="displayLarge" etc. in XML
        /// </summary>
        public static NimbusTextStyle Resolve(string styleName)
        {
            if (string.IsNullOrEmpty(styleName)) return null;

            switch (styleName.ToLower())
            {
                case "displaylarge": return NimbusTextStyle.DisplayLarge();
                case "displaymedium": return NimbusTextStyle.DisplayMedium();
                case "displaysmall": return NimbusTextStyle.DisplaySmall();
                case "headlinelarge": return NimbusTextStyle.HeadlineLarge();
                case "headlinemedium": return NimbusTextStyle.HeadlineMedium();
                case "headlinesmall": return NimbusTextStyle.HeadlineSmall();
                case "titlelarge": return NimbusTextStyle.TitleLarge();
                case "titlemedium": return NimbusTextStyle.TitleMedium();
                case "titlesmall": return NimbusTextStyle.TitleSmall();
                case "bodylarge": return NimbusTextStyle.BodyLarge();
                case "bodymedium": return NimbusTextStyle.BodyMedium();
                case "bodysmall": return NimbusTextStyle.BodySmall();
                case "labellarge": return NimbusTextStyle.LabelLarge();
                case "labelmedium": return NimbusTextStyle.LabelMedium();
                case "labelsmall": return NimbusTextStyle.LabelSmall();
                case "caption": return NimbusTextStyle.Caption();
                case "overline": return NimbusTextStyle.Overline();
                case "code": return NimbusTextStyle.Code();
                default: return null;
            }
        }
    }
}
