using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Text;

namespace Nimbus.WPF
{
    /// <summary>
    /// ModuleWindow - Full 2D Custom Drawing Renderer for UIModule
    /// Complete custom rendering using DrawingContext - NO WinForms controls
    /// Pure geometric/text rendering with no WPF Button, TextBox, CheckBox, etc.
    /// </summary>
    public class ModuleWindow : Window
    {
        private WpfEngine _engine;
        private IUIModule _rootModule;
        private DrawingCanvas _canvas;
        private List<string> _debugLogs;
        private bool _debugVisible = false;
        private XmlNode _rootNode;

        public ModuleWindow(WpfEngine engine, XmlNode rootNode, XmlNode uiNode, IUIModule rootModule)
        {
            _engine = engine;
            _rootModule = rootModule;
            _rootNode = rootNode;
            _debugLogs = new List<string>();

            // Configure window
            ConfigureWindow(rootNode);

            // Create drawing canvas for 2D rendering (FIX: pass engine reference)
            _canvas = new DrawingCanvas(_engine);
            this.Content = _canvas;

            // Add F12 key handler
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.F12)
                {
                    ToggleDebugPanel();
                    e.Handled = true;
                }
            };

            AddDebugLog("[INIT] ModuleWindow created with 2D rendering - Press F12 for debug console");
            
            // Render the UI module tree
            if (_rootModule != null)
            {
                Rect bounds = new Rect(0, 0, this.Width, this.Height);
                _canvas.RenderModule(_rootModule, bounds);
                AddDebugLog("[RENDER] UI tree rendered successfully (2D drawing)");
                AddDebugLog("[ROOT] Element: " + _rootModule.ElementType);
            }
        }

        private void PaintDebugConsole()
        {
            if (!_debugVisible) return;
            // Debug console will be rendered as overlay in DrawingCanvas._renderDebugConsole()
        }

        private void ToggleDebugPanel()
        {
            _debugVisible = !_debugVisible;
            
            if (_debugVisible)
            {
                AddDebugLog("[DEBUG] Console opened - F12 to close");
            }
            else
            {
                AddDebugLog("[DEBUG] Console closed");
            }
            
            _canvas.InvalidateVisual();
            _canvas.SetDebugVisible(_debugVisible);
        }

        private string BuildModuleTree(IUIModule module, int depth)
        {
            if (module == null) return "";
            
            string indent = new string(' ', depth * 2);
            string tree = indent + "├─ [" + module.ElementType + "] id=" + module.Id;
            
            if (module is ModuleUIElement)
            {
                ModuleUIElement me = (ModuleUIElement)module;
                tree += " | W=" + me.Width + " H=" + me.Height + " BG=" + me.Background;
            }
            
            tree += "\n";

            foreach (var child in module.Children)
            {
                tree += BuildModuleTree(child, depth + 1);
            }

            return tree;
        }

        private void AddDebugLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = "[" + timestamp + "] " + message;
            _debugLogs.Add(logEntry);
            _canvas.AddDebugLog(logEntry);
        }

        public List<string> GetDebugLogs()
        {
            return _debugLogs;
        }

        public IUIModule GetRootModule()
        {
            return _rootModule;
        }

        /// <summary>
        /// Configure window properties from root XML node
        /// </summary>
        private void ConfigureWindow(XmlNode rootNode)
        {
            if (rootNode == null) return;

            this.Title = GetAttribute(rootNode, "Name", "Nimbus App");
            
            double width = 800;
            if (double.TryParse(GetAttribute(rootNode, "Width", "800"), out width))
                this.Width = width;
            
            double height = 600;
            if (double.TryParse(GetAttribute(rootNode, "Height", "600"), out height))
                this.Height = height;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            string bgColor = GetAttribute(rootNode, "Background", "#1E1E1E");
            try { this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)); }
            catch { this.Background = new SolidColorBrush(Colors.White); }
        }

        private Color ParseColor(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorStr)) return defaultColor;
            try { return (Color)ColorConverter.ConvertFromString(colorStr); }
            catch { return defaultColor; }
        }

        private double ParseDouble(string value, double defaultValue)
        {
            double result;
            return double.TryParse(value, out result) ? result : defaultValue;
        }

        private HorizontalAlignment ParseHorizontalAlignment(string value)
        {
            switch ((value ?? "Stretch").ToLower())
            {
                case "left": return HorizontalAlignment.Left;
                case "right": return HorizontalAlignment.Right;
                case "center": return HorizontalAlignment.Center;
                default: return HorizontalAlignment.Stretch;
            }
        }

        private VerticalAlignment ParseVerticalAlignment(string value)
        {
            switch ((value ?? "Stretch").ToLower())
            {
                case "top": return VerticalAlignment.Top;
                case "bottom": return VerticalAlignment.Bottom;
                case "center": return VerticalAlignment.Center;
                default: return VerticalAlignment.Stretch;
            }
        }

        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
    }

    /// <summary>
    /// DrawingCanvas - Custom DrawingVisual-based rendering for IUIModule
    /// All rendering done with geometric shapes and text - ZERO WPF controls
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        private WpfEngine _engine;
        private IUIModule _rootModule;
        private List<string> _debugLogs = new List<string>();
        private bool _debugVisible = false;
        private const int DebugPanelHeight = 200;
        private Dictionary<Rect, IUIModule> _clickableRegions = new Dictionary<Rect, IUIModule>();
        private Point _lastMousePos;
        private IUIModule _hoveredModule = null;
        private IUIModule _activeInputModule = null;
        private Rect    _activeInputRect   = Rect.Empty;
        private System.Windows.Controls.TextBox _inputOverlay;
        // Native button press tracking
        private IUIModule _pressedModule = null;
        private Rect      _pressedRect   = Rect.Empty;
        
        // ═════════════════════════════════════════════════════════════
        // EVENT SYSTEM
        // ═════════════════════════════════════════════════════════════
        private EventDispatcher _eventDispatcher;
        private DateTime _mouseDownTime = DateTime.Now;
        private const int LongPressThresholdMs = 500;  // Long press if held > 500ms
        private System.Windows.Threading.DispatcherTimer _longPressTimer;
        private bool _longPressTriggered = false;
        
        public DrawingCanvas(WpfEngine engine)
        {
            _engine   = engine;
            this.Focusable = true;

            // ── Initialize Event Dispatcher ──
            _eventDispatcher = new EventDispatcher(debugLogging: false);

            // ── Long Press Timer ──
            _longPressTimer = new System.Windows.Threading.DispatcherTimer();
            _longPressTimer.Interval = TimeSpan.FromMilliseconds(LongPressThresholdMs);
            _longPressTimer.Tick += (s, e) =>
            {
                if (_pressedModule != null && !_longPressTriggered)
                {
                    _longPressTriggered = true;
                    _longPressTimer.Stop();

                    // Fire long press event
                    var mouseData = new MouseData
                    {
                        X = _lastMousePos.X,
                        Y = _lastMousePos.Y,
                        LeftButton = true
                    };
                    var evt = new LongPressEvent(_pressedModule, mouseData, LongPressThresholdMs);
                    DispatchNimbusEvent(evt);

                    InvalidateVisual();
                }
            };

            // ── Styled TextBox overlay for real keyboard input ──
            _inputOverlay = new System.Windows.Controls.TextBox
            {
                Visibility       = Visibility.Hidden,
                Background       = System.Windows.Media.Brushes.Transparent,
                BorderThickness  = new Thickness(0),
                Foreground       = System.Windows.Media.Brushes.White,
                CaretBrush       = System.Windows.Media.Brushes.White,
                FontFamily       = new FontFamily("Segoe UI"),
                FontSize         = 13,
                Padding          = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            this.Children.Add(_inputOverlay);

            _inputOverlay.TextChanged += (s, e) =>
            {
                if (_activeInputModule == null) return;
                string v = _inputOverlay.Text;
                if (_activeInputModule is NimbusTextInput) ((NimbusTextInput)_activeInputModule).Value = v;
                else if (_activeInputModule is CustomUIInput) ((CustomUIInput)_activeInputModule).Value = v;
                else if (_activeInputModule is NimbusSearchInput) ((NimbusSearchInput)_activeInputModule).Value  = v;
                else if (_activeInputModule is NimbusTextArea) ((NimbusTextArea)_activeInputModule).Value  = v;
                else if (_activeInputModule is NimbusPasswordInput) ((NimbusPasswordInput)_activeInputModule).Value = v;
                InvalidateVisual();
            };
            _inputOverlay.LostFocus += (s, e) => HideInputOverlay();
            _inputOverlay.KeyDown   += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape ||
                    e.Key == System.Windows.Input.Key.Return)
                { this.Focus(); e.Handled = true; }
            };
            
            // Attach real styled WPF ContextMenu — rebuilt per activation to use module's XML def
            _inputOverlay.ContextMenu = BuildWpfContextMenu(null);

            // ══════════════════════════════════════════════════════════════
            // MOUSE EVENTS - Now using Event System
            // ══════════════════════════════════════════════════════════════

            // ── Mouse DOWN: track pressed module for native feel ──
            this.MouseDown += (s, e) =>
            {
                Point pt = e.GetPosition(this);
                if (_activeInputModule != null && !_activeInputRect.Contains(pt))
                    HideInputOverlay();

                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    _mouseDownTime = DateTime.Now;
                    _longPressTriggered = false;
                    _longPressTimer.Start();

                    _pressedModule = null;
                    _pressedRect   = Rect.Empty;
                    foreach (var kvp in _clickableRegions)
                    {
                        if (kvp.Key.Contains(pt))
                        { _pressedModule = kvp.Value; _pressedRect = kvp.Key; }
                    }
                    
                    if (_pressedModule != null)
                    {
                        // Fire mousedown event
                        var mouseData = new MouseData
                        {
                            X = pt.X,
                            Y = pt.Y,
                            ClientX = pt.X - _pressedRect.Left,
                            ClientY = pt.Y - _pressedRect.Top,
                            LeftButton = true
                        };
                        var evt = new MouseDownEvent(_pressedModule, mouseData);
                        DispatchNimbusEvent(evt);

                        InvalidateVisual();
                    }
                }
                else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    HandleRightClick(pt);
                }
            };

            // ── Mouse UP: fire action only if still over same module (native feel) ──
            this.MouseUp += (s, e) =>
            {
                if (e.ChangedButton != System.Windows.Input.MouseButton.Left) return;

                _longPressTimer.Stop();

                Point pt = e.GetPosition(this);
                if (_pressedModule != null)
                {
                    IUIModule pm = _pressedModule;
                    DateTime releaseTime = DateTime.Now;
                    double heldMs = (releaseTime - _mouseDownTime).TotalMilliseconds;

                    _pressedModule = null;
                    InvalidateVisual();

                    if (_pressedRect.Contains(pt))
                    {
                        // Fire mouseup event
                        var mouseData = new MouseData
                        {
                            X = pt.X,
                            Y = pt.Y,
                            ClientX = pt.X - _pressedRect.Left,
                            ClientY = pt.Y - _pressedRect.Top,
                            LeftButton = true
                        };
                        var upEvent = new MouseUpEvent(pm, mouseData);
                        DispatchNimbusEvent(upEvent);

                        // Fire click event (only if not long press)
                        if (!_longPressTriggered && heldMs < LongPressThresholdMs)
                        {
                            var clickEvent = new ClickEvent(pm, mouseData, heldMs);
                            DispatchNimbusEvent(clickEvent);
                        }
                    }
                }
            };

            // ── Mouse move: hover + Hand cursor + emit events ──
            this.MouseMove += (s, e) =>
            {
                _lastMousePos = e.GetPosition(this);
                IUIModule hov = GetModuleAt(_lastMousePos);
                
                if (hov != _hoveredModule)
                {
                    // Mouse leave
                    if (_hoveredModule != null)
                    {
                        var leaveEvent = new MouseLeaveEvent(_hoveredModule, new MouseData
                        {
                            X = _lastMousePos.X,
                            Y = _lastMousePos.Y,
                            LeftButton = e.LeftButton == MouseButtonState.Pressed,
                            RightButton = e.RightButton == MouseButtonState.Pressed
                        });
                        DispatchNimbusEvent(leaveEvent);
                    }

                    // Mouse enter
                    _hoveredModule = hov;
                    if (_hoveredModule != null)
                    {
                        var enterEvent = new MouseEnterEvent(_hoveredModule, new MouseData
                        {
                            X = _lastMousePos.X,
                            Y = _lastMousePos.Y,
                            LeftButton = e.LeftButton == MouseButtonState.Pressed,
                            RightButton = e.RightButton == MouseButtonState.Pressed
                        });
                        DispatchNimbusEvent(enterEvent);
                    }

                    this.Cursor = hov != null
                        ? System.Windows.Input.Cursors.Hand
                        : System.Windows.Input.Cursors.Arrow;
                    InvalidateVisual();
                }
            };

            this.MouseLeave += (s, e) =>
            {
                if (_hoveredModule != null)
                {
                    var leaveEvent = new MouseLeaveEvent(_hoveredModule, new MouseData
                    {
                        X = _lastMousePos.X,
                        Y = _lastMousePos.Y
                    });
                    DispatchNimbusEvent(leaveEvent);
                }

                _hoveredModule = null;
                _pressedModule = null;
                _longPressTimer.Stop();
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                InvalidateVisual();
            };

            this.SizeChanged += (s, e) => InvalidateVisual();
        }

        // ── Helpers ──────────────────────────────────────────────────
        private bool IsHovered(IUIModule m)     { return m != null && m == _hoveredModule; }
        private bool IsInputActive(IUIModule m) { return m != null && m == _activeInputModule; }
        private bool IsPressed(IUIModule m)     { return m != null && m == _pressedModule; }

        /// <summary>Dispatch Nimbus event through the event system</summary>
        private void DispatchNimbusEvent(NimbusEvent evt)
        {
            if (evt == null || evt.Target == null) return;
            evt.Target.DispatchEvent(evt);
        }

        /// <summary>Handle right-click (context menu request)</summary>
        private void HandleRightClick(Point clickPos)
        {
            IUIModule module = null;
            Rect moduleRect = Rect.Empty;

            foreach (var kvp in _clickableRegions)
            {
                if (kvp.Key.Contains(clickPos))
                {
                    module = kvp.Value;
                    moduleRect = kvp.Key;
                }
            }

            if (module == null) return;

            // Fire context request event
            var mouseData = new MouseData
            {
                X = clickPos.X,
                Y = clickPos.Y,
                ClientX = clickPos.X - moduleRect.Left,
                ClientY = clickPos.Y - moduleRect.Top,
                RightButton = true
            };
            var contextEvent = new ContextRequestEvent(module, mouseData);
            DispatchNimbusEvent(contextEvent);

            // Also show traditional context menu for text inputs
            if (module is NimbusTextInput || module is CustomUIInput ||
                module is NimbusSearchInput || module is NimbusTextArea ||
                module is NimbusPasswordInput)
            {
                // WPF ContextMenu on _inputOverlay handles this
            }

            HandleClick(clickPos, System.Windows.Input.MouseButton.Right);
        }

        private Color Lighten(Color c, double f = 0.20)
        {
            return Color.FromArgb(c.A,
                (byte)Math.Min(255, c.R + (int)((255 - c.R) * f)),
                (byte)Math.Min(255, c.G + (int)((255 - c.G) * f)),
                (byte)Math.Min(255, c.B + (int)((255 - c.B) * f)));
        }

        private System.Windows.Controls.ContextMenu BuildWpfContextMenu(IUIModule module)
        {
            // Get the XML-defined context menu if available
            NimbusContextMenuDef def = null;
            if (module != null)
            {
                ModuleUIElement modEl = module as ModuleUIElement;
                if (modEl != null) def = modEl.ContextMenuDef;
            }

            var cm = new System.Windows.Controls.ContextMenu();
            cm.Template = BuildContextMenuTemplate(def);

            if (def != null && def.Items.Count > 0)
            {
                // Use XML-defined items
                foreach (var itemDef in def.Items)
                {
                    if (itemDef.IsSeparator)
                    {
                        cm.Items.Add(MakeSeparator());
                        continue;
                    }
                    IUIModule capturedModule = module;
                    NimbusContextMenuItemDef capturedItem = itemDef;
                    cm.Items.Add(MakeMenuItemFromDef(capturedItem, def, () =>
                    {
                        ExecuteContextMenuAction(capturedItem, capturedModule);
                    }));
                }
            }
            else
            {
                // Default built-in items
                cm.Items.Add(MakeMenuItem("\uD83D\uDCDD", "Nusxalash",         "Ctrl+C", false, null, () => _inputOverlay.Copy()));
                cm.Items.Add(MakeMenuItem("\u2702",       "Kesib olish",        "Ctrl+X", false, null, () => _inputOverlay.Cut()));
                cm.Items.Add(MakeMenuItem("\uD83D\uDCCB", "Joylash",            "Ctrl+V", false, null, () => _inputOverlay.Paste()));
                cm.Items.Add(MakeMenuItem("\u2610",       "Hammasini tanlash",  "Ctrl+A", false, null, () => _inputOverlay.SelectAll()));
                cm.Items.Add(MakeSeparator());
                cm.Items.Add(MakeMenuItem("\u274C",       "O'chirish",          "",       true,  null, () => { _inputOverlay.SelectedText = ""; }));
            }
            return cm;
        }

        private void ExecuteContextMenuAction(NimbusContextMenuItemDef item, IUIModule module)
        {
            // Built-in actions
            if (!string.IsNullOrEmpty(item.Action))
            {
                switch (item.Action.ToLower())
                {
                    case "copy":      _inputOverlay.Copy();               return;
                    case "cut":       _inputOverlay.Cut();                return;
                    case "paste":     _inputOverlay.Paste();              return;
                    case "selectall": _inputOverlay.SelectAll();          return;
                    case "delete":    _inputOverlay.SelectedText = "";   return;
                    case "undo":      _inputOverlay.Undo();               return;
                }
            }
            // Engine handler
            if (!string.IsNullOrEmpty(item.Handler) && _engine != null)
            {
                try { _engine.ExecuteHandler(item.Handler, module); }
                catch { }
            }
        }

        private System.Windows.Controls.MenuItem MakeMenuItemFromDef(
            NimbusContextMenuItemDef itemDef,
            NimbusContextMenuDef menuDef,
            System.Action action)
        {
            return MakeMenuItem(
                itemDef.Icon, itemDef.Header, itemDef.Shortcut,
                itemDef.Danger, itemDef.HoverBg ?? menuDef.HoverBg, action);
        }

        private System.Windows.Controls.ControlTemplate BuildContextMenuTemplate(NimbusContextMenuDef def = null)
        {
            var outerF = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.Border));
            outerF.SetValue(System.Windows.Controls.Border.PaddingProperty, new Thickness(6));

            var cardF = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.Border));
            cardF.SetValue(System.Windows.Controls.Border.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(22, 22, 34)));
            cardF.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(10));
            cardF.SetValue(System.Windows.Controls.Border.BorderBrushProperty,
                new SolidColorBrush(Color.FromArgb(55, 160, 140, 255)));
            cardF.SetValue(System.Windows.Controls.Border.BorderThicknessProperty, new Thickness(1));
            cardF.SetValue(System.Windows.Controls.Border.PaddingProperty, new Thickness(4));

            var effectF = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.Border));
            effectF.SetValue(System.Windows.Controls.Border.BackgroundProperty,
                new SolidColorBrush(Color.FromArgb(10, 255, 255, 255)));
            effectF.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));

            var panelF = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.StackPanel));
            panelF.SetValue(System.Windows.Controls.StackPanel.IsItemsHostProperty, true);

            effectF.AppendChild(panelF);
            cardF.AppendChild(effectF);
            outerF.AppendChild(cardF);

            var t = new System.Windows.Controls.ControlTemplate(typeof(System.Windows.Controls.ContextMenu));
            t.VisualTree = outerF;
            return t;
        }

        private System.Windows.Controls.MenuItem MakeMenuItem(string icon, string header, string shortcut, bool danger, string hoverBg, System.Action action)
        {
            Color fg = danger ? Color.FromRgb(255, 70, 90) : Color.FromRgb(218, 218, 228);
            var g = new System.Windows.Controls.Grid { Height = 34 };
            g.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(22) });
            g.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });

            var ic = new System.Windows.Controls.TextBlock { Text = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush(fg) };
            System.Windows.Controls.Grid.SetColumn(ic, 0);

            var hd = new System.Windows.Controls.TextBlock { Text = header, FontSize = 13, FontFamily = new FontFamily("Segoe UI"), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(fg) };
            System.Windows.Controls.Grid.SetColumn(hd, 1);
            g.Children.Add(ic); g.Children.Add(hd);

            if (!string.IsNullOrEmpty(shortcut))
            {
                var sc = new System.Windows.Controls.TextBlock { Text = shortcut, FontSize = 11, FontFamily = new FontFamily("Segoe UI"), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromArgb(100, 180, 180, 200)), Margin = new Thickness(12,0,0,0) };
                System.Windows.Controls.Grid.SetColumn(sc, 2);
                g.Children.Add(sc);
            }

            var item = new System.Windows.Controls.MenuItem { Header = g, Padding = new Thickness(8,0,12,0), Background = System.Windows.Media.Brushes.Transparent, BorderThickness = new Thickness(0) };
            item.Click += (s, e) => action();
            var hov = new SolidColorBrush(Color.FromArgb(28, 255, 255, 255));
            item.MouseEnter += (s, e) => ((System.Windows.Controls.MenuItem)s).Background = hov;
            item.MouseLeave += (s, e) => ((System.Windows.Controls.MenuItem)s).Background = System.Windows.Media.Brushes.Transparent;
            return item;
        }

        private System.Windows.Controls.Separator MakeSeparator()
        {
            var sep = new System.Windows.Controls.Separator();
            sep.Margin = new Thickness(10, 3, 10, 3);
            sep.Background = new SolidColorBrush(Color.FromArgb(40, 200, 200, 255));
            return sep;
        }

        private void ShowInputOverlay(IUIModule module, Rect bounds)
        {
            _activeInputModule = module;
            _activeInputRect   = bounds;
            string val = "";
            if      (module is NimbusTextInput) val = ((NimbusTextInput)module).Value ?? "";
            else if (module is CustomUIInput) val = ((CustomUIInput)module).Value ?? "";
            else if (module is NimbusSearchInput) val = ((NimbusSearchInput)module).Value  ?? "";
            else if (module is NimbusTextArea) val = ((NimbusTextArea)module).Value  ?? "";
            else if (module is NimbusPasswordInput) val = ((NimbusPasswordInput)module).Value ?? "";

            double lpad = (module is NimbusSearchInput) ? 34 : 12;
            double top  = bounds.Top + (bounds.Height - 22) / 2.0;
            Canvas.SetLeft(_inputOverlay, bounds.Left + lpad);
            Canvas.SetTop (_inputOverlay, top);
            _inputOverlay.Width      = bounds.Width - lpad - 8;
            _inputOverlay.Height     = 22;
            _inputOverlay.Text       = val;
            _inputOverlay.Visibility = Visibility.Visible;
            _inputOverlay.Focus();
            _inputOverlay.CaretIndex = _inputOverlay.Text.Length;
            InvalidateVisual();
        }

        private void HideInputOverlay()
        {
            _activeInputModule       = null;
            _activeInputRect         = Rect.Empty;
            _inputOverlay.Visibility = Visibility.Hidden;
            InvalidateVisual();
        }

        /// <summary>
        /// Returns the topmost clickable IUIModule at the given point, or null.
        /// </summary>
        private IUIModule GetModuleAt(Point pt)
        {
            IUIModule found = null;
            foreach (var kvp in _clickableRegions)
            {
                if (kvp.Key.Contains(pt))
                    found = kvp.Value;   // keep last (topmost)
            }
            return found;
        }

        private void HandleClick(Point clickPos, System.Windows.Input.MouseButton button)
        {
            // Find which element was clicked (last match = topmost)
            IUIModule clickedModule = null;
            Rect clickedRect = Rect.Empty;
            foreach (var kvp in _clickableRegions)
            {
                if (kvp.Key.Contains(clickPos))
                {
                    clickedModule = kvp.Value;
                    clickedRect = kvp.Key;
                }
            }
            if (clickedModule == null) return;
            IUIModule module = clickedModule;

            // ── Text inputs: show keyboard overlay ──
            if (module is NimbusTextInput || module is CustomUIInput ||
                module is NimbusSearchInput || module is NimbusTextArea ||
                module is NimbusPasswordInput)
            {
                if (button == System.Windows.Input.MouseButton.Right)
                    return; // WPF ContextMenu on _inputOverlay handles right-click
                
                ShowInputOverlay(module, clickedRect);
                AddDebugLog("[CLICK] Input focused: " + (module.Id ?? module.ElementType));
                return;
            }

            // ── ComboBox: cycle items ──
            if (module is NimbusComboBox)
            {
                NimbusComboBox cmb = (NimbusComboBox)module;
                if (cmb.Items != null && cmb.Items.Count > 0)
                    cmb.SelectedIndex = (cmb.SelectedIndex + 1) % cmb.Items.Count;
                AddDebugLog("[CLICK] ComboBox sel=" + cmb.SelectedIndex);
                InvalidateVisual();
                return;
            }

            // ── onclick XML handler (highest priority for buttons) ──
            object onclickHandler = module.GetProperty("__onclick__");
            if (onclickHandler != null && !string.IsNullOrEmpty(onclickHandler.ToString()))
            {
                string handlerName = onclickHandler.ToString();
                AddDebugLog("[CLICK] '" + (module.Id ?? module.ElementType) + "' -> " + handlerName);
                try   { if (_engine != null) _engine.ExecuteHandler(handlerName, module); }
                catch (Exception ex) { AddDebugLog("[ERROR] " + ex.Message); }
                InvalidateVisual();
                return;
            }

            // ── Toggle / Switch / CheckBox state flip ──
            if (module is NimbusSwitch)        { var m2 = (NimbusSwitch)module;       m2.IsOn      = !m2.IsOn;      AddDebugLog("[SW] IsOn="    +m2.IsOn);      InvalidateVisual(); return; }
            if (module is NimbusCheckBox)      { var m2 = (NimbusCheckBox)module;     m2.IsChecked = !m2.IsChecked; AddDebugLog("[CB] ="       +m2.IsChecked); InvalidateVisual(); return; }
            if (module is NimbusToggleButton)  { var m2 = (NimbusToggleButton)module; m2.IsToggled = !m2.IsToggled; AddDebugLog("[TB] ="       +m2.IsToggled); InvalidateVisual(); return; }
            if (module is NimbusRadioButton)
            {
                var rb = (NimbusRadioButton)module;
                foreach (var kv in _clickableRegions)
                {
                    if (kv.Value is NimbusRadioButton)
                    {
                        var otherRb = (NimbusRadioButton)kv.Value;
                        if (otherRb != rb && (otherRb.GroupName == rb.GroupName || string.IsNullOrEmpty(rb.GroupName)))
                            otherRb.IsSelected = false;
                    }
                }
                rb.IsSelected = true;
                AddDebugLog("[RB] sel");
                InvalidateVisual();
                return;
            }
            if (module is NimbusLinkButton)
            {
                var lb = (NimbusLinkButton)module;
                lb.IsVisited = true;
                AddDebugLog("[LINK] " + lb.Url);
                try { if (!string.IsNullOrEmpty(lb.Url)) System.Diagnostics.Process.Start(lb.Url); }
                catch (Exception ex) { AddDebugLog("[ERROR] " + ex.Message); }
                InvalidateVisual();
                return;
            }
            if (module is NimbusIconButton)    { var m2 = (NimbusIconButton)module;   m2.IsToggled = !m2.IsToggled; AddDebugLog("[IB] tog");                   InvalidateVisual(); return; }
            if (module is CustomUIToggle)
            {
                var t = (CustomUIToggle)module;
                t.IsChecked = !t.IsChecked;
                if (t.OnChange != null) try { t.OnChange(); } catch { }
                AddDebugLog("[TOG] =" + t.IsChecked);
                InvalidateVisual();
                return;
            }

            // ── OnClick delegate (legacy) ──
            if      (module is NimbusButton) { var nb = (NimbusButton)module; AddDebugLog("[BTN] " + nb.Id); if (nb.OnClick != null) try { nb.OnClick(); } catch { } }
            else if (module is CustomUIButton) { var cb2 = (CustomUIButton)module; AddDebugLog("[BTN] " + cb2.Id); if (cb2.OnClick != null) try { cb2.OnClick(); } catch { } }
            else { AddDebugLog("[CLICK] " + (module.Id ?? module.ElementType)); }

            InvalidateVisual();
        }

        public void RenderModule(IUIModule module, Rect bounds)
        {
            _rootModule = module;
            InvalidateVisual();
        }

        public void SetDebugVisible(bool visible)
        {
            _debugVisible = visible;
            InvalidateVisual();
        }

        public void AddDebugLog(string message)
        {
            _debugLogs.Add(message);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            _clickableRegions.Clear();
            
            if (_rootModule != null)
            {
                Rect bounds = new Rect(0, 0,
                    Math.Max(0, this.ActualWidth),
                    Math.Max(0, this.ActualHeight));
                // Dark background for InModule
                drawingContext.DrawRectangle(
                    new SolidColorBrush(ParseColor(null, Color.FromArgb(255, 10, 10, 18))), null, bounds);
                RenderModuleRecursive(drawingContext, _rootModule, bounds);
            }

            if (_debugVisible)
            {
                RenderDebugConsole(drawingContext);
            }
        }

        private void RenderModuleRecursive(DrawingContext dc, IUIModule module, Rect bounds)
        {
            if (module == null) return;

            string type = module.ElementType.ToLower();

            switch (type)
            {
                // Core containers
                case "flexpanel": RenderFlexPanel(dc, (CustomUIFlexPanel)module, bounds); break;
                case "absolutepanel": RenderAbsolutePanel(dc, (CustomUIAbsolutePanel)module, bounds); break;
                case "card": RenderCard(dc, (CustomUICard)module, bounds); break;
                case "grid": RenderGrid(dc, (CustomUIGrid)module, bounds); break;
                case "modal": RenderModal(dc, (CustomUIModal)module, bounds); break;
                case "tabs": RenderTabs(dc, (CustomUITabs)module, bounds); break;
                
                // Core controls
                case "button": RenderButton(dc, (CustomUIButton)module, bounds); break;
                case "label":
                case "text":
                case "textblock": RenderLabel(dc, (CustomUILabel)module, bounds); break;
                case "input":
                case "textbox": RenderInput(dc, (CustomUIInput)module, bounds); break;
                case "toggle":
                case "checkbox": RenderToggle(dc, (CustomUIToggle)module, bounds); break;
                case "slider": RenderSlider(dc, (CustomUISlider)module, bounds); break;
                case "progressbar": RenderProgressBar(dc, (CustomUIProgressBar)module, bounds); break;
                case "badge": RenderBadge(dc, (CustomUIBadge)module, bounds); break;

                // UILayout Buttons
                case "nimbusbutton": RenderNimbusButton(dc, (NimbusButton)module, bounds); break;
                case "iconbutton": RenderNimbusIconButton(dc, (NimbusIconButton)module, bounds); break;
                case "fab": RenderNimbusButton(dc, null, bounds); break;
                case "togglebutton": RenderNimbusToggleButton(dc, (NimbusToggleButton)module, bounds); break;
                case "linkbutton": RenderNimbusLinkButton(dc, (NimbusLinkButton)module, bounds); break;
                case "dropdownbutton": RenderGenericContainer(dc, module, bounds); break;

                // UILayout Inputs
                case "nimbustextinput": RenderNimbusTextInput(dc, (NimbusTextInput)module, bounds); break;
                case "nimbustextarea": RenderNimbusTextArea(dc, (NimbusTextArea)module, bounds); break;
                case "searchinput": RenderNimbusSearchInput(dc, (NimbusSearchInput)module, bounds); break;
                case "passwordinput": RenderNimbusPasswordInput(dc, (NimbusPasswordInput)module, bounds); break;
                case "numberinput": RenderNimbusNumberInput(dc, (NimbusNumberInput)module, bounds); break;
                case "combobox": RenderNimbusComboBox(dc, (NimbusComboBox)module, bounds); break;
                case "switch": RenderNimbusSwitch(dc, (NimbusSwitch)module, bounds); break;
                case "nimbuscheckbox": RenderNimbusCheckBox(dc, (NimbusCheckBox)module, bounds); break;
                case "radiobutton": RenderNimbusRadioButton(dc, (NimbusRadioButton)module, bounds); break;

                // UILayout Widgets
                case "divider": RenderNimbusDivider(dc, (NimbusDivider)module, bounds); break;
                case "avatar": RenderNimbusAvatar(dc, (NimbusAvatar)module, bounds); break;
                case "chip": RenderNimbusChip(dc, (NimbusChip)module, bounds); break;

                // Anything else: generic container (renders background + children)
                default: RenderGenericContainer(dc, module, bounds); break;
            }
        }

        private void RenderFlexPanel(DrawingContext dc, CustomUIFlexPanel panel, Rect bounds)
        {
            // Draw background
            Color bgColor = ParseColor(panel.Background, Color.FromArgb(0, 255, 255, 255));
            if (bgColor.A > 0)
                dc.DrawRectangle(new SolidColorBrush(bgColor), null, bounds);

            // Get padding/spacing
            double padding = ParseDouble(panel.Padding ?? "0", 0);
            double gap     = ParseDouble(panel.Gap     ?? "0", 0);

            // Calculate available space
            double availableWidth  = bounds.Width  - (padding * 2);
            double availableHeight = bounds.Height - (padding * 2);

            // Count fixed sizes and flex items
            double totalFixedWidth = 0;
            double totalFixedHeight = 0;
            int flexCount = 0;
            int childCount = panel.Children.Count;

            foreach (var child in panel.Children)
            {
                if (panel.Direction.ToLower() == "row")
                {
                    double? cw = GetComponentWidth(child, availableWidth);
                    if (cw.HasValue) totalFixedWidth += cw.Value;
                    else flexCount++;
                }
                else
                {
                    double? ch = GetComponentHeight(child, availableHeight);
                    if (ch.HasValue) totalFixedHeight += ch.Value;
                    else flexCount++;
                }
            }

            // Total gap space
            double totalGap = childCount > 1 ? gap * (childCount - 1) : 0;

            // Distribute flex space
            double flexWidth  = flexCount > 0 ? (availableWidth  - totalFixedWidth  - totalGap) / flexCount : 0;
            double flexHeight = flexCount > 0 ? (availableHeight - totalFixedHeight - totalGap) / flexCount : 0;

            // Render children
            double childX = bounds.Left + padding;
            double childY = bounds.Top  + padding;

            dc.PushClip(new RectangleGeometry(bounds));

            bool isFirst = true;
            foreach (var child in panel.Children)
            {
                if (!isFirst)
                {
                    if (panel.Direction.ToLower() == "row") childX += gap;
                    else childY += gap;
                }
                isFirst = false;

                Rect childBounds;
                if (panel.Direction.ToLower() == "row")
                {
                    double childWidth = GetComponentWidth(child, availableWidth) ?? flexWidth;
                    childBounds = new Rect(childX, childY, Math.Max(0, childWidth), Math.Max(0, availableHeight));
                    childX += childWidth;
                }
                else
                {
                    double childHeight = GetComponentHeight(child, availableHeight) ?? flexHeight;
                    childBounds = new Rect(childX, childY, Math.Max(0, availableWidth), Math.Max(0, childHeight));
                    childY += childHeight;
                }

                RenderModuleRecursive(dc, child, childBounds);
            }

            dc.Pop(); // Remove clip
        }

        private void RenderAbsolutePanel(DrawingContext dc, CustomUIAbsolutePanel panel, Rect bounds)
        {
            Color bgColor = ParseColor(panel.Background, Colors.White);
            if (bgColor.A > 0)
                dc.DrawRectangle(new SolidColorBrush(bgColor), null, bounds);

            foreach (var child in panel.Children)
            {
                double left = ParseDouble(child.Properties.ContainsKey("Left") ? child.Properties["Left"].ToString() : "0", 0);
                double top = ParseDouble(child.Properties.ContainsKey("Top") ? child.Properties["Top"].ToString() : "0", 0);
                double width = GetComponentWidth(child, bounds.Width) ?? 100;
                double height = GetComponentHeight(child, bounds.Height) ?? 50;

                Rect childBounds = new Rect(bounds.Left + left, bounds.Top + top, Math.Max(0, width), Math.Max(0, height));
                RenderModuleRecursive(dc, child, childBounds);
            }
        }

        private void RenderCard(DrawingContext dc, CustomUICard card, Rect bounds)
        {
            // Draw background with rounded corners
            Color bgColor = ParseColor(card.Background, Colors.White);
            dc.DrawRoundedRectangle(new SolidColorBrush(bgColor), null, bounds, card.CornerRadius, card.CornerRadius);

            // Draw border
            Color borderColor = ParseColor(card.BorderBrush, Colors.Gray);
            double bt = card.BorderThickness > 0 ? card.BorderThickness : 1.0;
            if (borderColor.A > 0)
                dc.DrawRoundedRectangle(null, new Pen(new SolidColorBrush(borderColor), bt), bounds, card.CornerRadius, card.CornerRadius);

            // Render children with padding, stacking sequentially
            double padding = ParseDouble(card.Padding ?? "16", 16);
            double gap = 8;
            Rect innerBounds = new Rect(
                bounds.Left + padding, bounds.Top + padding,
                Math.Max(0, bounds.Width - padding * 2),
                Math.Max(0, bounds.Height - padding * 2));
            double childY = innerBounds.Top;

            dc.PushClip(new RectangleGeometry(bounds, card.CornerRadius, card.CornerRadius));

            foreach (var child in card.Children)
            {
                double childH = GetComponentHeight(child, innerBounds.Height) ?? 40;
                Rect childBounds = new Rect(innerBounds.Left, childY, innerBounds.Width, Math.Max(0, childH));
                RenderModuleRecursive(dc, child, childBounds);
                childY += childH + gap;
            }
            
            dc.Pop(); // Remove clip
        }

        private void RenderButton(DrawingContext dc, CustomUIButton button, Rect bounds)
        {
            // Register as clickable
            _clickableRegions[bounds] = button;
            
            // Draw button background
            Color bgColor = ParseColor(button.Background, Color.FromArgb(255, 0, 122, 204));
            // HOVER: lighten background
            if (IsHovered(button)) bgColor = Lighten(bgColor, 0.22);
            dc.DrawRoundedRectangle(new SolidColorBrush(bgColor), null, bounds, 4, 4);

            // Draw text
            Color textColor = ParseColor(button.Foreground, Colors.White);
            FormattedText text = new FormattedText(
                button.Text ?? "Button",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                13,
                new SolidColorBrush(textColor)
            );

            double textX = bounds.Left + (bounds.Width - text.Width) / 2;
            double textY = bounds.Top + (bounds.Height - text.Height) / 2;
            dc.DrawText(text, new Point(textX, textY));
        }

        private void RenderLabel(DrawingContext dc, CustomUILabel label, Rect bounds)
        {
            Color textColor = ParseColor(label.Foreground, Colors.Black);
            FormattedText text = new FormattedText(
                label.Text ?? "",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                label.FontSize,
                new SolidColorBrush(textColor)
            );

            dc.DrawText(text, new Point(bounds.Left, bounds.Top));
        }

        private void RenderInput(DrawingContext dc, CustomUIInput input, Rect bounds)
        {
            // Register as clickable so overlay is shown
            _clickableRegions[bounds] = input;
            bool active = IsInputActive(input);
            bool hov    = IsHovered(input);

            Color bgColor = ParseColor(input.Background, Color.FromArgb(255, 36, 36, 52));
            if (hov) bgColor = Lighten(bgColor, 0.12);
            dc.DrawRoundedRectangle(new SolidColorBrush(bgColor), null, bounds, 6, 6);

            // Border: accent when active, subtle when hovered, dim otherwise
            Color border = active ? Color.FromArgb(255, 124, 111, 255)
                         : hov   ? Color.FromArgb(255, 90, 90, 130)
                                 : Color.FromArgb(255, 55, 55, 80);
            dc.DrawRoundedRectangle(null, new Pen(new SolidColorBrush(border), active ? 2 : 1), bounds, 6, 6);

            // Placeholder or value
            string display = input.Value ?? "";
            string placeholder = input.Placeholder ?? "";
            bool showPlaceholder = string.IsNullOrEmpty(display) && !active;
            Color textColor = showPlaceholder
                ? Color.FromArgb(160, 150, 150, 180)
                : Color.FromArgb(255, 238, 238, 255);
            string textToShow = showPlaceholder ? placeholder : (active ? "" : display);

            if (!string.IsNullOrEmpty(textToShow))
            {
                FormattedText ft = new FormattedText(textToShow,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12,
                    new SolidColorBrush(textColor));
                dc.DrawText(ft, new Point(bounds.Left + 12, bounds.Top + (bounds.Height - ft.Height) / 2));
            }
        }

        private void RenderToggle(DrawingContext dc, CustomUIToggle toggle, Rect bounds)
        {
            _clickableRegions[bounds] = toggle;
            bool hov = IsHovered(toggle);

            // Draw checkbox box
            Rect box = new Rect(bounds.Left, bounds.Top + (bounds.Height - 18) / 2, 18, 18);
            Color boxBg = toggle.IsChecked
                ? Color.FromArgb(255, 124, 111, 255)
                : Color.FromArgb(255, 36, 36, 52);
            if (hov) boxBg = Lighten(boxBg, 0.15);
            dc.DrawRoundedRectangle(new SolidColorBrush(boxBg),
                new Pen(new SolidColorBrush(Color.FromArgb(255, 100, 100, 180)), 1.5), box, 4, 4);

            if (toggle.IsChecked)
            {
                // Checkmark
                Pen ck = new Pen(new SolidColorBrush(Colors.White), 2) { LineJoin = PenLineJoin.Round };
                dc.DrawLine(ck, new Point(box.Left + 3, box.Top + 9), new Point(box.Left + 7, box.Top + 13));
                dc.DrawLine(ck, new Point(box.Left + 7, box.Top + 13), new Point(box.Left + 15, box.Top + 4));
            }

            // Label
            string lbl = toggle.Label ?? "";
            if (!string.IsNullOrEmpty(lbl))
            {
                FormattedText ft = new FormattedText(lbl,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12,
                    new SolidColorBrush(Color.FromArgb(255, 200, 200, 220)));
                dc.DrawText(ft, new Point(box.Right + 8, bounds.Top + (bounds.Height - ft.Height) / 2));
            }
        }

        private void RenderSlider(DrawingContext dc, CustomUISlider slider, Rect bounds)
        {
            // Draw track
            Rect trackBounds = new Rect(bounds.Left, bounds.Top + bounds.Height / 2 - 2, bounds.Width, 4);
            dc.DrawRectangle(new SolidColorBrush(Colors.LightGray), null, trackBounds);

            // Draw thumb
            double thumbPos = bounds.Left + (slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum) * bounds.Width;
            Rect thumbBounds = new Rect(thumbPos - 6, bounds.Top + bounds.Height / 2 - 8, 12, 16);
            dc.DrawEllipse(new SolidColorBrush(Colors.CornflowerBlue), new Pen(new SolidColorBrush(Colors.Gray), 1), 
                new Point(thumbPos, bounds.Top + bounds.Height / 2), 6, 8);
        }

        private void RenderProgressBar(DrawingContext dc, CustomUIProgressBar bar, Rect bounds)
        {
            // Draw background
            dc.DrawRectangle(new SolidColorBrush(Colors.LightGray), null, bounds);

            // Draw progress
            double progressWidth = (bar.Progress / 100.0) * bounds.Width;
            Rect progressBounds = new Rect(bounds.Left, bounds.Top, progressWidth, bounds.Height);
            Color progressColor = ParseColor(bar.ProgressColor, Color.FromArgb(255, 0, 122, 204));
            dc.DrawRectangle(new SolidColorBrush(progressColor), null, progressBounds);

            // Draw border
            dc.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.Gray), 1), bounds);
        }

        private void RenderBadge(DrawingContext dc, CustomUIBadge badge, Rect bounds)
        {
            // Draw badge background (rounded)
            dc.DrawRoundedRectangle(new SolidColorBrush(Colors.CornflowerBlue), null, bounds, 12, 12);

            // Draw text
            FormattedText text = new FormattedText(
                badge.Content ?? "",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                new SolidColorBrush(Colors.White)
            );

            double textX = bounds.Left + (bounds.Width - text.Width) / 2;
            double textY = bounds.Top + (bounds.Height - text.Height) / 2;
            dc.DrawText(text, new Point(textX, textY));
        }

        private void RenderModal(DrawingContext dc, CustomUIModal modal, Rect bounds)
        {
            if (!modal.IsVisible) return;

            // Draw semi-transparent overlay
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), null, bounds);

            // Draw modal box
            Rect modalBounds = new Rect(bounds.Left + 50, bounds.Top + 50, Math.Max(0, bounds.Width - 100), Math.Max(0, bounds.Height - 100));
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(230, 30, 30, 30)), null, modalBounds, 12, 12);

            // Render children
            double childY = modalBounds.Top + 16;
            foreach (var child in modal.Children)
            {
                double childHeight = GetComponentHeight(child, modalBounds.Height) ?? 50;
                Rect childBounds = new Rect(modalBounds.Left + 16, childY, Math.Max(0, modalBounds.Width - 32), Math.Max(0, childHeight));
                RenderModuleRecursive(dc, child, childBounds);
                childY += childHeight;
            }
        }

        private void RenderTabs(DrawingContext dc, CustomUITabs tabs, Rect bounds)
        {
            // Draw tab headers
            double tabWidth = bounds.Width / tabs.TabNames.Count;
            for (int i = 0; i < tabs.TabNames.Count; i++)
            {
                Rect tabBounds = new Rect(bounds.Left + i * tabWidth, bounds.Top, tabWidth, 30);
                dc.DrawRectangle(new SolidColorBrush(Colors.LightGray), null, tabBounds);
                dc.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.Gray), 1), tabBounds);

                FormattedText tabText = new FormattedText(
                    tabs.TabNames[i],
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    12,
                    new SolidColorBrush(Colors.Black)
                );

                dc.DrawText(tabText, new Point(tabBounds.Left + 8, tabBounds.Top + 8));
            }

            // Draw tab content area
            Rect contentBounds = new Rect(bounds.Left, bounds.Top + 30, bounds.Width, bounds.Height - 30);
            dc.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.Gray), 1), contentBounds);
        }

        private void RenderGrid(DrawingContext dc, CustomUIGrid grid, Rect bounds)
        {
            Color bgColor = ParseColor(grid.Background, Colors.White);
            if (bgColor.A > 0)
                dc.DrawRectangle(new SolidColorBrush(bgColor), null, bounds);

            double childY = bounds.Top;
            foreach (var child in grid.Children)
            {
                double childHeight = GetComponentHeight(child, bounds.Height) ?? 50;
                Rect childBounds = new Rect(bounds.Left, childY, bounds.Width, childHeight);
                RenderModuleRecursive(dc, child, childBounds);
                childY += childHeight;
            }
        }

        private void RenderGenericContainer(DrawingContext dc, IUIModule module, Rect bounds)
        {
            if (module is ModuleUIElement)
            {
                ModuleUIElement elem = (ModuleUIElement)module;
                Color bgColor = ParseColor(elem.Background, Colors.White);
                if (bgColor.A > 0)
                    dc.DrawRectangle(new SolidColorBrush(bgColor), null, bounds);
            }

            foreach (var child in module.Children)
            {
                RenderModuleRecursive(dc, child, bounds);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // UILayout — Button Renderers
        // ═══════════════════════════════════════════════════════════

        private void RenderNimbusButton(DrawingContext dc, NimbusButton btn, Rect bounds)
        {
            if (btn == null) { RenderGenericContainer(dc, null, bounds); return; }
            _clickableRegions[bounds] = btn;

            string bgHex = btn.GetEffectiveBackground();
            Color bg = ParseColor(bgHex, Color.FromArgb(255, 108, 99, 255));
            if (IsHovered(btn)) bg = Lighten(bg, 0.22);
            double cr = btn.CornerRadius;
            dc.DrawRoundedRectangle(new SolidColorBrush(bg), null, bounds, cr, cr);

            // Outlined border
            string borderHex = btn.GetEffectiveBorder();
            Color border = ParseColor(borderHex, Colors.Transparent);
            if (border.A > 0)
            {
                if (IsHovered(btn)) border = Lighten(border, 0.3);
                double bt = btn.BorderThickness > 0 ? btn.BorderThickness : 1.5;
                dc.DrawRoundedRectangle(null, new Pen(new SolidColorBrush(border), bt), bounds, cr, cr);
            }

            // Label
            string label = btn.IsLoading ? btn.LoadingText : btn.Text;
            Color fg = ParseColor(btn.TextColor ?? btn.Foreground, Colors.White);
            FormattedText ft = new FormattedText(
                label ?? "Button",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(btn.FontFamily ?? "Segoe UI"),
                btn.FontSize > 0 ? btn.FontSize : 14,
                new SolidColorBrush(fg));
            dc.DrawText(ft, new Point(
                bounds.Left + (bounds.Width - ft.Width) / 2,
                bounds.Top  + (bounds.Height - ft.Height) / 2));
        }

        private void RenderNimbusIconButton(DrawingContext dc, NimbusIconButton btn, Rect bounds)
        {
            if (btn == null) return;
            _clickableRegions[bounds] = btn;
            double cr = btn.IsCircular ? bounds.Width / 2 : btn.CornerRadius;
            Color bg = ParseColor(btn.IsToggled ? btn.ToggledColor : btn.ButtonColor, Colors.Transparent);
            if (IsHovered(btn) && bg.A > 0) bg = Lighten(bg, 0.22);
            if (bg.A > 0)
                dc.DrawRoundedRectangle(new SolidColorBrush(bg), null, bounds, cr, cr);
            Color iconColor = ParseColor(btn.IsToggled ? btn.ToggledIconColor : btn.IconColor, Colors.White);
            if (IsHovered(btn) && bg.A == 0) iconColor = Lighten(iconColor, 0.4);
            FormattedText icon = new FormattedText(
                btn.Icon ?? "●",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                btn.IconSize > 0 ? btn.IconSize : 18,
                new SolidColorBrush(iconColor));
            dc.DrawText(icon, new Point(
                bounds.Left + (bounds.Width - icon.Width) / 2,
                bounds.Top  + (bounds.Height - icon.Height) / 2));
        }

        private void RenderNimbusToggleButton(DrawingContext dc, NimbusToggleButton btn, Rect bounds)
        {
            if (btn == null) return;
            _clickableRegions[bounds] = btn;
            Color bg = ParseColor(btn.IsToggled ? btn.ActiveColor : btn.InactiveColor, Color.FromArgb(255, 62, 62, 66));
            if (IsHovered(btn)) bg = Lighten(bg, 0.22);
            dc.DrawRoundedRectangle(new SolidColorBrush(bg), null, bounds, btn.CornerRadius, btn.CornerRadius);
            Color fg = ParseColor(btn.IsToggled ? btn.ActiveTextColor : btn.InactiveTextColor, Colors.White);
            string label = btn.IsToggled ? (btn.ActiveText ?? btn.Text) : btn.Text;
            FormattedText ft = new FormattedText(
                label ?? "Toggle",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), btn.FontSize > 0 ? btn.FontSize : 14,
                new SolidColorBrush(fg));
            dc.DrawText(ft, new Point(
                bounds.Left + (bounds.Width - ft.Width) / 2,
                bounds.Top  + (bounds.Height - ft.Height) / 2));
        }

        private void RenderNimbusLinkButton(DrawingContext dc, NimbusLinkButton btn, Rect bounds)
        {
            if (btn == null) return;
            _clickableRegions[bounds] = btn;
            Color fg = ParseColor(btn.IsVisited ? btn.VisitedColor : btn.LinkColor, Color.FromArgb(255, 108, 99, 255));
            if (IsHovered(btn)) fg = Lighten(fg, 0.4);
            FormattedText ft = new FormattedText(
                btn.Text ?? "Link",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), btn.FontSize > 0 ? btn.FontSize : 14,
                new SolidColorBrush(fg));
            dc.DrawText(ft, new Point(bounds.Left, bounds.Top + (bounds.Height - ft.Height) / 2));
            if (btn.ShowUnderline || IsHovered(btn))
                dc.DrawLine(new Pen(new SolidColorBrush(fg), 1),
                    new Point(bounds.Left, bounds.Top + (bounds.Height + ft.Height) / 2),
                    new Point(bounds.Left + ft.Width, bounds.Top + (bounds.Height + ft.Height) / 2));
        }

        // ═══════════════════════════════════════════════════════════
        // UILayout — Input Renderers
        // ═══════════════════════════════════════════════════════════

        private void RenderNimbusTextInput(DrawingContext dc, NimbusTextInput inp, Rect bounds)
        {
            if (inp == null) return;
            _clickableRegions[bounds] = inp;
            Color bg = ParseColor(inp.Background, Color.FromArgb(255, 45, 45, 48));
            Color border = ParseColor(inp.GetEffectiveBorderColor(), Color.FromArgb(255, 85, 85, 85));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(border), 1.5),
                bounds, inp.CornerRadius, inp.CornerRadius);
            // Label (floating)
            if (!string.IsNullOrEmpty(inp.Label))
            {
                Color lc = ParseColor(inp.LabelColor, Color.FromArgb(255, 158, 158, 158));
                FormattedText lbl = new FormattedText(inp.Label,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 11, new SolidColorBrush(lc));
                dc.DrawText(lbl, new Point(bounds.Left + 12, bounds.Top - 8));
            }
            // Value or placeholder
            bool active = IsInputActive(inp);
            string display = string.IsNullOrEmpty(inp.Value) ? inp.Placeholder : inp.Value;
            bool showPlaceholder = string.IsNullOrEmpty(inp.Value) && !active;
            Color textColor = showPlaceholder
                ? ParseColor(inp.PlaceholderColor, Color.FromArgb(255, 100, 100, 100))
                : ParseColor(inp.Foreground, Colors.White);
            string textToShow = showPlaceholder ? inp.Placeholder : (active ? "" : display);

            if (!string.IsNullOrEmpty(textToShow))
            {
                FormattedText ft = new FormattedText(textToShow,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), inp.FontSize > 0 ? inp.FontSize : 14,
                    new SolidColorBrush(textColor));
                dc.DrawText(ft, new Point(bounds.Left + 12, bounds.Top + (bounds.Height - ft.Height) / 2));
            }
        }

        private void RenderNimbusTextArea(DrawingContext dc, NimbusTextArea ta, Rect bounds)
        {
            if (ta == null) return;
            Color bg = ParseColor(ta.Background, Color.FromArgb(255, 45, 45, 48));
            Color border = ParseColor(ta.BorderBrush, Color.FromArgb(255, 85, 85, 85));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(border), 1.5),
                bounds, ta.CornerRadius, ta.CornerRadius);
            bool active = IsInputActive(ta);
            string display = string.IsNullOrEmpty(ta.Value) ? ta.Placeholder : ta.Value;
            bool showPlaceholder = string.IsNullOrEmpty(ta.Value) && !active;
            Color tc = showPlaceholder
                ? Color.FromArgb(255, 100, 100, 100)
                : ParseColor(ta.Foreground, Colors.White);
            string textToShow = showPlaceholder ? ta.Placeholder : (active ? "" : display);

            if (!string.IsNullOrEmpty(textToShow))
            {
                FormattedText ft = new FormattedText(textToShow,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), ta.FontSize > 0 ? ta.FontSize : 14,
                    new SolidColorBrush(tc));
                ft.MaxTextWidth = bounds.Width - 24;
                dc.DrawText(ft, new Point(bounds.Left + 12, bounds.Top + 12));
            }
        }

        private void RenderNimbusSearchInput(DrawingContext dc, NimbusSearchInput si, Rect bounds)
        {
            if (si == null) return;
            _clickableRegions[bounds] = si;
            Color bg = ParseColor(si.Background, Color.FromArgb(255, 45, 45, 48));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg),
                new Pen(new SolidColorBrush(ParseColor(si.BorderBrush, Color.FromArgb(255, 85, 85, 85))), 1),
                bounds, si.CornerRadius, si.CornerRadius);
            // Search icon
            if (si.ShowSearchIcon)
            {
                FormattedText icon = new FormattedText("🔍",
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 14, new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)));
                dc.DrawText(icon, new Point(bounds.Left + 10, bounds.Top + (bounds.Height - icon.Height) / 2));
            }
            bool active = IsInputActive(si);
            string display = string.IsNullOrEmpty(si.Value) ? si.Placeholder : si.Value;
            bool showPlaceholder = string.IsNullOrEmpty(si.Value) && !active;
            Color tc = showPlaceholder ? Color.FromArgb(255, 100, 100, 100) : ParseColor(si.Foreground, Colors.White);
            string textToShow = showPlaceholder ? si.Placeholder : (active ? "" : display);

            if (!string.IsNullOrEmpty(textToShow))
            {
                FormattedText ft = new FormattedText(textToShow,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 14, new SolidColorBrush(tc));
                dc.DrawText(ft, new Point(bounds.Left + 32, bounds.Top + (bounds.Height - ft.Height) / 2));
            }
        }

        private void RenderNimbusPasswordInput(DrawingContext dc, NimbusPasswordInput pi, Rect bounds)
        {
            if (pi == null) return;
            _clickableRegions[bounds] = pi;
            Color bg = ParseColor(pi.Background, Color.FromArgb(255, 45, 45, 48));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg),
                new Pen(new SolidColorBrush(ParseColor(pi.BorderBrush, Color.FromArgb(255, 85, 85, 85))), 1.5),
                bounds, pi.CornerRadius, pi.CornerRadius);
            bool active = IsInputActive(pi);
            string display = string.IsNullOrEmpty(pi.Value) ? pi.Placeholder
                : (pi.IsPasswordVisible ? pi.Value : new string('●', pi.Value.Length));
            bool showPlaceholder = string.IsNullOrEmpty(pi.Value) && !active;
            Color tc = showPlaceholder ? Color.FromArgb(255, 100, 100, 100) : ParseColor(pi.Foreground, Colors.White);
            string textToShow = showPlaceholder ? pi.Placeholder : (active ? "" : display);

            if (!string.IsNullOrEmpty(textToShow))
            {
                FormattedText ft = new FormattedText(textToShow,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), pi.FontSize > 0 ? pi.FontSize : 14,
                    new SolidColorBrush(tc));
                dc.DrawText(ft, new Point(bounds.Left + 12, bounds.Top + (bounds.Height - ft.Height) / 2));
            }
            // Eye icon
            if (pi.ShowToggleButton)
            {
                FormattedText eye = new FormattedText(pi.IsPasswordVisible ? "🙈" : "👁",
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 14, new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)));
                dc.DrawText(eye, new Point(bounds.Right - 30, bounds.Top + (bounds.Height - eye.Height) / 2));
            }
        }

        private void RenderNimbusNumberInput(DrawingContext dc, NimbusNumberInput ni, Rect bounds)
        {
            if (ni == null) return;
            _clickableRegions[bounds] = ni;
            Color bg = ParseColor(ni.Background, Color.FromArgb(255, 45, 45, 48));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg),
                new Pen(new SolidColorBrush(ParseColor(ni.BorderBrush, Color.FromArgb(255, 85, 85, 85))), 1.5),
                bounds, ni.CornerRadius, ni.CornerRadius);
            string display = ni.Value.ToString("F" + ni.DecimalPlaces) + (ni.Unit != null ? " " + ni.Unit : "");
            FormattedText ft = new FormattedText(display,
                System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), ni.FontSize > 0 ? ni.FontSize : 14,
                new SolidColorBrush(ParseColor(ni.Foreground, Colors.White)));
            dc.DrawText(ft, new Point(bounds.Left + 10, bounds.Top + (bounds.Height - ft.Height) / 2));
            if (ni.ShowStepper)
            {
                // + button
                Rect plusR = new Rect(bounds.Right - 24, bounds.Top + 4, 20, bounds.Height - 8);
                dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(255, 80, 80, 90)), null, plusR, 4, 4);
                FormattedText plus = new FormattedText("+",
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 14, new SolidColorBrush(Colors.White));
                dc.DrawText(plus, new Point(plusR.Left + (plusR.Width - plus.Width) / 2, plusR.Top + (plusR.Height - plus.Height) / 2));
            }
        }

        private void RenderNimbusComboBox(DrawingContext dc, NimbusComboBox cb, Rect bounds)
        {
            if (cb == null) return;
            _clickableRegions[bounds] = cb;
            Color bg = ParseColor(cb.Background, Color.FromArgb(255, 45, 45, 48));
            dc.DrawRoundedRectangle(new SolidColorBrush(bg),
                new Pen(new SolidColorBrush(ParseColor(cb.BorderBrush, Color.FromArgb(255, 85, 85, 85))), 1.5),
                bounds, cb.CornerRadius, cb.CornerRadius);
            string display = cb.SelectedIndex >= 0 && cb.SelectedIndex < cb.Items.Count
                ? cb.Items[cb.SelectedIndex].DisplayText : cb.Placeholder;
            Color tc = cb.SelectedIndex < 0 ? Color.FromArgb(255, 100, 100, 100) : ParseColor(cb.Foreground, Colors.White);
            FormattedText ft = new FormattedText(display ?? "",
                System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), cb.FontSize > 0 ? cb.FontSize : 14, new SolidColorBrush(tc));
            dc.DrawText(ft, new Point(bounds.Left + 12, bounds.Top + (bounds.Height - ft.Height) / 2));
            // Arrow
            FormattedText arrow = new FormattedText(cb.IsOpen ? "▲" : "▼",
                System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 10, new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)));
            dc.DrawText(arrow, new Point(bounds.Right - 20, bounds.Top + (bounds.Height - arrow.Height) / 2));
        }

        private void RenderNimbusSwitch(DrawingContext dc, NimbusSwitch sw, Rect bounds)
        {
            if (sw == null) return;
            _clickableRegions[bounds] = sw;
            double trackW = 48, trackH = 24;
            Rect trackR = new Rect(bounds.Left, bounds.Top + (bounds.Height - trackH) / 2, trackW, trackH);
            Color trackColor = ParseColor(sw.IsOn ? sw.ActiveColor : sw.InactiveColor, Color.FromArgb(255, 85, 85, 85));
            dc.DrawRoundedRectangle(new SolidColorBrush(trackColor), null, trackR, trackH / 2, trackH / 2);
            // Thumb
            double thumbX = sw.IsOn ? trackR.Right - trackH + 2 : trackR.Left + 2;
            double thumbSz = trackH - 4;
            dc.DrawEllipse(new SolidColorBrush(ParseColor(sw.ThumbColor, Colors.White)), null,
                new Point(thumbX + thumbSz / 2, trackR.Top + trackH / 2), thumbSz / 2, thumbSz / 2);
            // Label
            if (!string.IsNullOrEmpty(sw.Label))
            {
                FormattedText lbl = new FormattedText(sw.Label,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 13, new SolidColorBrush(ParseColor(sw.Foreground, Colors.White)));
                dc.DrawText(lbl, new Point(trackR.Right + 8, trackR.Top + (trackH - lbl.Height) / 2));
            }
        }

        private void RenderNimbusCheckBox(DrawingContext dc, NimbusCheckBox cb, Rect bounds)
        {
            if (cb == null) return;
            _clickableRegions[bounds] = cb;
            double sz = cb.CheckSize > 0 ? cb.CheckSize : 20;
            Rect box = new Rect(bounds.Left, bounds.Top + (bounds.Height - sz) / 2, sz, sz);
            Color boxColor = cb.IsChecked
                ? ParseColor(cb.CheckColor, Color.FromArgb(255, 108, 99, 255))
                : ParseColor(cb.UncheckedColor, Color.FromArgb(255, 85, 85, 85));
            dc.DrawRoundedRectangle(new SolidColorBrush(boxColor),
                new Pen(new SolidColorBrush(boxColor), 1.5), box, 4, 4);
            if (cb.IsChecked)
            {
                Pen ck = new Pen(new SolidColorBrush(ParseColor(cb.CheckmarkColor, Colors.White)), 2);
                dc.DrawLine(ck, new Point(box.Left + 4, box.Top + sz / 2), new Point(box.Left + sz / 2.5, box.Bottom - 4));
                dc.DrawLine(ck, new Point(box.Left + sz / 2.5, box.Bottom - 4), new Point(box.Right - 3, box.Top + 4));
            }
            if (!string.IsNullOrEmpty(cb.Label))
            {
                FormattedText lbl = new FormattedText(cb.Label,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 13, new SolidColorBrush(ParseColor(cb.Foreground, Colors.White)));
                dc.DrawText(lbl, new Point(box.Right + 8, box.Top + (sz - lbl.Height) / 2));
            }
        }

        private void RenderNimbusRadioButton(DrawingContext dc, NimbusRadioButton rb, Rect bounds)
        {
            if (rb == null) return;
            _clickableRegions[bounds] = rb;
            double sz = rb.RadioSize > 0 ? rb.RadioSize : 20;
            Point center = new Point(bounds.Left + sz / 2, bounds.Top + (bounds.Height) / 2);
            Color outerColor = rb.IsSelected
                ? ParseColor(rb.ActiveColor, Color.FromArgb(255, 108, 99, 255))
                : ParseColor(rb.InactiveColor, Color.FromArgb(255, 85, 85, 85));
            dc.DrawEllipse(null, new Pen(new SolidColorBrush(outerColor), 2), center, sz / 2, sz / 2);
            if (rb.IsSelected)
                dc.DrawEllipse(new SolidColorBrush(outerColor), null, center, sz / 4, sz / 4);
            if (!string.IsNullOrEmpty(rb.Label))
            {
                FormattedText lbl = new FormattedText(rb.Label,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 13, new SolidColorBrush(ParseColor(rb.Foreground, Colors.White)));
                dc.DrawText(lbl, new Point(bounds.Left + sz + 8, center.Y - lbl.Height / 2));
            }
        }

        // ═══════════════════════════════════════════════════════════
        // UILayout — Widget Renderers
        // ═══════════════════════════════════════════════════════════

        private void RenderNimbusDivider(DrawingContext dc, NimbusDivider div, Rect bounds)
        {
            if (div == null) return;
            Color c = ParseColor(div.DividerColor, Color.FromArgb(255, 62, 62, 66));
            double thick = div.Thickness > 0 ? div.Thickness : 1;
            if (div.Orientation == "Vertical")
            {
                double cx = bounds.Left + bounds.Width / 2;
                dc.DrawLine(new Pen(new SolidColorBrush(c), thick),
                    new Point(cx, bounds.Top), new Point(cx, bounds.Bottom));
            }
            else
            {
                double cy = bounds.Top + bounds.Height / 2;
                dc.DrawLine(new Pen(new SolidColorBrush(c), thick),
                    new Point(bounds.Left + div.Indent, cy), new Point(bounds.Right, cy));
                if (!string.IsNullOrEmpty(div.DividerText))
                {
                    FormattedText ft = new FormattedText(div.DividerText,
                        System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"), 12, new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)));
                    double tx = bounds.Left + (bounds.Width - ft.Width) / 2;
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)), null,
                        new Rect(tx - 4, cy - ft.Height / 2 - 2, ft.Width + 8, ft.Height + 4));
                    dc.DrawText(ft, new Point(tx, cy - ft.Height / 2));
                }
            }
        }

        private void RenderNimbusAvatar(DrawingContext dc, NimbusAvatar av, Rect bounds)
        {
            if (av == null) return;
            double sz = Math.Min(bounds.Width, bounds.Height);
            Point center = new Point(bounds.Left + sz / 2, bounds.Top + sz / 2);
            Color bg = ParseColor(av.AvatarColor, Color.FromArgb(255, 108, 99, 255));
            if (av.Shape == "Circle")
                dc.DrawEllipse(new SolidColorBrush(bg), null, center, sz / 2, sz / 2);
            else
                dc.DrawRoundedRectangle(new SolidColorBrush(bg), null,
                    new Rect(bounds.Left, bounds.Top, sz, sz), av.Shape == "Rounded" ? 8 : 0, av.Shape == "Rounded" ? 8 : 0);
            FormattedText initials = new FormattedText(
                av.Initials ?? "N",
                System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), System.Windows.FontStyles.Normal, System.Windows.FontWeights.SemiBold, System.Windows.FontStretches.Normal),
                sz * 0.38, new SolidColorBrush(Colors.White));
            dc.DrawText(initials, new Point(center.X - initials.Width / 2, center.Y - initials.Height / 2));
            // Status dot
            if (!string.IsNullOrEmpty(av.StatusDot))
            {
                Color dotColor = ParseColor(av.StatusDotColor, Color.FromArgb(255, 76, 175, 80));
                dc.DrawEllipse(new SolidColorBrush(dotColor), null,
                    new Point(bounds.Left + sz - 4, bounds.Top + sz - 4), 5, 5);
            }
        }

        private void RenderNimbusChip(DrawingContext dc, NimbusChip chip, Rect bounds)
        {
            if (chip == null) return;
            _clickableRegions[bounds] = chip;
            Color bg = ParseColor(chip.ChipColor, Color.FromArgb(255, 62, 62, 66));
            Color fg = ParseColor(chip.ChipTextColor, Colors.White);
            if (chip.ChipStyle == "Outlined")
            {
                dc.DrawRoundedRectangle(new SolidColorBrush(Colors.Transparent),
                    new Pen(new SolidColorBrush(bg), 1.5), bounds, bounds.Height / 2, bounds.Height / 2);
            }
            else
            {
                dc.DrawRoundedRectangle(new SolidColorBrush(bg), null, bounds, bounds.Height / 2, bounds.Height / 2);
            }
            FormattedText ft = new FormattedText(chip.Text ?? "",
                System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), chip.FontSize > 0 ? chip.FontSize : 13, new SolidColorBrush(fg));
            double offsetX = chip.IsDeletable ? bounds.Left + (bounds.Width - ft.Width - 20) / 2 : bounds.Left + (bounds.Width - ft.Width) / 2;
            dc.DrawText(ft, new Point(offsetX, bounds.Top + (bounds.Height - ft.Height) / 2));
            if (chip.IsDeletable)
            {
                FormattedText x = new FormattedText("×",
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 14, new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)));
                dc.DrawText(x, new Point(bounds.Right - 16, bounds.Top + (bounds.Height - x.Height) / 2));
            }
        }

        private void RenderDebugConsole(DrawingContext drawingContext)
        {
            // Draw debug panel at bottom
            Rect panelBounds = new Rect(0, this.ActualHeight - DebugPanelHeight, this.ActualWidth, DebugPanelHeight);
            
            // Background
            drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(240, 30, 30, 30)), null, panelBounds);
            
            // Border
            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.Cyan), 2), panelBounds);
            
            // Header
            FormattedText header = new FormattedText(
                "DEBUG CONSOLE (F12 to close)",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                12,
                new SolidColorBrush(Colors.Cyan)
            );
            drawingContext.DrawText(header, new Point(panelBounds.Left + 10, panelBounds.Top + 5));

            // Draw log entries
            double logY = panelBounds.Top + 30;
            int startLog = Math.Max(0, _debugLogs.Count - 6);

            for (int i = startLog; i < _debugLogs.Count && logY < panelBounds.Bottom; i++)
            {
                FormattedText logText = new FormattedText(
                    _debugLogs[i],
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    9,
                    new SolidColorBrush(Colors.LimeGreen)
                );
                drawingContext.DrawText(logText, new Point(panelBounds.Left + 10, logY));
                logY += 20;
            }
        }

        private double? GetComponentWidth(IUIModule module, double parentWidth)
        {
            if (module is ModuleUIElement)
            {
                string widthStr = ((ModuleUIElement)module).Width;
                if (widthStr != null)
                {
                    if (widthStr == "*") return parentWidth;
                    if (widthStr.EndsWith("%"))
                    {
                        double pct = ParseDouble(widthStr.Substring(0, widthStr.Length - 1), 100);
                        return parentWidth * pct / 100;
                    }
                    double val;
                    if (double.TryParse(widthStr, out val)) return val;
                }
            }
            return null;
        }

        private double? GetComponentHeight(IUIModule module, double parentHeight)
        {
            if (module is ModuleUIElement)
            {
                string heightStr = ((ModuleUIElement)module).Height;
                if (heightStr != null && heightStr != "Auto")
                {
                    if (heightStr == "*") return parentHeight;
                    if (heightStr.EndsWith("%"))
                    {
                        double pct = ParseDouble(heightStr.Substring(0, heightStr.Length - 1), 100);
                        return parentHeight * pct / 100;
                    }
                    double val;
                    if (double.TryParse(heightStr, out val)) return val;
                }
                
                // Dynamic height calculation for FlexPanel if Auto/Null
                CustomUIFlexPanel flex = module as CustomUIFlexPanel;
                if (flex != null)
                {
                    double totalH = 0;
                    double margin = ParseDouble(flex.Margin ?? "0", 8);
                    double gap = ParseDouble(flex.Gap ?? "0", 0);
                    bool isRow = flex.Direction.ToLower() == "row";
                    
                    foreach (var c in flex.Children)
                    {
                        double childH = GetComponentHeight(c, parentHeight) ?? 40;
                        if (isRow)
                            totalH = Math.Max(totalH, childH);
                        else
                            totalH += childH + gap;
                    }
                    // Add padding
                    double padding = ParseDouble(flex.Padding ?? "0", 0);
                    totalH += padding * 2;
                    return totalH;
                }
            }
            return null;
        }

        private Color ParseColor(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorStr)) return defaultColor;
            try { return (Color)ColorConverter.ConvertFromString(colorStr); }
            catch { return defaultColor; }
        }

        private double ParseDouble(string value, double defaultValue)
        {
            double result;
            return double.TryParse(value, out result) ? result : defaultValue;
        }
    }
}
