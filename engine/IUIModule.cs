using System;
using System.Collections.Generic;
using System.Xml;

namespace Nimbus.WPF
{
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
        public CustomUIToggle(string id) : base(id, "Toggle")
        {
            IsChecked = false;
            Label = "Toggle";
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
}
