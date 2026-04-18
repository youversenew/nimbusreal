using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  NimbusColor - Immutable ARGB color with utility methods
    //  Inspired by Flutter's Color class
    // ═══════════════════════════════════════════════════════════════════
    public struct NimbusColor
    {
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public NimbusColor(byte r, byte g, byte b) : this()
        {
            A = 255; R = r; G = g; B = b;
        }

        public NimbusColor(byte a, byte r, byte g, byte b) : this()
        {
            A = a; R = r; G = g; B = b;
        }

        /// <summary>Adjust opacity (0.0 = fully transparent, 1.0 = fully opaque)</summary>
        public NimbusColor WithOpacity(double opacity)
        {
            byte a = (byte)(255 * Math.Max(0.0, Math.Min(1.0, opacity)));
            return new NimbusColor(a, R, G, B);
        }

        /// <summary>Lighten the color by a percentage (0.0 - 1.0)</summary>
        public NimbusColor Lighten(double amount)
        {
            amount = Math.Max(0.0, Math.Min(1.0, amount));
            return new NimbusColor(A,
                (byte)Math.Min(255, R + (255 - R) * amount),
                (byte)Math.Min(255, G + (255 - G) * amount),
                (byte)Math.Min(255, B + (255 - B) * amount));
        }

        /// <summary>Darken the color by a percentage (0.0 - 1.0)</summary>
        public NimbusColor Darken(double amount)
        {
            amount = Math.Max(0.0, Math.Min(1.0, amount));
            return new NimbusColor(A,
                (byte)(R * (1.0 - amount)),
                (byte)(G * (1.0 - amount)),
                (byte)(B * (1.0 - amount)));
        }

        /// <summary>Linearly interpolate between two colors</summary>
        public NimbusColor Lerp(NimbusColor other, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));
            return new NimbusColor(
                (byte)(A + (other.A - A) * t),
                (byte)(R + (other.R - R) * t),
                (byte)(G + (other.G - G) * t),
                (byte)(B + (other.B - B) * t));
        }

        /// <summary>Calculate relative luminance for contrast checks</summary>
        public double GetLuminance()
        {
            double r = R / 255.0, g = G / 255.0, b = B / 255.0;
            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>Is this a dark color? Useful for choosing text color</summary>
        public bool IsDark { get { return GetLuminance() < 0.5; } }

        /// <summary>Convert to hex string (#RRGGBB or #AARRGGBB)</summary>
        public string ToHex()
        {
            if (A == 255)
                return string.Format("#{0:X2}{1:X2}{2:X2}", R, G, B);
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", A, R, G, B);
        }

        /// <summary>Parse from hex string (#RGB, #RRGGBB, #AARRGGBB)</summary>
        public static NimbusColor FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return NimbusColors.Black;
            hex = hex.TrimStart('#');

            byte a = 255, r = 0, g = 0, b = 0;
            if (hex.Length == 3)
            {
                r = Convert.ToByte(new string(hex[0], 2), 16);
                g = Convert.ToByte(new string(hex[1], 2), 16);
                b = Convert.ToByte(new string(hex[2], 2), 16);
            }
            else if (hex.Length == 6)
            {
                r = Convert.ToByte(hex.Substring(0, 2), 16);
                g = Convert.ToByte(hex.Substring(2, 2), 16);
                b = Convert.ToByte(hex.Substring(4, 2), 16);
            }
            else if (hex.Length == 8)
            {
                a = Convert.ToByte(hex.Substring(0, 2), 16);
                r = Convert.ToByte(hex.Substring(2, 2), 16);
                g = Convert.ToByte(hex.Substring(4, 2), 16);
                b = Convert.ToByte(hex.Substring(6, 2), 16);
            }
            return new NimbusColor(a, r, g, b);
        }

        public static NimbusColor FromARGB(int a, int r, int g, int b)
        {
            return new NimbusColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public override string ToString() { return ToHex(); }

        public override bool Equals(object obj)
        {
            if (!(obj is NimbusColor)) return false;
            NimbusColor c = (NimbusColor)obj;
            return A == c.A && R == c.R && G == c.G && B == c.B;
        }

        public override int GetHashCode()
        {
            return (A << 24) | (R << 16) | (G << 8) | B;
        }

        public static bool operator ==(NimbusColor a, NimbusColor b) { return a.Equals(b); }
        public static bool operator !=(NimbusColor a, NimbusColor b) { return !a.Equals(b); }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MaterialSwatch - Material Design color swatch with 10 shades
    // ═══════════════════════════════════════════════════════════════════
    public class MaterialSwatch
    {
        private Dictionary<int, NimbusColor> _shades;

        /// <summary>Primary shade (500)</summary>
        public NimbusColor Primary { get { return _shades[500]; } }

        /// <summary>Access specific shade: 50, 100, 200, ..., 900</summary>
        public NimbusColor this[int shade]
        {
            get { return _shades.ContainsKey(shade) ? _shades[shade] : Primary; }
        }

        public MaterialSwatch(string s50, string s100, string s200, string s300, string s400,
            string s500, string s600, string s700, string s800, string s900)
        {
            _shades = new Dictionary<int, NimbusColor>();
            _shades[50]  = NimbusColor.FromHex(s50);
            _shades[100] = NimbusColor.FromHex(s100);
            _shades[200] = NimbusColor.FromHex(s200);
            _shades[300] = NimbusColor.FromHex(s300);
            _shades[400] = NimbusColor.FromHex(s400);
            _shades[500] = NimbusColor.FromHex(s500);
            _shades[600] = NimbusColor.FromHex(s600);
            _shades[700] = NimbusColor.FromHex(s700);
            _shades[800] = NimbusColor.FromHex(s800);
            _shades[900] = NimbusColor.FromHex(s900);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusColors - Full Material Design Color Palette
    //  Static access to all colors like Flutter's Colors class
    // ═══════════════════════════════════════════════════════════════════
    public static class NimbusColors
    {
        // ─────────── BASIC COLORS ───────────
        public static readonly NimbusColor Transparent = new NimbusColor(0, 0, 0, 0);
        public static readonly NimbusColor Black       = new NimbusColor(0, 0, 0);
        public static readonly NimbusColor White       = new NimbusColor(255, 255, 255);
        public static readonly NimbusColor Black87     = new NimbusColor(222, 0, 0, 0);
        public static readonly NimbusColor Black54     = new NimbusColor(138, 0, 0, 0);
        public static readonly NimbusColor Black38     = new NimbusColor(97, 0, 0, 0);
        public static readonly NimbusColor Black12     = new NimbusColor(31, 0, 0, 0);
        public static readonly NimbusColor White70     = new NimbusColor(179, 255, 255, 255);
        public static readonly NimbusColor White54     = new NimbusColor(138, 255, 255, 255);
        public static readonly NimbusColor White38     = new NimbusColor(97, 255, 255, 255);

        // ─────────── MATERIAL SWATCHES ───────────
        public static readonly MaterialSwatch Red = new MaterialSwatch(
            "FFEBEE", "FFCDD2", "EF9A9A", "E57373", "EF5350",
            "F44336", "E53935", "D32F2F", "C62828", "B71C1C");

        public static readonly MaterialSwatch Pink = new MaterialSwatch(
            "FCE4EC", "F8BBD0", "F48FB1", "F06292", "EC407A",
            "E91E63", "D81B60", "C2185B", "AD1457", "880E4F");

        public static readonly MaterialSwatch Purple = new MaterialSwatch(
            "F3E5F5", "E1BEE7", "CE93D8", "BA68C8", "AB47BC",
            "9C27B0", "8E24AA", "7B1FA2", "6A1B9A", "4A148C");

        public static readonly MaterialSwatch DeepPurple = new MaterialSwatch(
            "EDE7F6", "D1C4E9", "B39DDB", "9575CD", "7E57C2",
            "673AB7", "5E35B1", "512DA8", "4527A0", "311B92");

        public static readonly MaterialSwatch Indigo = new MaterialSwatch(
            "E8EAF6", "C5CAE9", "9FA8DA", "7986CB", "5C6BC0",
            "3F51B5", "3949AB", "303F9F", "283593", "1A237E");

        public static readonly MaterialSwatch Blue = new MaterialSwatch(
            "E3F2FD", "BBDEFB", "90CAF9", "64B5F6", "42A5F5",
            "2196F3", "1E88E5", "1976D2", "1565C0", "0D47A1");

        public static readonly MaterialSwatch LightBlue = new MaterialSwatch(
            "E1F5FE", "B3E5FC", "81D4FA", "4FC3F7", "29B6F6",
            "03A9F4", "039BE5", "0288D1", "0277BD", "01579B");

        public static readonly MaterialSwatch Cyan = new MaterialSwatch(
            "E0F7FA", "B2EBF2", "80DEEA", "4DD0E1", "26C6DA",
            "00BCD4", "00ACC1", "0097A7", "00838F", "006064");

        public static readonly MaterialSwatch Teal = new MaterialSwatch(
            "E0F2F1", "B2DFDB", "80CBC4", "4DB6AC", "26A69A",
            "009688", "00897B", "00796B", "00695C", "004D40");

        public static readonly MaterialSwatch Green = new MaterialSwatch(
            "E8F5E9", "C8E6C9", "A5D6A7", "81C784", "66BB6A",
            "4CAF50", "43A047", "388E3C", "2E7D32", "1B5E20");

        public static readonly MaterialSwatch LightGreen = new MaterialSwatch(
            "F1F8E9", "DCEDC8", "C5E1A5", "AED581", "9CCC65",
            "8BC34A", "7CB342", "689F38", "558B2F", "33691E");

        public static readonly MaterialSwatch Lime = new MaterialSwatch(
            "F9FBE7", "F0F4C3", "E6EE9C", "DCE775", "D4E157",
            "CDDC39", "C0CA33", "AFB42B", "9E9D24", "827717");

        public static readonly MaterialSwatch Yellow = new MaterialSwatch(
            "FFFDE7", "FFF9C4", "FFF59D", "FFF176", "FFEE58",
            "FFEB3B", "FDD835", "FBC02D", "F9A825", "F57F17");

        public static readonly MaterialSwatch Amber = new MaterialSwatch(
            "FFF8E1", "FFECB3", "FFE082", "FFD54F", "FFCA28",
            "FFC107", "FFB300", "FFA000", "FF8F00", "FF6F00");

        public static readonly MaterialSwatch Orange = new MaterialSwatch(
            "FFF3E0", "FFE0B2", "FFCC80", "FFB74D", "FFA726",
            "FF9800", "FB8C00", "F57C00", "EF6C00", "E65100");

        public static readonly MaterialSwatch DeepOrange = new MaterialSwatch(
            "FBE9E7", "FFCCBC", "FFAB91", "FF8A65", "FF7043",
            "FF5722", "F4511E", "E64A19", "D84315", "BF360C");

        public static readonly MaterialSwatch Brown = new MaterialSwatch(
            "EFEBE9", "D7CCC8", "BCAAA4", "A1887F", "8D6E63",
            "795548", "6D4C41", "5D4037", "4E342E", "3E2723");

        public static readonly MaterialSwatch Grey = new MaterialSwatch(
            "FAFAFA", "F5F5F5", "EEEEEE", "E0E0E0", "BDBDBD",
            "9E9E9E", "757575", "616161", "424242", "212121");

        public static readonly MaterialSwatch BlueGrey = new MaterialSwatch(
            "ECEFF1", "CFD8DC", "B0BEC5", "90A4AE", "78909C",
            "607D8B", "546E7A", "455A64", "37474F", "263238");

        // ─────────── DARK THEME SURFACE COLORS ───────────
        public static readonly NimbusColor Surface         = NimbusColor.FromHex("121212");
        public static readonly NimbusColor SurfaceVariant  = NimbusColor.FromHex("1E1E1E");
        public static readonly NimbusColor SurfaceContainer = NimbusColor.FromHex("252525");
        public static readonly NimbusColor SurfaceElevated = NimbusColor.FromHex("2D2D30");
        public static readonly NimbusColor SurfaceBright   = NimbusColor.FromHex("3C3C3C");
        public static readonly NimbusColor OnSurface       = NimbusColor.FromHex("E0E0E0");
        public static readonly NimbusColor OnSurfaceVariant = NimbusColor.FromHex("9E9E9E");

        // ─────────── SEMANTIC COLORS ───────────
        public static readonly NimbusColor Error   = NimbusColor.FromHex("CF6679");
        public static readonly NimbusColor Success = NimbusColor.FromHex("4CAF50");
        public static readonly NimbusColor Warning = NimbusColor.FromHex("FFC107");
        public static readonly NimbusColor Info    = NimbusColor.FromHex("2196F3");

        // ─────────── ACCENT / BRAND COLORS ───────────
        public static readonly NimbusColor Accent        = NimbusColor.FromHex("6C63FF");
        public static readonly NimbusColor AccentLight   = NimbusColor.FromHex("9D97FF");
        public static readonly NimbusColor AccentDark    = NimbusColor.FromHex("4A42D4");
        public static readonly NimbusColor PrimaryColor  = NimbusColor.FromHex("BB86FC");
        public static readonly NimbusColor SecondaryColor = NimbusColor.FromHex("03DAC6");

        /// <summary>Get the best contrasting text color (black or white)</summary>
        public static NimbusColor ContrastTextFor(NimbusColor bg)
        {
            return bg.IsDark ? White : Black87;
        }

        /// <summary>Resolve a color name string to NimbusColor</summary>
        public static NimbusColor Resolve(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr)) return Transparent;
            if (colorStr.StartsWith("#")) return NimbusColor.FromHex(colorStr);

            switch (colorStr.ToLower())
            {
                case "transparent": return Transparent;
                case "black": return Black;
                case "white": return White;
                case "red": return Red.Primary;
                case "pink": return Pink.Primary;
                case "purple": return Purple.Primary;
                case "deeppurple": return DeepPurple.Primary;
                case "indigo": return Indigo.Primary;
                case "blue": return Blue.Primary;
                case "lightblue": return LightBlue.Primary;
                case "cyan": return Cyan.Primary;
                case "teal": return Teal.Primary;
                case "green": return Green.Primary;
                case "lightgreen": return LightGreen.Primary;
                case "lime": return Lime.Primary;
                case "yellow": return Yellow.Primary;
                case "amber": return Amber.Primary;
                case "orange": return Orange.Primary;
                case "deeporange": return DeepOrange.Primary;
                case "brown": return Brown.Primary;
                case "grey": case "gray": return Grey.Primary;
                case "bluegrey": case "bluegray": return BlueGrey.Primary;
                case "surface": return Surface;
                case "accent": return Accent;
                case "primary": return PrimaryColor;
                case "secondary": return SecondaryColor;
                case "error": return Error;
                case "success": return Success;
                case "warning": return Warning;
                case "info": return Info;
                default: return NimbusColor.FromHex(colorStr);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusGradient - Linear / Radial gradient definitions
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusGradient
    {
        public string Type { get; set; }            // "linear", "radial"
        public string Direction { get; set; }       // "topBottom", "leftRight", "diagonal", angle in degrees
        public List<GradientStop> Stops { get; set; }

        public NimbusGradient()
        {
            Type = "linear";
            Direction = "topBottom";
            Stops = new List<GradientStop>();
        }

        public static NimbusGradient Linear(NimbusColor start, NimbusColor end, string direction)
        {
            NimbusGradient g = new NimbusGradient();
            g.Type = "linear";
            g.Direction = direction ?? "topBottom";
            g.Stops.Add(new GradientStop(start, 0.0));
            g.Stops.Add(new GradientStop(end, 1.0));
            return g;
        }

        public static NimbusGradient Radial(NimbusColor center, NimbusColor edge)
        {
            NimbusGradient g = new NimbusGradient();
            g.Type = "radial";
            g.Stops.Add(new GradientStop(center, 0.0));
            g.Stops.Add(new GradientStop(edge, 1.0));
            return g;
        }

        /// <summary>Parse gradient from string: "linear(#FF0000,#0000FF,leftRight)"</summary>
        public static NimbusGradient Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            NimbusGradient g = new NimbusGradient();
            string inner = value;

            if (value.StartsWith("linear(") && value.EndsWith(")"))
            {
                g.Type = "linear";
                inner = value.Substring(7, value.Length - 8);
            }
            else if (value.StartsWith("radial(") && value.EndsWith(")"))
            {
                g.Type = "radial";
                inner = value.Substring(7, value.Length - 8);
            }

            string[] parts = inner.Split(',');
            if (parts.Length >= 2)
            {
                g.Stops.Add(new GradientStop(NimbusColor.FromHex(parts[0].Trim()), 0.0));
                g.Stops.Add(new GradientStop(NimbusColor.FromHex(parts[1].Trim()), 1.0));
            }
            if (parts.Length >= 3)
            {
                g.Direction = parts[2].Trim();
            }

            return g;
        }
    }

    public struct GradientStop
    {
        public NimbusColor Color { get; private set; }
        public double Offset { get; private set; }

        public GradientStop(NimbusColor color, double offset) : this()
        {
            Color = color;
            Offset = offset;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusThemeData - Theme configuration like Flutter's ThemeData
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusThemeData
    {
        public NimbusColor PrimaryColor { get; set; }
        public NimbusColor SecondaryColor { get; set; }
        public NimbusColor BackgroundColor { get; set; }
        public NimbusColor SurfaceColor { get; set; }
        public NimbusColor ErrorColor { get; set; }
        public NimbusColor OnPrimary { get; set; }
        public NimbusColor OnSecondary { get; set; }
        public NimbusColor OnBackground { get; set; }
        public NimbusColor OnSurface { get; set; }
        public NimbusColor OnError { get; set; }
        public NimbusColor DividerColor { get; set; }
        public NimbusColor DisabledColor { get; set; }
        public NimbusColor HintColor { get; set; }
        public string FontFamily { get; set; }
        public double DefaultFontSize { get; set; }
        public double DefaultCornerRadius { get; set; }
        public double DefaultElevation { get; set; }
        public bool IsDark { get; set; }

        /// <summary>Default dark theme (Material You inspired)</summary>
        public static NimbusThemeData DarkTheme()
        {
            NimbusThemeData theme = new NimbusThemeData();
            theme.PrimaryColor = NimbusColors.PrimaryColor;
            theme.SecondaryColor = NimbusColors.SecondaryColor;
            theme.BackgroundColor = NimbusColors.Surface;
            theme.SurfaceColor = NimbusColors.SurfaceVariant;
            theme.ErrorColor = NimbusColors.Error;
            theme.OnPrimary = NimbusColors.Black;
            theme.OnSecondary = NimbusColors.Black;
            theme.OnBackground = NimbusColors.OnSurface;
            theme.OnSurface = NimbusColors.OnSurface;
            theme.OnError = NimbusColors.Black;
            theme.DividerColor = NimbusColors.White.WithOpacity(0.12);
            theme.DisabledColor = NimbusColors.White.WithOpacity(0.38);
            theme.HintColor = NimbusColors.OnSurfaceVariant;
            theme.FontFamily = "Segoe UI";
            theme.DefaultFontSize = 14;
            theme.DefaultCornerRadius = 8;
            theme.DefaultElevation = 2;
            theme.IsDark = true;
            return theme;
        }

        /// <summary>Default light theme</summary>
        public static NimbusThemeData LightTheme()
        {
            NimbusThemeData theme = new NimbusThemeData();
            theme.PrimaryColor = NimbusColors.Indigo.Primary;
            theme.SecondaryColor = NimbusColors.Teal.Primary;
            theme.BackgroundColor = NimbusColors.Grey[50];
            theme.SurfaceColor = NimbusColors.White;
            theme.ErrorColor = NimbusColors.Red[700];
            theme.OnPrimary = NimbusColors.White;
            theme.OnSecondary = NimbusColors.White;
            theme.OnBackground = NimbusColors.Black87;
            theme.OnSurface = NimbusColors.Black87;
            theme.OnError = NimbusColors.White;
            theme.DividerColor = NimbusColors.Black12;
            theme.DisabledColor = NimbusColors.Black38;
            theme.HintColor = NimbusColors.Black54;
            theme.FontFamily = "Segoe UI";
            theme.DefaultFontSize = 14;
            theme.DefaultCornerRadius = 8;
            theme.DefaultElevation = 2;
            theme.IsDark = false;
            return theme;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EdgeInsets - Padding/Margin values (like Flutter)
    // ═══════════════════════════════════════════════════════════════════
    public class EdgeInsets
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        public double Horizontal { get { return Left + Right; } }
        public double Vertical { get { return Top + Bottom; } }

        private EdgeInsets() { }

        public static EdgeInsets All(double value)
        {
            EdgeInsets e = new EdgeInsets();
            e.Left = value; e.Top = value; e.Right = value; e.Bottom = value;
            return e;
        }

        public static EdgeInsets Symmetric(double vertical, double horizontal)
        {
            EdgeInsets e = new EdgeInsets();
            e.Left = horizontal; e.Right = horizontal;
            e.Top = vertical; e.Bottom = vertical;
            return e;
        }

        public static EdgeInsets Only(double left, double top, double right, double bottom)
        {
            EdgeInsets e = new EdgeInsets();
            e.Left = left; e.Top = top; e.Right = right; e.Bottom = bottom;
            return e;
        }

        public static EdgeInsets Zero { get { return All(0); } }

        /// <summary>Parse "8" or "8,16" or "8,16,8,16" into EdgeInsets</summary>
        public static EdgeInsets Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return Zero;

            string[] parts = value.Split(',');
            double v1, v2, v3, v4;

            if (parts.Length == 1 && double.TryParse(parts[0].Trim(), out v1))
                return All(v1);
            if (parts.Length == 2 && double.TryParse(parts[0].Trim(), out v1)
                                 && double.TryParse(parts[1].Trim(), out v2))
                return Symmetric(v1, v2);
            if (parts.Length == 4 && double.TryParse(parts[0].Trim(), out v1)
                                 && double.TryParse(parts[1].Trim(), out v2)
                                 && double.TryParse(parts[2].Trim(), out v3)
                                 && double.TryParse(parts[3].Trim(), out v4))
                return Only(v1, v2, v3, v4);

            return Zero;
        }

        public override string ToString()
        {
            if (Left == Top && Top == Right && Right == Bottom)
                return Left.ToString();
            return string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  BoxDecoration - Visual decoration for containers (like Flutter)
    // ═══════════════════════════════════════════════════════════════════
    public class BoxDecoration
    {
        public NimbusColor Color { get; set; }
        public NimbusGradient Gradient { get; set; }
        public double CornerRadius { get; set; }
        public NimbusColor BorderColor { get; set; }
        public double BorderWidth { get; set; }
        public List<BoxShadow> Shadows { get; set; }

        public BoxDecoration()
        {
            Color = NimbusColors.Transparent;
            Gradient = null;
            CornerRadius = 0;
            BorderColor = NimbusColors.Transparent;
            BorderWidth = 0;
            Shadows = new List<BoxShadow>();
        }
    }

    public class BoxShadow
    {
        public NimbusColor Color { get; set; }
        public double BlurRadius { get; set; }
        public double SpreadRadius { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public BoxShadow()
        {
            Color = NimbusColors.Black.WithOpacity(0.3);
            BlurRadius = 4;
            SpreadRadius = 0;
            OffsetX = 0;
            OffsetY = 2;
        }

        /// <summary>Standard elevation shadow</summary>
        public static BoxShadow Elevation(double elevation)
        {
            BoxShadow s = new BoxShadow();
            s.BlurRadius = elevation * 2;
            s.SpreadRadius = 0;
            s.OffsetY = elevation;
            s.Color = NimbusColors.Black.WithOpacity(0.15 + elevation * 0.02);
            return s;
        }
    }
}
