using System;
using System.Collections.Generic;
using System.Xml;

namespace Nimbus.WPF
{
    // ══════════════════════════════════════════════════════════
    // Context Menu — XML-driven definition
    // ══════════════════════════════════════════════════════════

    /// <summary>A single item inside a Nimbus context menu.</summary>
    public class NimbusContextMenuItemDef
    {
        public bool IsSeparator { get; set; }
        public string Icon     { get; set; }   // emoji / text icon
        public string Header   { get; set; }   // display text
        public string Shortcut { get; set; }   // e.g. Ctrl+C
        public bool   Danger   { get; set; }   // red color if true
        public string Handler  { get; set; }   // engine handler name  (optional)
        // Built-in actions: "Copy", "Paste", "SelectAll", "Delete", "Cut", "Undo"
        public string Action   { get; set; }
        // Styling overrides
        public string Foreground    { get; set; }
        public string HoverBg       { get; set; }
        public string Background    { get; set; }
    }

    /// <summary>Full context menu definition parsed from XML.</summary>
    public class NimbusContextMenuDef
    {
        public string Background   { get; set; }
        public string BorderBrush  { get; set; }
        public double CornerRadius { get; set; }
        public string HoverBg      { get; set; }
        public double FontSize     { get; set; }
        public string FontFamily   { get; set; }
        public double ItemHeight   { get; set; }
        public List<NimbusContextMenuItemDef> Items { get; set; }

        public NimbusContextMenuDef()
        {
            Background   = "#161622";
            BorderBrush  = "#37375A";
            CornerRadius = 10;
            HoverBg      = "#FFFFFF1E";
            FontSize     = 13;
            FontFamily   = "Segoe UI";
            ItemHeight   = 36;
            Items        = new List<NimbusContextMenuItemDef>();
        }
    }

    /// <summary>
    /// IUIModule - Interface for custom UI elements created without WPF
    /// Supports: Layout, Styling, Events, Rendering
    /// </summary>
    public interface IUIModule
    {
        string Id { get; set; }
        string ElementType { get; set; }
        Dictionary<string, object> Properties { get; set; }
        List<IUIModule> Children { get; set; }
        IUIModule Parent { get; set; }
        
        void SetProperty(string name, object value);
        object GetProperty(string name);
        void AddChild(IUIModule child);
        void RemoveChild(IUIModule child);
        void Render();

        // ═════════════════════════════════════════════════════════════
        // EVENT SYSTEM
        // ═════════════════════════════════════════════════════════════
        /// <summary>Add event listener (e.g., "click", "press", "longpress")</summary>
        void AddEventListener(string eventType, EventListener handler);

        /// <summary>Remove event listener</summary>
        void RemoveEventListener(string eventType, EventListener handler);

        /// <summary>Remove all listeners of a specific type or all types</summary>
        void RemoveAllEventListeners(string eventType = null);

        /// <summary>Fire event on this element</summary>
        void DispatchEvent(NimbusEvent evt);
    }

    /// <summary>
    /// UIModule - Base class for custom UI elements
    /// </summary>
    public class UIModule : IUIModule
    {
        public string Id { get; set; }
        public string ElementType { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public List<IUIModule> Children { get; set; }
        public IUIModule Parent { get; set; }

        public UIModule(string id, string type)
        {
            Id = id;
            ElementType = type;
            Properties = new Dictionary<string, object>();
            Children = new List<IUIModule>();
            Parent = null;
        }

        public void SetProperty(string name, object value)
        {
            Properties[name] = value;
        }

        public object GetProperty(string name)
        {
            if (Properties.ContainsKey(name))
                return Properties[name];
            return null;
        }

        public void AddChild(IUIModule child)
        {
            if (child != null)
            {
                Children.Add(child);
                child.Parent = this;
            }
        }

        public void RemoveChild(IUIModule child)
        {
            Children.Remove(child);
            if (child != null)
                child.Parent = null;
        }

        public virtual void Render()
        {
            // Base rendering logic - can be overridden
        }

        // ═════════════════════════════════════════════════════════════
        // EVENT SYSTEM IMPLEMENTATION
        // ═════════════════════════════════════════════════════════════
        /// <summary>Add event listener to this element</summary>
        public void AddEventListener(string eventType, EventListener handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null)
                return;

            // Store listeners in properties under special key
            string listenersKey = "__eventlisteners__";
            if (!Properties.ContainsKey(listenersKey))
                Properties[listenersKey] = new Dictionary<string, List<EventListener>>();

            var allListeners = (Dictionary<string, List<EventListener>>)Properties[listenersKey];
            if (!allListeners.ContainsKey(eventType))
                allListeners[eventType] = new List<EventListener>();

            allListeners[eventType].Add(handler);
        }

        /// <summary>Remove event listener</summary>
        public void RemoveEventListener(string eventType, EventListener handler)
        {
            if (string.IsNullOrEmpty(eventType))
                return;

            string listenersKey = "__eventlisteners__";
            if (!Properties.ContainsKey(listenersKey))
                return;

            var allListeners = (Dictionary<string, List<EventListener>>)Properties[listenersKey];
            if (allListeners.ContainsKey(eventType))
            {
                allListeners[eventType].Remove(handler);
                if (allListeners[eventType].Count == 0)
                    allListeners.Remove(eventType);
            }
        }

        /// <summary>Remove all event listeners of specific type or all types</summary>
        public void RemoveAllEventListeners(string eventType = null)
        {
            string listenersKey = "__eventlisteners__";
            if (!Properties.ContainsKey(listenersKey))
                return;

            var allListeners = (Dictionary<string, List<EventListener>>)Properties[listenersKey];
            
            if (eventType != null)
            {
                if (allListeners.ContainsKey(eventType))
                    allListeners.Remove(eventType);
            }
            else
            {
                allListeners.Clear();
            }
        }

        /// <summary>Dispatch event from this element (with full propagation)</summary>
        public void DispatchEvent(NimbusEvent evt)
        {
            if (evt == null) return;
            evt.Target = this;
            
            // Invoke local listeners
            string listenersKey = "__eventlisteners__";
            if (Properties.ContainsKey(listenersKey))
            {
                var allListeners = (Dictionary<string, List<EventListener>>)Properties[listenersKey];
                if (allListeners.ContainsKey(evt.Type))
                {
                    var listeners = new List<EventListener>(allListeners[evt.Type]);
                    foreach (var listener in listeners)
                    {
                        try { if (listener != null) listener.Invoke(evt); }
                        catch { }
                    }
                }
            }
        }

        /// <summary>Get event listeners (for debugging)</summary>
        public int GetEventListenerCount(string eventType)
        {
            string listenersKey = "__eventlisteners__";
            if (!Properties.ContainsKey(listenersKey))
                return 0;

            var allListeners = (Dictionary<string, List<EventListener>>)Properties[listenersKey];
            if (allListeners.ContainsKey(eventType))
                return allListeners[eventType].Count;
            
            return 0;
        }
    }

    /// <summary>
    /// ModuleUIElement - Specialized for UI layout elements with modern features
    /// Supports: Liquid/Flex layouts, Static/Absolute positioning, Modern effects & styling
    /// </summary>
    public class ModuleUIElement : UIModule
    {
        // ═══════════════════════════════════════════════════════════
        // BASIC STYLING
        // ═══════════════════════════════════════════════════════════
        public string Background { get; set; }
        public string Foreground { get; set; }
        public string Content { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // SIZING (supports px, %, Auto, *)
        // ═══════════════════════════════════════════════════════════
        public string Width { get; set; }  // "100px", "50%", "Auto", "*"
        public string Height { get; set; }
        public string MinWidth { get; set; }
        public string MaxWidth { get; set; }
        public string MinHeight { get; set; }
        public string MaxHeight { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // SPACING
        // ═══════════════════════════════════════════════════════════
        public string Margin { get; set; }  // "8" or "8,16" or "8,16,8,16"
        public string Padding { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // ALIGNMENT
        // ═══════════════════════════════════════════════════════════
        public string HorizontalAlignment { get; set; }  // Stretch, Left, Center, Right
        public string VerticalAlignment { get; set; }    // Stretch, Top, Center, Bottom
        
        // ═══════════════════════════════════════════════════════════
        // POSITIONING (Static/Absolute)
        // ═══════════════════════════════════════════════════════════
        public string Position { get; set; }  // "Static", "Absolute", "Relative"
        public string Left { get; set; }
        public string Top { get; set; }
        public string Right { get; set; }
        public string Bottom { get; set; }
        public int ZIndex { get; set; }
        
        // ═══════════════════════════════════════════════════════════
        // MODERN EFFECTS & STYLING
        // ═══════════════════════════════════════════════════════════
        public double CornerRadius { get; set; }  // Rounded corners
        public string Shadow { get; set; }        // "true" or shadow definition
        public double Opacity { get; set; }       // 0.0 to 1.0
        public string Transition { get; set; }    // CSS-like transition
        public string Gradient { get; set; }      // Linear/Radial gradient
        public string BorderBrush { get; set; }   // Border color
        public double BorderThickness { get; set; } // Border width
        
        // ═══════════════════════════════════════════════════════════
        // TYPOGRAPHY
        // ═══════════════════════════════════════════════════════════
        public string FontFamily { get; set; }   // Segoe UI, Arial, etc.
        public double FontSize { get; set; }
        public string FontWeight { get; set; }   // Normal, Bold, SemiBold, etc.
        public string FontStyle { get; set; }    // Normal, Italic
        public string TextDecoration { get; set; } // None, Underline, Strikethrough
        public string TextAlignment { get; set; }  // Left, Center, Right
        
        // ═══════════════════════════════════════════════════════════
        // INTERACTIONS & ADVANCED
        // ═══════════════════════════════════════════════════════════
        public string Cursor { get; set; }      // Hand, Arrow, etc.
        public string Icon { get; set; }        // Icon name/path
        public string Tooltip { get; set; }     // Tooltip text
        public bool IsEnabled { get; set; }
        public string Style { get; set; }       // Custom style name
        public string Theme { get; set; }       // Light, Dark, Accent
        public string AccentColor { get; set; } // Override accent color

        // ── XML-driven context menu (optional) ──────────────────────
        public NimbusContextMenuDef ContextMenuDef { get; set; }


        public ModuleUIElement(string id, string type) : base(id, type)
        {
            // Basic styling
            Background = "Transparent";
            Foreground = "#FFFFFF";
            Content = "";
            
            // Sizing
            Width = "Auto";
            Height = "Auto";
            MinWidth = null;
            MaxWidth = null;
            MinHeight = null;
            MaxHeight = null;
            
            // Spacing
            Margin = "0";
            Padding = "0";
            
            // Alignment
            HorizontalAlignment = "Stretch";
            VerticalAlignment = "Stretch";
            
            // Positioning
            Position = "Static";
            Left = null;
            Top = null;
            Right = null;
            Bottom = null;
            ZIndex = 0;
            
            // Modern effects
            CornerRadius = 0;
            Shadow = "false";
            Opacity = 1.0;
            Transition = null;
            Gradient = null;
            BorderBrush = null;
            BorderThickness = 0;
            
            // Typography
            FontFamily = "Segoe UI";
            FontSize = 14;
            FontWeight = "Normal";
            FontStyle = "Normal";
            TextDecoration = "None";
            TextAlignment = "Left";
            
            // Interactions
            Cursor = "Arrow";
            Icon = null;
            Tooltip = null;
            IsEnabled = true;
            Style = null;
            Theme = "Dark";
            AccentColor = "#007ACC";
        }

        public override void Render()
        {
            Console.WriteLine("[" + ElementType + "] Id=" + Id + " Background=" + Background + " CornerRadius=" + CornerRadius);
        }
    }

    /// <summary>
    /// CustomUIButton - Button implementation
    /// </summary>
    public class CustomUIButton : ModuleUIElement
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }

        public CustomUIButton(string id) : base(id, "Button")
        {
            Text = "Button";
            OnClick = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Button] Text=" + Text);
        }
    }

    /// <summary>
    /// CustomUILabel - Label/Text implementation
    /// </summary>
    public class CustomUILabel : ModuleUIElement
    {
        public string Text { get; set; }
        public double FontSize { get; set; }

        public CustomUILabel(string id) : base(id, "Label")
        {
            Text = "Label";
            FontSize = 14;
        }

        public override void Render()
        {
            Console.WriteLine("[Label] Text=" + Text + " FontSize=" + FontSize);
        }
    }

    /// <summary>
    /// CustomUIGrid - Container for layout
    /// </summary>
    public class CustomUIGrid : ModuleUIElement
    {
        public string ColumnDefinitions { get; set; }
        public string RowDefinitions { get; set; }

        public CustomUIGrid(string id) : base(id, "Grid")
        {
            ColumnDefinitions = "";
            RowDefinitions = "";
        }

        public override void Render()
        {
            Console.WriteLine("[Grid] Rows=" + RowDefinitions + " Cols=" + ColumnDefinitions);
            foreach (var child in Children)
            {
                if (child is UIModule)
                    ((UIModule)child).Render();
            }
        }
    }

    /// <summary>
    /// CustomUIStackPanel - Simple stacking container
    /// </summary>
    public class CustomUIStackPanel : ModuleUIElement
    {
        public string Orientation { get; set; }
        public double Spacing { get; set; }

        public CustomUIStackPanel(string id) : base(id, "StackPanel")
        {
            Orientation = "Vertical";
            Spacing = 0;
        }

        public override void Render()
        {
            Console.WriteLine("[StackPanel] Orientation=" + Orientation);
            foreach (var child in Children)
            {
                if (child is UIModule)
                    ((UIModule)child).Render();
            }
        }
    }

    /// <summary>
    /// CustomUIFlexPanel - Liquid/Responsive flex layout container
    /// Supports: Direction (Row/Column), Gap, JustifyContent, AlignItems
    /// </summary>
    public class CustomUIFlexPanel : ModuleUIElement
    {
        public string Direction { get; set; }         // Row, Column, RowReverse, ColumnReverse
        public string Gap { get; set; }               // Spacing between items
        public string JustifyContent { get; set; }    // Start, Center, End, SpaceBetween, SpaceAround, SpaceEvenly
        public string AlignItems { get; set; }        // Start, Center, End, Stretch
        public string AlignContent { get; set; }      // For multi-line flex
        public bool Wrap { get; set; }                // Allow wrapping

        public CustomUIFlexPanel(string id) : base(id, "FlexPanel")
        {
            Direction = "Row";
            Gap = "0";
            JustifyContent = "Start";
            AlignItems = "Stretch";
            AlignContent = "Start";
            Wrap = false;
        }

        public override void Render()
        {
            Console.WriteLine("[FlexPanel] Direction=" + Direction + " Gap=" + Gap + " JustifyContent=" + JustifyContent);
            foreach (var child in Children)
                ((UIModule)child).Render();
        }
    }

    /// <summary>
    /// CustomUIAbsolutePanel - Static/Absolute positioning container
    /// Allows children to be positioned absolutely (Left, Top, Right, Bottom, ZIndex)
    /// </summary>
    public class CustomUIAbsolutePanel : ModuleUIElement
    {
        public CustomUIAbsolutePanel(string id) : base(id, "AbsolutePanel")
        {
            Position = "Relative";  // Container is relative
        }

        public override void Render()
        {
            Console.WriteLine("[AbsolutePanel]");
            foreach (var child in Children)
                ((UIModule)child).Render();
        }
    }

    /// <summary>
    /// CustomUICard - Modern Card container with shadow and rounded corners
    /// </summary>
    public class CustomUICard : ModuleUIElement
    {
        public CustomUICard(string id) : base(id, "Card")
        {
            Background = "#2D2D30";
            CornerRadius = 8;
            Shadow = "true";
            Padding = "16";
        }

        public override void Render()
        {
            Console.WriteLine("[Card] Shadow=" + Shadow + " CornerRadius=" + CornerRadius);
            foreach (var child in Children)
                ((UIModule)child).Render();
        }
    }

    /// <summary>
    /// CustomUIModal - Modal dialog container
    /// </summary>
    public class CustomUIModal : ModuleUIElement
    {
        public string Title { get; set; }
        public bool IsVisible { get; set; }
        public CustomUIModal(string id) : base(id, "Modal")
        {
            Background = "#1E1E1E";
            CornerRadius = 12;
            Shadow = "true";
            IsVisible = false;
            Title = "Modal";
            Position = "Absolute";
            ZIndex = 1000;
        }

        public override void Render()
        {
            Console.WriteLine("[Modal] Title=" + Title + " Visible=" + IsVisible);
        }
    }

    /// <summary>
    /// CustomUIToggle - Modern toggle/checkbox control
    /// </summary>
    public class CustomUIToggle : ModuleUIElement
    {
        public bool IsChecked { get; set; }
        public string Label { get; set; }
        public Action OnChange { get; set; }
        public CustomUIToggle(string id) : base(id, "Toggle")
        {
            IsChecked = false;
            Label = "Toggle";
            OnChange = null;
            Width = "24";
            Height = "24";
            CornerRadius = 4;
        }

        public override void Render()
        {
            Console.WriteLine("[Toggle] Label=" + Label + " Checked=" + IsChecked);
        }
    }

    /// <summary>
    /// CustomUISlider - Modern slider control
    /// </summary>
    public class CustomUISlider : ModuleUIElement
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Step { get; set; }
        public CustomUISlider(string id) : base(id, "Slider")
        {
            Value = 50;
            Minimum = 0;
            Maximum = 100;
            Step = 1;
            Height = "24";
            Width = "200";
        }

        public override void Render()
        {
            Console.WriteLine("[Slider] Value=" + Value + " Min=" + Minimum + " Max=" + Maximum);
        }
    }

    /// <summary>
    /// CustomUIProgressBar - Modern progress bar control
    /// </summary>
    public class CustomUIProgressBar : ModuleUIElement
    {
        public double Progress { get; set; }  // 0 to 100
        public string ProgressColor { get; set; }
        public CustomUIProgressBar(string id) : base(id, "ProgressBar")
        {
            Progress = 0;
            ProgressColor = "#007ACC";
            Height = "8";
            Width = "100%";
            Background = "#3E3E42";
            CornerRadius = 4;
        }

        public override void Render()
        {
            Console.WriteLine("[ProgressBar] Progress=" + Progress + "%");
        }
    }

    /// <summary>
    /// CustomUIInput - Modern text input field
    /// </summary>
    public class CustomUIInput : ModuleUIElement
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string InputType { get; set; }  // Text, Password, Email, Number, etc.
        public CustomUIInput(string id) : base(id, "Input")
        {
            Value = "";
            Placeholder = "Enter text...";
            InputType = "Text";
            Height = "36";
            Padding = "8,12";
            CornerRadius = 4;
            Background = "#3E3E42";
            BorderBrush = "#555555";
            BorderThickness = 1;
        }

        public override void Render()
        {
            Console.WriteLine("[Input] Type=" + InputType + " Placeholder=" + Placeholder);
        }
    }

    /// <summary>
    /// CustomUITabs - Modern tabbed interface
    /// </summary>
    public class CustomUITabs : ModuleUIElement
    {
        public List<string> TabNames { get; set; }
        public int SelectedTabIndex { get; set; }
        public CustomUITabs(string id) : base(id, "Tabs")
        {
            TabNames = new List<string>();
            SelectedTabIndex = 0;
        }

        public override void Render()
        {
            Console.WriteLine("[Tabs] Count=" + TabNames.Count + " Selected=" + SelectedTabIndex);
        }
    }

    /// <summary>
    /// CustomUIBadge - Modern badge/chip component
    /// </summary>
    public class CustomUIBadge : ModuleUIElement
    {
        public string BadgeStyle { get; set; }  // Default, Primary, Success, Warning, Danger
        public CustomUIBadge(string id) : base(id, "Badge")
        {
            BadgeStyle = "Default";
            Padding = "4,8";
            CornerRadius = 12;
            FontSize = 12;
        }

        public override void Render()
        {
            Console.WriteLine("[Badge] Style=" + BadgeStyle + " Content=" + Content);
        }
    }

    /// <summary>
    /// CustomUITooltip - Modern tooltip overlay
    /// </summary>
    public class CustomUITooltip : ModuleUIElement
    {
        public string Position { get; set; }  // Top, Right, Bottom, Left
        public CustomUITooltip(string id) : base(id, "Tooltip")
        {
            Position = "Top";
            Background = "#333333";
            Foreground = "#FFFFFF";
            Padding = "8";
            CornerRadius = 4;
            FontSize = 12;
            ZIndex = 10000;
        }

        public override void Render()
        {
            Console.WriteLine("[Tooltip] Position=" + Position + " Content=" + Content);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UIModuleFactory - Creates any UIModule by element type name
    //  Central factory integrating ALL UILayout components
    // ═══════════════════════════════════════════════════════════════════════
    public static class UIModuleFactory
    {
        /// <summary>
        /// Create a UIModule element by type name.
        /// Supports all built-in + UILayout button, input, widget, layout types.
        /// </summary>
        public static IUIModule Create(string elementType, string elementId)
        {
            switch (elementType.ToLower())
            {
                // ══════════════ CORE CONTAINERS ══════════════
                case "grid":                return new CustomUIGrid(elementId);
                case "stackpanel":          return new CustomUIStackPanel(elementId);
                case "flexpanel":           return new CustomUIFlexPanel(elementId);
                case "absolutepanel":       return new CustomUIAbsolutePanel(elementId);
                case "card":                return new CustomUICard(elementId);
                case "modal":               return new CustomUIModal(elementId);

                // ══════════════ CORE CONTROLS ══════════════
                case "button":              return new CustomUIButton(elementId);
                case "label":
                case "text":
                case "textblock":           return new CustomUILabel(elementId);
                case "toggle":
                case "checkbox":            return new CustomUIToggle(elementId);
                case "slider":              return new CustomUISlider(elementId);
                case "progressbar":         return new CustomUIProgressBar(elementId);
                case "input":
                case "textbox":             return new CustomUIInput(elementId);
                case "tabs":                return new CustomUITabs(elementId);
                case "badge":               return new CustomUIBadge(elementId);
                case "tooltip":             return new CustomUITooltip(elementId);

                // ══════════════ UILAYOUT BUTTONS ══════════════
                case "nimbusbutton":        return new NimbusButton(elementId);
                case "iconbutton":          return new NimbusIconButton(elementId);
                case "fab":
                case "floatingactionbutton": return new NimbusFloatingActionButton(elementId);
                case "dropdownbutton":      return new NimbusDropdownButton(elementId);
                case "togglebutton":        return new NimbusToggleButton(elementId);
                case "buttongroup":
                case "segmentedbutton":     return new NimbusButtonGroup(elementId);
                case "linkbutton":
                case "hyperlink":           return new NimbusLinkButton(elementId);

                // ══════════════ UILAYOUT INPUTS ══════════════
                case "nimbustextinput":
                case "nimbuinput":
                case "textfield":           return new NimbusTextInput(elementId);
                case "textarea":
                case "nimbustextarea":      return new NimbusTextArea(elementId);
                case "searchinput":
                case "search":              return new NimbusSearchInput(elementId);
                case "passwordinput":
                case "password":            return new NimbusPasswordInput(elementId);
                case "numberinput":
                case "number":              return new NimbusNumberInput(elementId);
                case "combobox":
                case "select":
                case "dropdown":            return new NimbusComboBox(elementId);
                case "switch":
                case "nimbusswitch":        return new NimbusSwitch(elementId);
                case "nimbuscheckbox":      return new NimbusCheckBox(elementId);
                case "radiobutton":
                case "radio":               return new NimbusRadioButton(elementId);
                case "radiogroup":          return new NimbusRadioGroup(elementId);
                case "rangeslider":         return new NimbusRangeSlider(elementId);
                case "colorpicker":         return new NimbusColorPicker(elementId);
                case "datepicker":          return new NimbusDatePicker(elementId);

                // ══════════════ UILAYOUT WIDGETS ══════════════
                case "divider":
                case "separator":           return new NimbusDivider(elementId);
                case "avatar":              return new NimbusAvatar(elementId);
                case "chip":
                case "tag":                 return new NimbusChip(elementId);
                case "listtile":
                case "listitem":            return new NimbusListTile(elementId);
                case "snackbar":
                case "toast":               return new NimbusSnackbar(elementId);
                case "appbar":
                case "toolbar":
                case "navbar":              return new NimbusAppBar(elementId);
                case "bottomnav":
                case "bottomnavigation":    return new NimbusBottomNav(elementId);
                case "expander":
                case "accordion":           return new NimbusExpander(elementId);
                case "dialog":
                case "alertdialog":         return new NimbusDialog(elementId);
                case "circularprogress":
                case "spinner":
                case "loading":             return new NimbusCircularProgress(elementId);
                case "image":
                case "img":                 return new NimbusImage(elementId);
                case "scrollview":
                case "scroll":              return new NimbusScrollView(elementId);
                case "datatable":
                case "table":               return new NimbusDataTable(elementId);
                case "treeview":
                case "tree":                return new NimbusTreeView(elementId);
                case "richtext":            return new NimbusRichText(elementId);
                case "skeleton":            return new NimbusSkeleton(elementId);
                case "stepper":             return new NimbusStepper(elementId);

                // ══════════════ UILAYOUT LAYOUTS ══════════════
                case "wrappanel":
                case "flow":                return new NimbusWrapPanel(elementId);
                case "gridlayout":
                case "cssgrid":             return new NimbusGridLayout(elementId);
                case "container":           return new NimbusContainer(elementId);
                case "spacer":              return new NimbusSpacer(elementId);
                case "sizedbox":            return new NimbusSizedBox(elementId);
                case "center":              return new NimbusCenter(elementId);
                case "aspectratio":         return new NimbusAspectRatio(elementId);

                default:
                    // Fallback: create generic ModuleUIElement
                    return new ModuleUIElement(elementId, elementType);
            }
        }

        /// <summary>
        /// Get available element type names for documentation/tooling
        /// </summary>
        public static List<string> GetAvailableTypes()
        {
            return new List<string>
            {
                // Core
                "Grid", "StackPanel", "FlexPanel", "AbsolutePanel", "Card", "Modal",
                "Button", "Label", "Toggle", "Slider", "ProgressBar", "Input", "Tabs", "Badge", "Tooltip",
                // Buttons
                "NimbusButton", "IconButton", "FAB", "DropdownButton", "ToggleButton", "ButtonGroup", "LinkButton",
                // Inputs
                "NimbusTextInput", "TextArea", "SearchInput", "PasswordInput", "NumberInput",
                "ComboBox", "Switch", "NimbusCheckBox", "RadioButton", "RadioGroup",
                "RangeSlider", "ColorPicker", "DatePicker",
                // Widgets
                "Divider", "Avatar", "Chip", "ListTile", "Snackbar", "AppBar", "BottomNav",
                "Expander", "Dialog", "CircularProgress", "Image", "ScrollView",
                "DataTable", "TreeView", "RichText", "Skeleton", "Stepper",
                // Layouts
                "WrapPanel", "GridLayout", "Container", "Spacer", "SizedBox", "Center", "AspectRatio"
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UIModuleExtensions - Helper methods for building UI trees
    // ═══════════════════════════════════════════════════════════════════════
    public static class UIModuleExtensions
    {
        /// <summary>Fluent method: Set a property and return the module</summary>
        public static T SetProp<T>(this T module, string name, object value) where T : IUIModule
        {
            module.SetProperty(name, value);
            return module;
        }

        /// <summary>Fluent method: Add a child and return the parent</summary>
        public static T WithChild<T>(this T parent, IUIModule child) where T : IUIModule
        {
            parent.AddChild(child);
            return parent;
        }

        /// <summary>Fluent method: Add multiple children and return the parent</summary>
        public static T WithChildren<T>(this T parent, params IUIModule[] children) where T : IUIModule
        {
            foreach (var child in children)
                parent.AddChild(child);
            return parent;
        }

        /// <summary>Find a child element by ID recursively</summary>
        public static IUIModule FindById(this IUIModule root, string id)
        {
            if (root == null) return null;
            if (root.Id == id) return root;
            foreach (var child in root.Children)
            {
                var found = FindById(child, id);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Find all elements matching a predicate recursively</summary>
        public static List<IUIModule> FindAll(this IUIModule root, Func<IUIModule, bool> predicate)
        {
            List<IUIModule> results = new List<IUIModule>();
            if (root == null) return results;
            if (predicate(root)) results.Add(root);
            foreach (var child in root.Children)
                results.AddRange(FindAll(child, predicate));
            return results;
        }

        /// <summary>Find all elements of a specific type recursively</summary>
        public static List<T> FindByType<T>(this IUIModule root) where T : class, IUIModule
        {
            List<T> results = new List<T>();
            if (root == null) return results;
            T typed = root as T;
            if (typed != null) results.Add(typed);
            foreach (var child in root.Children)
                results.AddRange(FindByType<T>(child));
            return results;
        }

        /// <summary>Get the total count of all descendants</summary>
        public static int DescendantCount(this IUIModule root)
        {
            if (root == null) return 0;
            int count = root.Children.Count;
            foreach (var child in root.Children)
                count += DescendantCount(child);
            return count;
        }

        /// <summary>Apply theme colors from NimbusThemeData to a ModuleUIElement</summary>
        public static void ApplyTheme(this ModuleUIElement element, NimbusThemeData theme)
        {
            if (element == null || theme == null) return;
            element.Foreground = theme.OnSurface.ToHex();
            element.FontFamily = theme.FontFamily;
            element.FontSize = theme.DefaultFontSize;
            element.CornerRadius = theme.DefaultCornerRadius;
            element.AccentColor = theme.PrimaryColor.ToHex();
        }

        // ─────────── Quick Builder Helpers ───────────

        /// <summary>Create a quick NimbusButton</summary>
        public static NimbusButton QuickButton(string id, string text, Action onClick)
        {
            NimbusButton btn = new NimbusButton(id);
            btn.Text = text;
            btn.OnClick = onClick;
            return btn;
        }

        /// <summary>Create a quick NimbusTextInput</summary>
        public static NimbusTextInput QuickInput(string id, string label, string placeholder)
        {
            NimbusTextInput input = new NimbusTextInput(id);
            input.Label = label;
            input.Placeholder = placeholder;
            return input;
        }

        /// <summary>Create a quick Row (horizontal flex)</summary>
        public static CustomUIFlexPanel QuickRow(string id, double gap)
        {
            CustomUIFlexPanel panel = new CustomUIFlexPanel(id);
            panel.Direction = "Row";
            panel.Gap = gap.ToString();
            panel.AlignItems = "Center";
            return panel;
        }

        /// <summary>Create a quick Column (vertical flex)</summary>
        public static CustomUIFlexPanel QuickColumn(string id, double gap)
        {
            CustomUIFlexPanel panel = new CustomUIFlexPanel(id);
            panel.Direction = "Column";
            panel.Gap = gap.ToString();
            return panel;
        }

        /// <summary>Create a quick Card with theme</summary>
        public static CustomUICard QuickCard(string id, NimbusThemeData theme)
        {
            CustomUICard card = new CustomUICard(id);
            if (theme != null)
            {
                card.Background = theme.SurfaceColor.ToHex();
                card.CornerRadius = theme.DefaultCornerRadius;
            }
            return card;
        }

        /// <summary>Create a quick Label</summary>
        public static CustomUILabel QuickLabel(string id, string text, NimbusTextStyle style)
        {
            CustomUILabel label = new CustomUILabel(id);
            label.Text = text;
            if (style != null)
            {
                label.FontSize = style.FontSize;
                label.Foreground = style.Color;
            }
            return label;
        }

        /// <summary>Create a quick Divider</summary>
        public static NimbusDivider QuickDivider(string id)
        {
            return new NimbusDivider(id);
        }

        /// <summary>Create a quick Switch</summary>
        public static NimbusSwitch QuickSwitch(string id, string label, bool isOn, Action<bool> onToggle)
        {
            NimbusSwitch sw = new NimbusSwitch(id);
            sw.Label = label;
            sw.IsOn = isOn;
            sw.OnToggle = onToggle;
            return sw;
        }
    }
}

