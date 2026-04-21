using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using Nimbus.MDUI;

namespace Nimbus.MDUI
{
    /// <summary>
    /// Material Design Engine - GDI+ Rendering Engine
    /// Complete WPF-like UI framework built on GDI+
    /// </summary>
    public class MDEngine
    {
        private Graphics _graphics;
        private Bitmap _backBuffer;
        private Control _hostControl;
        private List<MDElement> _elements;
        private Dictionary<MDElement, bool> _elementHoverState;
        private MDElement _focusedElement;
        private float _dpiScale = 1.0f;

        public float DpiScale
        {
            get => _dpiScale;
            set => _dpiScale = Math.Max(0.5f, value);
        }

        public Color BackgroundColor { get; set; } = MDColors.Background;
        public bool VSync { get; set; } = true;
        public int TargetFPS { get; set; } = 60;

        public event EventHandler<RenderEventArgs> PreRender;
        public event EventHandler<RenderEventArgs> PostRender;

        public MDEngine(Control hostControl)
        {
            _hostControl = hostControl ?? throw new ArgumentNullException(nameof(hostControl));
            _elements = new List<MDElement>();
            _elementHoverState = new Dictionary<MDElement, bool>();

            InitializeControl();
        }

        private void InitializeControl()
        {
            _hostControl.DoubleBuffered = true;
            _hostControl.BackColor = BackgroundColor;
            _hostControl.MouseMove += OnHostMouseMove;
            _hostControl.MouseDown += OnHostMouseDown;
            _hostControl.MouseUp += OnHostMouseUp;
            _hostControl.MouseLeave += OnHostMouseLeave;
            _hostControl.KeyDown += OnHostKeyDown;
            _hostControl.KeyUp += OnHostKeyUp;
            _hostControl.Paint += OnHostPaint;

            // Get DPI scaling
            using (var g = _hostControl.CreateGraphics())
            {
                _dpiScale = g.DpiX / 96f;
            }
        }

        #region Element Management

        public void AddElement(MDElement element)
        {
            if (element != null && !_elements.Contains(element))
            {
                _elements.Add(element);
                _elementHoverState[element] = false;
                Invalidate();
            }
        }

        public void RemoveElement(MDElement element)
        {
            if (element != null)
            {
                _elements.Remove(element);
                _elementHoverState.Remove(element);
                if (_focusedElement == element)
                    _focusedElement = null;
                Invalidate();
            }
        }

        public void ClearElements()
        {
            _elements.Clear();
            _elementHoverState.Clear();
            _focusedElement = null;
            Invalidate();
        }

        public IReadOnlyList<MDElement> GetElements() => _elements.AsReadOnly();

        public MDElement GetElementAt(Point point)
        {
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                if (_elements[i].HitTest(point))
                    return _elements[i];
            }
            return null;
        }

        #endregion

        #region Focus Management

        public void SetFocus(MDElement element)
        {
            if (_focusedElement == element) return;

            _focusedElement = element;
            Invalidate();
        }

        public MDElement GetFocused() => _focusedElement;

        #endregion

        #region Input Handling

        private void OnHostMouseMove(object sender, MouseEventArgs e)
        {
            foreach (var element in _elements)
            {
                bool isOver = element.HitTest(e.Location);
                bool wasHovered = _elementHoverState.ContainsKey(element) && _elementHoverState[element];

                if (isOver && !wasHovered)
                {
                    element.OnMouseEnter();
                    _elementHoverState[element] = true;
                    Invalidate();
                }
                else if (!isOver && wasHovered)
                {
                    element.OnMouseLeave();
                    _elementHoverState[element] = false;
                    Invalidate();
                }

                if (isOver)
                {
                    element.OnMouseMove(e);
                }
            }
        }

        private void OnHostMouseDown(object sender, MouseEventArgs e)
        {
            if (!_hostControl.Enabled) return;

            var element = GetElementAt(e.Location);
            if (element != null)
            {
                SetFocus(element);
                element.OnMouseDown(e);
                Invalidate();
            }
        }

        private void OnHostMouseUp(object sender, MouseEventArgs e)
        {
            var element = GetElementAt(e.Location);
            if (element != null)
            {
                element.OnMouseUp(e);
                Invalidate();
            }
        }

        private void OnHostMouseLeave(object sender, EventArgs e)
        {
            foreach (var element in _elements.Where(el => _elementHoverState.ContainsKey(el) && _elementHoverState[el]).ToList())
            {
                element.OnMouseLeave();
                _elementHoverState[element] = false;
            }
            Invalidate();
        }

        private void OnHostKeyDown(object sender, KeyEventArgs e)
        {
            _focusedElement?.OnKeyDown(e);
            Invalidate();
        }

        private void OnHostKeyUp(object sender, KeyEventArgs e)
        {
            _focusedElement?.OnKeyUp(e);
            Invalidate();
        }

        #endregion

        #region Rendering

        private void OnHostPaint(object sender, PaintEventArgs e)
        {
            Render(e.Graphics);
        }

        public void Render(Graphics g)
        {
            if (_hostControl.Width <= 0 || _hostControl.Height <= 0) return;

            // Create backbuffer if needed
            if (_backBuffer == null || _backBuffer.Width != _hostControl.Width || _backBuffer.Height != _hostControl.Height)
            {
                _backBuffer?.Dispose();
                _backBuffer = new Bitmap(_hostControl.Width, _hostControl.Height);
            }

            using (var backGraphics = Graphics.FromImage(_backBuffer))
            {
                backGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                backGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                backGraphics.Clear(BackgroundColor);

                RenderFrame(backGraphics);
            }

            g.DrawImageUnscaled(_backBuffer, 0, 0);
        }

        private void RenderFrame(Graphics g)
        {
            // Pre-render event
            PreRender?.Invoke(this, new RenderEventArgs(g));

            // Sort elements by depth (containers should render before their children)
            var sortedElements = _elements.OrderBy(el => el is MDContainer ? 0 : 1).ToList();

            // Render all elements
            foreach (var element in sortedElements)
            {
                try
                {
                    element.Draw(g);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error rendering element {element?.Name}: {ex.Message}");
                }
            }

            // Draw focus indicator
            if (_focusedElement != null && _focusedElement.Visible)
            {
                DrawFocusIndicator(g, _focusedElement);
            }

            // Post-render event
            PostRender?.Invoke(this, new RenderEventArgs(g));
        }

        private void DrawFocusIndicator(Graphics g, MDElement element)
        {
            var focusRect = new Rectangle(
                element.Bounds.X - 2,
                element.Bounds.Y - 2,
                element.Bounds.Width + 4,
                element.Bounds.Height + 4);

            using (var pen = new Pen(MDColors.Primary, 2) { DashStyle = DashStyle.Dash })
            {
                g.DrawRectangle(pen, focusRect);
            }
        }

        public void Invalidate()
        {
            _hostControl?.Invalidate();
        }

        #endregion

        #region Animation Support

        public void AnimateProperty(MDElement element, string propertyName, object targetValue, int duration)
        {
            // Simple animation framework - can be extended
            var animation = new PropertyAnimation(element, propertyName, targetValue, duration);
            animation.Start(_hostControl);
        }

        #endregion

        #region Utility Methods

        public void SetTheme(MDTheme theme)
        {
            MDColors.Primary = theme.Primary;
            MDColors.PrimaryLight = theme.PrimaryLight;
            MDColors.PrimaryDark = theme.PrimaryDark;
            MDColors.Secondary = theme.Secondary;
            MDColors.Surface = theme.Surface;
            MDColors.Background = theme.Background;
            BackgroundColor = theme.Background;
            Invalidate();
        }

        public Point ScreenToClient(Point screenPoint)
        {
            return _hostControl.PointToClient(screenPoint);
        }

        public Point ClientToScreen(Point clientPoint)
        {
            return _hostControl.PointToScreen(clientPoint);
        }

        public Size GetSize() => _hostControl.ClientSize;

        public void Dispose()
        {
            _backBuffer?.Dispose();
            _graphics?.Dispose();
        }

        #endregion
    }

    #region Animation Support

    public class PropertyAnimation
    {
        private MDElement _element;
        private string _propertyName;
        private object _targetValue;
        private int _duration;
        private int _elapsed;
        private Timer _timer;
        private object _startValue;

        public PropertyAnimation(MDElement element, string propertyName, object targetValue, int duration)
        {
            _element = element;
            _propertyName = propertyName;
            _targetValue = targetValue;
            _duration = Math.Max(1, duration);
            _elapsed = 0;
        }

        public void Start(Control hostControl)
        {
            var property = _element.GetType().GetProperty(_propertyName);
            if (property == null) return;

            _startValue = property.GetValue(_element);

            _timer = new Timer();
            _timer.Interval = 16; // ~60 FPS
            _timer.Tick += (s, e) => OnTick(hostControl);
            _timer.Start();
        }

        private void OnTick(Control hostControl)
        {
            _elapsed += _timer.Interval;
            float progress = Math.Min(1, (float)_elapsed / _duration);

            var property = _element.GetType().GetProperty(_propertyName);
            if (property != null)
            {
                object value = EaseValue(_startValue, _targetValue, progress);
                property.SetValue(_element, value);
            }

            hostControl?.Invalidate();

            if (progress >= 1)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        private object EaseValue(object start, object end, float progress)
        {
            if (start is int iStart && end is int iEnd)
            {
                return (int)(iStart + (iEnd - iStart) * progress);
            }
            if (start is float fStart && end is float fEnd)
            {
                return fStart + (fEnd - fStart) * progress;
            }
            if (start is double dStart && end is double dEnd)
            {
                return dStart + (dEnd - dStart) * progress;
            }
            return end;
        }
    }

    #endregion

    #region Theme System

    public class MDTheme
    {
        public Color Primary { get; set; } = MDColors.Primary;
        public Color PrimaryLight { get; set; } = MDColors.PrimaryLight;
        public Color PrimaryDark { get; set; } = MDColors.PrimaryDark;
        public Color Secondary { get; set; } = MDColors.Secondary;
        public Color Surface { get; set; } = MDColors.Surface;
        public Color Background { get; set; } = MDColors.Background;

        public static MDTheme Dark()
        {
            return new MDTheme
            {
                Primary = Color.FromArgb(187, 134, 252),
                PrimaryLight = Color.FromArgb(209, 154, 255),
                PrimaryDark = Color.FromArgb(156, 39, 176),
                Secondary = Color.FromArgb(255, 183, 77),
                Surface = Color.FromArgb(49, 48, 51),
                Background = Color.FromArgb(28, 27, 31)
            };
        }

        public static MDTheme Light()
        {
            return new MDTheme
            {
                Primary = Color.FromArgb(103, 58, 183),
                PrimaryLight = Color.FromArgb(156, 39, 176),
                PrimaryDark = Color.FromArgb(63, 81, 181),
                Secondary = Color.FromArgb(0, 188, 212),
                Surface = Color.FromArgb(245, 245, 245),
                Background = Color.White
            };
        }

        public static MDTheme BlueTheme()
        {
            return new MDTheme
            {
                Primary = Color.FromArgb(13, 110, 253),
                PrimaryLight = Color.FromArgb(102, 166, 255),
                PrimaryDark = Color.FromArgb(0, 80, 200),
                Secondary = Color.FromArgb(32, 201, 151),
                Surface = Color.FromArgb(240, 245, 250),
                Background = Color.White
            };
        }

        public static MDTheme GreenTheme()
        {
            return new MDTheme
            {
                Primary = Color.FromArgb(56, 142, 60),
                PrimaryLight = Color.FromArgb(129, 199, 132),
                PrimaryDark = Color.FromArgb(27, 94, 32),
                Secondary = Color.FromArgb(251, 188, 5),
                Surface = Color.FromArgb(240, 250, 240),
                Background = Color.White
            };
        }
    }

    #endregion

    #region Event Arguments

    public class RenderEventArgs : EventArgs
    {
        public Graphics Graphics { get; set; }

        public RenderEventArgs(Graphics graphics)
        {
            Graphics = graphics;
        }
    }

    #endregion

    #region Layout Helpers

    public static class MDLayoutHelper
    {
        public static void CenterHorizontal(MDElement element, int containerWidth)
        {
            element.Bounds = new Rectangle(
                (containerWidth - element.Bounds.Width) / 2,
                element.Bounds.Y,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void CenterVertical(MDElement element, int containerHeight)
        {
            element.Bounds = new Rectangle(
                element.Bounds.X,
                (containerHeight - element.Bounds.Height) / 2,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void Center(MDElement element, int containerWidth, int containerHeight)
        {
            element.Bounds = new Rectangle(
                (containerWidth - element.Bounds.Width) / 2,
                (containerHeight - element.Bounds.Height) / 2,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void AlignTop(MDElement element, int margin = 0)
        {
            element.Bounds = new Rectangle(
                element.Bounds.X,
                margin,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void AlignBottom(MDElement element, int containerHeight, int margin = 0)
        {
            element.Bounds = new Rectangle(
                element.Bounds.X,
                containerHeight - element.Bounds.Height - margin,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void AlignLeft(MDElement element, int margin = 0)
        {
            element.Bounds = new Rectangle(
                margin,
                element.Bounds.Y,
                element.Bounds.Width,
                element.Bounds.Height);
        }

        public static void AlignRight(MDElement element, int containerWidth, int margin = 0)
        {
            element.Bounds = new Rectangle(
                containerWidth - element.Bounds.Width - margin,
                element.Bounds.Y,
                element.Bounds.Width,
                element.Bounds.Height);
        }
    }

    #endregion

    #region Grid Layout

    public class MDGridLayout : MDContainer
    {
        public int Columns { get; set; } = 1;
        public int Rows { get; set; } = 1;
        public int CellSpacing { get; set; } = 8;

        protected override void LayoutChildren()
        {
            int cellWidth = (Bounds.Width - Padding.Horizontal - (Columns - 1) * CellSpacing) / Columns;
            int cellHeight = (Bounds.Height - Padding.Vertical - (Rows - 1) * CellSpacing) / Rows;

            int childIndex = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (childIndex >= _children.Count) break;

                    var child = _children[childIndex];
                    int x = Bounds.X + Padding.Left + col * (cellWidth + CellSpacing);
                    int y = Bounds.Y + Padding.Top + row * (cellHeight + CellSpacing);

                    child.Bounds = new Rectangle(x, y, cellWidth, cellHeight);
                    childIndex++;
                }
            }
        }
    }

    #endregion
}
