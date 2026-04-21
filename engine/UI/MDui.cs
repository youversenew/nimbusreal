using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace Nimbus.MDUI
{
    /// <summary>
    /// Material Design UI Framework - Base Classes and Core Components
    /// GDI+ orqali to'liq WPF ga o'xshash UI library
    /// </summary>

    #region Enumerations and Constants

    /// <summary>
    /// Material Design elevation levels
    /// </summary>
    public enum ElevationLevel
    {
        None = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Level6 = 6,
        Level8 = 8,
        Level12 = 12,
        Level16 = 16,
        Level24 = 24
    }

    /// <summary>
    /// Button types in Material Design
    /// </summary>
    public enum MDButtonType
    {
        Contained,
        Outlined,
        Text,
        Elevated,
        Tonal
    }

    /// <summary>
    /// Size variants
    /// </summary>
    public enum MDSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// Animation easing functions
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseInOut,
        EaseIn,
        EaseOut,
        Cubic
    }

    #endregion

    #region Color and Style Management

    /// <summary>
    /// Material Design color palette
    /// </summary>
    public static class MDColors
    {
        // Primary Colors
        public static Color Primary = Color.FromArgb(103, 58, 183);
        public static Color PrimaryLight = Color.FromArgb(156, 39, 176);
        public static Color PrimaryDark = Color.FromArgb(63, 81, 181);

        // Secondary Colors
        public static Color Secondary = Color.FromArgb(0, 188, 212);
        public static Color SecondaryLight = Color.FromArgb(77, 182, 172);
        public static Color SecondaryDark = Color.FromArgb(0, 150, 136);

        // Neutral Colors
        public static Color Surface = Color.FromArgb(245, 245, 245);
        public static Color SurfaceVariant = Color.FromArgb(230, 230, 230);
        public static Color Background = Color.White;
        public static Color OnBackground = Color.FromArgb(28, 27, 31);
        public static Color OnSurface = Color.FromArgb(28, 27, 31);

        // Semantic Colors
        public static Color Error = Color.FromArgb(179, 38, 30);
        public static Color ErrorLight = Color.FromArgb(245, 125, 120);
        public static Color Success = Color.FromArgb(56, 142, 60);
        public static Color Warning = Color.FromArgb(251, 188, 5);
        public static Color Info = Color.FromArgb(13, 110, 253);

        // Text Colors
        public static Color TextPrimary = Color.FromArgb(28, 27, 31);
        public static Color TextSecondary = Color.FromArgb(119, 118, 125);
        public static Color TextDisabled = Color.FromArgb(198, 197, 203);

        // Overlay Colors
        public static Color Overlay = Color.FromArgb(180, 0, 0, 0);
        public static Color SurfaceOverlay = Color.FromArgb(200, 103, 58, 183);
    }

    /// <summary>
    /// Material Design typography
    /// </summary>
    public static class MDTypography
    {
        public static Font DisplayLarge => new Font("Segoe UI", 57, FontStyle.Regular);
        public static Font DisplayMedium => new Font("Segoe UI", 45, FontStyle.Regular);
        public static Font DisplaySmall => new Font("Segoe UI", 36, FontStyle.Regular);

        public static Font HeadlineLarge => new Font("Segoe UI", 32, FontStyle.Regular);
        public static Font HeadlineMedium => new Font("Segoe UI", 28, FontStyle.Regular);
        public static Font HeadlineSmall => new Font("Segoe UI", 24, FontStyle.Regular);

        public static Font TitleLarge => new Font("Segoe UI", 22, FontStyle.Regular);
        public static Font TitleMedium => new Font("Segoe UI", 16, FontStyle.SemiBold);
        public static Font TitleSmall => new Font("Segoe UI", 14, FontStyle.SemiBold);

        public static Font BodyLarge => new Font("Segoe UI", 16, FontStyle.Regular);
        public static Font BodyMedium => new Font("Segoe UI", 14, FontStyle.Regular);
        public static Font BodySmall => new Font("Segoe UI", 12, FontStyle.Regular);

        public static Font LabelLarge => new Font("Segoe UI", 14, FontStyle.SemiBold);
        public static Font LabelMedium => new Font("Segoe UI", 12, FontStyle.SemiBold);
        public static Font LabelSmall => new Font("Segoe UI", 11, FontStyle.SemiBold);
    }

    #endregion

    #region Base Classes

    /// <summary>
    /// Base class for all Material Design UI elements
    /// </summary>
    public abstract class MDElement
    {
        public string Name { get; set; }
        public Rectangle Bounds { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public object Tag { get; set; }
        
        public Color BackgroundColor { get; set; } = MDColors.Background;
        public Color ForeColor { get; set; } = MDColors.TextPrimary;
        public ElevationLevel Elevation { get; set; } = ElevationLevel.None;
        
        public Padding Padding { get; set; } = new Padding(16);
        public Padding Margin { get; set; } = new Padding(0);

        public event EventHandler VisibleChanged;
        public event EventHandler EnabledChanged;
        public event EventHandler SizeChanged;

        public virtual void Draw(Graphics g)
        {
            if (!Visible) return;
        }

        public virtual bool HitTest(Point point)
        {
            return Bounds.Contains(point);
        }

        public virtual void OnMouseDown(MouseEventArgs e) { }
        public virtual void OnMouseUp(MouseEventArgs e) { }
        public virtual void OnMouseMove(MouseEventArgs e) { }
        public virtual void OnMouseEnter() { }
        public virtual void OnMouseLeave() { }
        public virtual void OnKeyDown(KeyEventArgs e) { }
        public virtual void OnKeyUp(KeyEventArgs e) { }
        public virtual void OnTextInput(string text) { }

        protected void RaiseVisibleChanged()
        {
            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void RaiseEnabledChanged()
        {
            EnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void RaiseSizeChanged()
        {
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Base class for container elements
    /// </summary>
    public abstract class MDContainer : MDElement
    {
        protected List<MDElement> _children = new List<MDElement>();
        public IReadOnlyList<MDElement> Children => _children.AsReadOnly();

        public virtual void AddChild(MDElement child)
        {
            if (child != null && !_children.Contains(child))
            {
                _children.Add(child);
                child.SizeChanged += (s, e) => InvalidateLayout();
            }
        }

        public virtual void RemoveChild(MDElement child)
        {
            _children.Remove(child);
        }

        public virtual void ClearChildren()
        {
            _children.Clear();
        }

        protected virtual void InvalidateLayout()
        {
            LayoutChildren();
        }

        protected virtual void LayoutChildren()
        {
            // Override in derived classes
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            foreach (var child in _children)
            {
                child.Draw(g);
            }
        }
    }

    /// <summary>
    /// Base class for interactive elements
    /// </summary>
    public abstract class MDControl : MDElement
    {
        protected bool _isHovered = false;
        protected bool _isPressed = false;
        protected bool _isFocused = false;

        public bool IsHovered => _isHovered;
        public bool IsPressed => _isPressed;
        public bool IsFocused => _isFocused;

        public event EventHandler Click;
        public event EventHandler DoubleClick;
        public event EventHandler FocusChanged;
        public event EventHandler HoverChanged;

        public override void OnMouseEnter()
        {
            _isHovered = true;
            HoverChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void OnMouseLeave()
        {
            _isHovered = false;
            _isPressed = false;
            HoverChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;
            _isPressed = true;
            _isFocused = true;
            FocusChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (_isPressed && _isHovered)
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
            _isPressed = false;
        }

        protected void RaiseClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Layout Components

    /// <summary>
    /// Panel component - container with background
    /// </summary>
    public class MDPanel : MDContainer
    {
        public int CornerRadius { get; set; } = 12;
        public Color BorderColor { get; set; } = MDColors.SurfaceVariant;
        public int BorderWidth { get; set; } = 1;

        public MDPanel()
        {
            BackgroundColor = MDColors.Surface;
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow if elevation
            if (Elevation != ElevationLevel.None)
            {
                DrawShadow(g, Bounds, (int)Elevation);
            }

            // Draw background
            using (var brush = new SolidBrush(BackgroundColor))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }

            // Draw border
            if (BorderWidth > 0)
            {
                using (var pen = new Pen(BorderColor, BorderWidth))
                {
                    var path = GetRoundedRectangle(Bounds, CornerRadius);
                    g.DrawPath(pen, path);
                }
            }

            base.Draw(g);
        }

        private void DrawShadow(Graphics g, Rectangle bounds, int elevation)
        {
            int shadowBlur = elevation / 2 + 2;
            int shadowOffset = elevation / 3 + 1;

            var shadowColor = Color.FromArgb((int)(255 * (elevation / 30.0)), 0, 0, 0);
            var shadowRect = new Rectangle(
                bounds.X + shadowOffset,
                bounds.Y + shadowOffset,
                bounds.Width,
                bounds.Height);

            using (var brush = new SolidBrush(shadowColor))
            {
                var path = GetRoundedRectangle(shadowRect, CornerRadius);
                g.FillPath(brush, path);
            }
        }

        protected static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int x = bounds.X;
            int y = bounds.Y;
            int width = bounds.Width;
            int height = bounds.Height;
            int r = Math.Min(radius, Math.Min(width / 2, height / 2));

            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    /// <summary>
    /// Linear layout container - arranges children horizontally or vertically
    /// </summary>
    public class MDLinearLayout : MDContainer
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public Orientation LayoutOrientation { get; set; } = Orientation.Vertical;
        public int Spacing { get; set; } = 8;
        public ContentAlignment Alignment { get; set; } = ContentAlignment.TopLeft;

        protected override void LayoutChildren()
        {
            int x = Bounds.X + Padding.Left;
            int y = Bounds.Y + Padding.Top;
            int maxWidth = Bounds.Width - Padding.Horizontal;
            int maxHeight = Bounds.Height - Padding.Vertical;

            if (LayoutOrientation == Orientation.Vertical)
            {
                foreach (var child in _children)
                {
                    if (!child.Visible) continue;

                    child.Bounds = new Rectangle(x, y, maxWidth - Padding.Right, child.Bounds.Height);
                    y += child.Bounds.Height + Spacing;
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    if (!child.Visible) continue;

                    child.Bounds = new Rectangle(x, y, child.Bounds.Width, maxHeight - Padding.Bottom);
                    x += child.Bounds.Width + Spacing;
                }
            }
        }
    }

    #endregion

    #region Interactive Components

    /// <summary>
    /// Material Design Button
    /// </summary>
    public class MDButton : MDControl
    {
        public string Text { get; set; }
        public MDButtonType ButtonType { get; set; } = MDButtonType.Contained;
        public MDSize Size { get; set; } = MDSize.Medium;
        public int CornerRadius { get; set; } = 20;
        public bool IsEnable { get; set; } = true;

        public Color ContainedColor { get; set; } = MDColors.Primary;
        public Color ContainedHoverColor { get; set; } = MDColors.PrimaryLight;
        public Color TextColor { get; set; } = Color.White;
        public Font TextFont { get; set; } = MDTypography.LabelLarge;

        public MDButton()
        {
            Bounds = new Rectangle(0, 0, 200, 48);
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (ButtonType)
            {
                case MDButtonType.Contained:
                    DrawContainedButton(g);
                    break;
                case MDButtonType.Outlined:
                    DrawOutlinedButton(g);
                    break;
                case MDButtonType.Text:
                    DrawTextButton(g);
                    break;
                case MDButtonType.Elevated:
                    DrawElevatedButton(g);
                    break;
                case MDButtonType.Tonal:
                    DrawTonalButton(g);
                    break;
            }

            DrawText(g);
        }

        private void DrawContainedButton(Graphics g)
        {
            Color bgColor = !Enabled ? MDColors.TextDisabled : (_isPressed ? ContainedHoverColor : ContainedColor);

            using (var brush = new SolidBrush(bgColor))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }

            if (_isHovered && Enabled)
            {
                DrawRipple(g);
            }
        }

        private void DrawOutlinedButton(Graphics g)
        {
            Color borderColor = !Enabled ? MDColors.TextDisabled : (_isPressed ? ContainedColor : MDColors.Primary);

            using (var brush = new SolidBrush(MDColors.Background))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }

            using (var pen = new Pen(borderColor, 2))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.DrawPath(pen, path);
            }
        }

        private void DrawTextButton(Graphics g)
        {
            if (_isHovered && Enabled)
            {
                using (var brush = new SolidBrush(Color.FromArgb(30, MDColors.Primary)))
                {
                    var path = GetRoundedRectangle(Bounds, CornerRadius);
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawElevatedButton(Graphics g)
        {
            DrawShadow(g, Bounds, 2);

            using (var brush = new SolidBrush(MDColors.Surface))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }
        }

        private void DrawTonalButton(Graphics g)
        {
            Color bgColor = !Enabled ? MDColors.TextDisabled : Color.FromArgb(200, MDColors.Primary);

            using (var brush = new SolidBrush(bgColor))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }
        }

        private void DrawText(Graphics g)
        {
            Color textColor = !Enabled ? MDColors.TextDisabled : TextColor;

            using (var brush = new SolidBrush(textColor))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(Text ?? "", TextFont, brush, Bounds, stringFormat);
            }
        }

        private void DrawRipple(Graphics g)
        {
            int rippleRadius = Math.Max(Bounds.Width, Bounds.Height) / 2;
            var rippleColor = Color.FromArgb(50, Color.White);

            using (var brush = new SolidBrush(rippleColor))
            {
                g.FillEllipse(brush, 
                    Bounds.X + Bounds.Width / 2 - rippleRadius,
                    Bounds.Y + Bounds.Height / 2 - rippleRadius,
                    rippleRadius * 2,
                    rippleRadius * 2);
            }
        }

        private void DrawShadow(Graphics g, Rectangle bounds, int elevation)
        {
            int shadowBlur = elevation + 2;
            var shadowColor = Color.FromArgb(30, 0, 0, 0);
            var shadowRect = new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height);

            using (var brush = new SolidBrush(shadowColor))
            {
                var path = GetRoundedRectangle(shadowRect, CornerRadius);
                g.FillPath(brush, path);
            }
        }

        protected static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int x = bounds.X;
            int y = bounds.Y;
            int width = bounds.Width;
            int height = bounds.Height;
            int r = Math.Min(radius, Math.Min(width / 2, height / 2));

            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    /// <summary>
    /// Material Design Label/Text
    /// </summary>
    public class MDLabel : MDElement
    {
        public string Text { get; set; }
        public Font TextFont { get; set; } = MDTypography.BodyMedium;
        public bool AutoSize { get; set; } = true;

        public override void Draw(Graphics g)
        {
            if (!Visible || string.IsNullOrEmpty(Text)) return;

            using (var brush = new SolidBrush(ForeColor))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Top,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                if (AutoSize)
                {
                    var size = g.MeasureString(Text, TextFont);
                    Bounds = new Rectangle(Bounds.X, Bounds.Y, (int)size.Width, (int)size.Height);
                }

                g.DrawString(Text, TextFont, brush, Bounds, stringFormat);
            }
        }
    }

    /// <summary>
    /// Material Design Text Input
    /// </summary>
    public class MDTextBox : MDControl
    {
        public string Text { get; set; } = string.Empty;
        public string Placeholder { get; set; }
        public Font TextFont { get; set; } = MDTypography.BodyMedium;
        public int CornerRadius { get; set; } = 8;
        public bool IsPassword { get; set; }

        public Color OutlineColor { get; set; } = MDColors.SurfaceVariant;
        public Color OutlineFocusColor { get; set; } = MDColors.Primary;
        public int OutlineWidth { get; set; } = 1;

        public MDTextBox()
        {
            Bounds = new Rectangle(0, 0, 300, 56);
            BackgroundColor = MDColors.Surface;
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (var brush = new SolidBrush(BackgroundColor))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }

            // Draw outline
            Color outlineColor = _isFocused ? OutlineFocusColor : OutlineColor;
            using (var pen = new Pen(outlineColor, OutlineWidth))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.DrawPath(pen, path);
            }

            // Draw text
            int textX = Bounds.X + 16;
            int textY = Bounds.Y + (Bounds.Height - (int)g.MeasureString("A", TextFont).Height) / 2;

            string displayText = IsPassword ? new string('•', Text.Length) : Text;
            if (string.IsNullOrEmpty(displayText))
            {
                using (var brush = new SolidBrush(MDColors.TextSecondary))
                {
                    g.DrawString(Placeholder ?? "", TextFont, brush, textX, textY);
                }
            }
            else
            {
                using (var brush = new SolidBrush(ForeColor))
                {
                    g.DrawString(displayText, TextFont, brush, textX, textY);
                }
            }
        }

        public override void OnTextInput(string text)
        {
            if (!Enabled || !_isFocused) return;
            Text += text;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (!Enabled || !_isFocused) return;

            if (e.KeyCode == Keys.Back && Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
        }

        protected static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int x = bounds.X;
            int y = bounds.Y;
            int width = bounds.Width;
            int height = bounds.Height;
            int r = Math.Min(radius, Math.Min(width / 2, height / 2));

            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    #endregion

    #region Advanced Components

    /// <summary>
    /// Checkbox component
    /// </summary>
    public class MDCheckBox : MDControl
    {
        public bool Checked { get; set; }
        public string Text { get; set; }
        public Font TextFont { get; set; } = MDTypography.BodyMedium;

        public MDCheckBox()
        {
            Bounds = new Rectangle(0, 0, 200, 40);
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw checkbox box
            var checkBoxRect = new Rectangle(Bounds.X, Bounds.Y + 8, 24, 24);
            Color boxColor = !Enabled ? MDColors.TextDisabled : (_isPressed ? MDColors.PrimaryLight : MDColors.Primary);

            if (Checked)
            {
                using (var brush = new SolidBrush(boxColor))
                {
                    g.FillRectangle(brush, checkBoxRect);
                }

                // Draw checkmark
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawLine(pen, checkBoxRect.X + 6, checkBoxRect.Y + 12, checkBoxRect.X + 10, checkBoxRect.Y + 16);
                    g.DrawLine(pen, checkBoxRect.X + 10, checkBoxRect.Y + 16, checkBoxRect.X + 18, checkBoxRect.Y + 8);
                }
            }
            else
            {
                using (var pen = new Pen(boxColor, 2))
                {
                    g.DrawRectangle(pen, checkBoxRect);
                }
            }

            // Draw text
            if (!string.IsNullOrEmpty(Text))
            {
                using (var brush = new SolidBrush(ForeColor))
                {
                    g.DrawString(Text, TextFont, brush, Bounds.X + 32, Bounds.Y + 12);
                }
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!Enabled) return;
            Checked = !Checked;
            RaiseClick();
            base.OnMouseUp(e);
        }
    }

    /// <summary>
    /// Slider component
    /// </summary>
    public class MDSlider : MDControl
    {
        public float Value { get; set; }
        public float MinValue { get; set; } = 0;
        public float MaxValue { get; set; } = 100;
        public bool ShowLabel { get; set; } = true;

        public MDSlider()
        {
            Bounds = new Rectangle(0, 0, 300, 40);
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw track
            var trackRect = new Rectangle(Bounds.X + 16, Bounds.Y + 18, Bounds.Width - 32, 4);
            using (var brush = new SolidBrush(MDColors.SurfaceVariant))
            {
                g.FillRectangle(brush, trackRect);
            }

            // Draw progress
            float progress = (Value - MinValue) / (MaxValue - MinValue);
            var progressRect = new Rectangle(trackRect.X, trackRect.Y, (int)(trackRect.Width * progress), trackRect.Height);
            using (var brush = new SolidBrush(MDColors.Primary))
            {
                g.FillRectangle(brush, progressRect);
            }

            // Draw thumb
            int thumbX = trackRect.X + (int)(trackRect.Width * progress);
            var thumbRect = new Rectangle(thumbX - 8, Bounds.Y + 8, 16, 16);
            using (var brush = new SolidBrush(MDColors.Primary))
            {
                g.FillEllipse(brush, thumbRect);
            }

            // Draw label
            if (ShowLabel)
            {
                using (var brush = new SolidBrush(ForeColor))
                {
                    g.DrawString($"{Value:F0}", MDTypography.LabelSmall, brush, Bounds.X + Bounds.Width - 30, Bounds.Y);
                }
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!_isPressed || !Enabled) return;

            var trackRect = new Rectangle(Bounds.X + 16, Bounds.Y + 18, Bounds.Width - 32, 4);
            float progress = Math.Max(0, Math.Min(1, (float)(e.X - trackRect.X) / trackRect.Width));
            Value = MinValue + progress * (MaxValue - MinValue);
        }
    }

    /// <summary>
    /// Progress bar component
    /// </summary>
    public class MDProgressBar : MDElement
    {
        public float Value { get; set; }
        public float MaxValue { get; set; } = 100;
        public bool Indeterminate { get; set; }
        public int CornerRadius { get; set; } = 4;

        public MDProgressBar()
        {
            Bounds = new Rectangle(0, 0, 300, 4);
        }

        public override void Draw(Graphics g)
        {
            if (!Visible) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (var brush = new SolidBrush(MDColors.SurfaceVariant))
            {
                var path = GetRoundedRectangle(Bounds, CornerRadius);
                g.FillPath(brush, path);
            }

            // Draw progress
            if (!Indeterminate)
            {
                float progress = Math.Min(1, Value / MaxValue);
                var progressRect = new Rectangle(Bounds.X, Bounds.Y, (int)(Bounds.Width * progress), Bounds.Height);
                using (var brush = new SolidBrush(MDColors.Primary))
                {
                    var path = GetRoundedRectangle(progressRect, CornerRadius);
                    g.FillPath(brush, path);
                }
            }
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int x = bounds.X;
            int y = bounds.Y;
            int width = bounds.Width;
            int height = bounds.Height;
            int r = Math.Min(radius, Math.Min(width / 2, height / 2));

            if (width > 0 && height > 0)
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
            }

            return path;
        }
    }

    #endregion
}
