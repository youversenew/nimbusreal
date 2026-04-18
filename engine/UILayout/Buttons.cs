using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  Button Style Definitions
    // ═══════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// NimbusButtonStyle - Predefined button styles (Material Design inspired)
    /// </summary>
    public enum NimbusButtonStyle
    {
        Filled,         // Solid background (default)
        Outlined,       // Border only, transparent background
        Text,           // No background or border
        Elevated,       // With shadow/elevation
        Tonal,          // Tinted background (lighter primary)
        Ghost           // Transparent, shows on hover
    }

    /// <summary>
    /// NimbusButtonSize - Predefined button sizes
    /// </summary>
    public enum NimbusButtonSize
    {
        Small,      // 28px height, 12px font
        Medium,     // 36px height, 14px font (default)
        Large,      // 44px height, 16px font
        XLarge      // 52px height, 18px font
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusButton - Full-featured button component
    //  Supports: Icons, Loading state, Variants, Sizes, Groups
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusButton : ModuleUIElement
    {
        // Content
        public string Text { get; set; }
        public string IconLeft { get; set; }        // Icon name/path before text
        public string IconRight { get; set; }       // Icon name/path after text
        
        // Style
        public NimbusButtonStyle ButtonStyle { get; set; }
        public NimbusButtonSize ButtonSize { get; set; }
        public string ButtonColor { get; set; }     // Primary color override
        public string TextColor { get; set; }       // Text color override
        public string HoverColor { get; set; }      // Hover state color
        public string PressedColor { get; set; }    // Pressed state color
        public string DisabledColor { get; set; }   // Disabled state color
        
        // State
        public bool IsLoading { get; set; }
        public bool IsPressed { get; set; }
        public bool IsHovered { get; set; }
        public string LoadingText { get; set; }
        
        // Events
        public Action OnClick { get; set; }
        public Action OnDoubleClick { get; set; }
        public Action OnLongPress { get; set; }
        public Action OnHover { get; set; }
        public Action OnHoverExit { get; set; }

        // Ripple Effect
        public bool RippleEnabled { get; set; }
        public string RippleColor { get; set; }

        public NimbusButton(string id) : base(id, "NimbusButton")
        {
            Text = "Button";
            IconLeft = null;
            IconRight = null;
            
            ButtonStyle = NimbusButtonStyle.Filled;
            ButtonSize = NimbusButtonSize.Medium;
            ButtonColor = "#6C63FF";
            TextColor = "#FFFFFF";
            HoverColor = "#7D75FF";
            PressedColor = "#5A52E0";
            DisabledColor = "#555555";
            
            IsLoading = false;
            IsPressed = false;
            IsHovered = false;
            LoadingText = "Loading...";
            
            OnClick = null;
            OnDoubleClick = null;
            OnLongPress = null;
            OnHover = null;
            OnHoverExit = null;
            
            RippleEnabled = true;
            RippleColor = "#FFFFFF";
            
            ApplySizeDefaults();
        }

        private void ApplySizeDefaults()
        {
            switch (ButtonSize)
            {
                case NimbusButtonSize.Small:
                    Height = "28";
                    FontSize = 12;
                    Padding = "4,12";
                    CornerRadius = 4;
                    break;
                case NimbusButtonSize.Medium:
                    Height = "36";
                    FontSize = 14;
                    Padding = "8,16";
                    CornerRadius = 6;
                    break;
                case NimbusButtonSize.Large:
                    Height = "44";
                    FontSize = 16;
                    Padding = "10,24";
                    CornerRadius = 8;
                    break;
                case NimbusButtonSize.XLarge:
                    Height = "52";
                    FontSize = 18;
                    Padding = "12,32";
                    CornerRadius = 10;
                    break;
            }
        }

        /// <summary>Get the effective background color based on state</summary>
        public string GetEffectiveBackground()
        {
            if (!IsEnabled) return DisabledColor;
            if (IsPressed) return PressedColor;
            if (IsHovered) return HoverColor;
            
            switch (ButtonStyle)
            {
                case NimbusButtonStyle.Filled:
                case NimbusButtonStyle.Elevated:
                    return ButtonColor;
                case NimbusButtonStyle.Tonal:
                    return ButtonColor + "33"; // 20% opacity
                case NimbusButtonStyle.Outlined:
                case NimbusButtonStyle.Text:
                case NimbusButtonStyle.Ghost:
                    return "Transparent";
                default:
                    return ButtonColor;
            }
        }

        /// <summary>Get the effective border color based on style</summary>
        public string GetEffectiveBorder()
        {
            switch (ButtonStyle)
            {
                case NimbusButtonStyle.Outlined:
                    return IsEnabled ? ButtonColor : DisabledColor;
                default:
                    return "Transparent";
            }
        }

        public override void Render()
        {
            Console.WriteLine("[NimbusButton] Text=" + Text + " Style=" + ButtonStyle + " Size=" + ButtonSize);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusIconButton - Icon-only circular/square button
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusIconButton : ModuleUIElement
    {
        public string Icon { get; set; }
        public string IconColor { get; set; }
        public double IconSize { get; set; }
        public bool IsCircular { get; set; }
        public NimbusButtonStyle ButtonStyle { get; set; }
        public string ButtonColor { get; set; }
        public string HoverColor { get; set; }
        public bool IsToggled { get; set; }
        public string ToggledColor { get; set; }
        public string ToggledIconColor { get; set; }
        
        public Action OnClick { get; set; }
        public Action<bool> OnToggle { get; set; }
        public bool RippleEnabled { get; set; }

        public NimbusIconButton(string id) : base(id, "IconButton")
        {
            Icon = "●";
            IconColor = "#FFFFFF";
            IconSize = 20;
            IsCircular = true;
            ButtonStyle = NimbusButtonStyle.Ghost;
            ButtonColor = "Transparent";
            HoverColor = "#FFFFFF1A";
            IsToggled = false;
            ToggledColor = "#6C63FF";
            ToggledIconColor = "#FFFFFF";
            
            Width = "40";
            Height = "40";
            CornerRadius = 20;
            
            OnClick = null;
            OnToggle = null;
            RippleEnabled = true;
        }

        public override void Render()
        {
            Console.WriteLine("[IconButton] Icon=" + Icon + " Circular=" + IsCircular);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusFloatingActionButton - FAB (Material Design)
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusFloatingActionButton : ModuleUIElement
    {
        public string Icon { get; set; }
        public string Text { get; set; }           // For extended FAB
        public bool IsExtended { get; set; }       // Show text alongside icon
        public bool IsMini { get; set; }           // Smaller FAB
        public string FabColor { get; set; }
        public string IconColor { get; set; }
        public double Elevation { get; set; }
        
        public Action OnClick { get; set; }

        public NimbusFloatingActionButton(string id) : base(id, "FAB")
        {
            Icon = "+";
            Text = "";
            IsExtended = false;
            IsMini = false;
            FabColor = "#6C63FF";
            IconColor = "#FFFFFF";
            Elevation = 6;
            
            Width = "56";
            Height = "56";
            CornerRadius = 16;
            Shadow = "true";
            
            Position = "Absolute";
            Right = "16";
            Bottom = "16";
            ZIndex = 100;
            
            OnClick = null;
        }

        public override void Render()
        {
            Console.WriteLine("[FAB] Icon=" + Icon + " Extended=" + IsExtended);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusDropdownButton - Button with dropdown menu
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusDropdownButton : ModuleUIElement
    {
        public string Text { get; set; }
        public List<NimbusDropdownItem> Items { get; set; }
        public bool IsOpen { get; set; }
        public string DropdownColor { get; set; }
        public NimbusButtonStyle ButtonStyle { get; set; }
        public string ButtonColor { get; set; }
        
        public Action OnClick { get; set; }
        public Action<string> OnItemSelected { get; set; }

        public NimbusDropdownButton(string id) : base(id, "DropdownButton")
        {
            Text = "Select";
            Items = new List<NimbusDropdownItem>();
            IsOpen = false;
            DropdownColor = "#2D2D30";
            ButtonStyle = NimbusButtonStyle.Filled;
            ButtonColor = "#6C63FF";
            
            Height = "36";
            CornerRadius = 6;
            
            OnClick = null;
            OnItemSelected = null;
        }

        public override void Render()
        {
            Console.WriteLine("[DropdownButton] Text=" + Text + " Items=" + Items.Count + " Open=" + IsOpen);
        }
    }

    public class NimbusDropdownItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDivider { get; set; }
        public Action OnClick { get; set; }

        public NimbusDropdownItem()
        {
            Id = "";
            Text = "";
            Icon = null;
            IsEnabled = true;
            IsDivider = false;
            OnClick = null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusToggleButton - Toggle/switch style button
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusToggleButton : ModuleUIElement
    {
        public string Text { get; set; }
        public string ActiveText { get; set; }     // Text when toggled on
        public bool IsToggled { get; set; }
        public string ActiveColor { get; set; }
        public string InactiveColor { get; set; }
        public string ActiveTextColor { get; set; }
        public string InactiveTextColor { get; set; }
        public string Icon { get; set; }
        public string ActiveIcon { get; set; }
        
        public Action<bool> OnToggle { get; set; }

        public NimbusToggleButton(string id) : base(id, "ToggleButton")
        {
            Text = "Off";
            ActiveText = "On";
            IsToggled = false;
            ActiveColor = "#6C63FF";
            InactiveColor = "#3E3E42";
            ActiveTextColor = "#FFFFFF";
            InactiveTextColor = "#AAAAAA";
            Icon = null;
            ActiveIcon = null;
            
            Height = "36";
            CornerRadius = 6;
            
            OnToggle = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ToggleButton] Text=" + Text + " Toggled=" + IsToggled);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusButtonGroup - Segmented button group
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusButtonGroup : ModuleUIElement
    {
        public List<string> ButtonLabels { get; set; }
        public int SelectedIndex { get; set; }
        public bool AllowMultiple { get; set; }
        public List<int> SelectedIndices { get; set; }
        public string ActiveColor { get; set; }
        public string InactiveColor { get; set; }
        public string ActiveTextColor { get; set; }
        public string InactiveTextColor { get; set; }
        public string BorderColor { get; set; }
        
        public Action<int> OnSelectionChanged { get; set; }

        public NimbusButtonGroup(string id) : base(id, "ButtonGroup")
        {
            ButtonLabels = new List<string>();
            SelectedIndex = 0;
            AllowMultiple = false;
            SelectedIndices = new List<int>();
            ActiveColor = "#6C63FF";
            InactiveColor = "#2D2D30";
            ActiveTextColor = "#FFFFFF";
            InactiveTextColor = "#AAAAAA";
            BorderColor = "#555555";
            
            Height = "36";
            CornerRadius = 6;
            
            OnSelectionChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ButtonGroup] Labels=" + ButtonLabels.Count + " Selected=" + SelectedIndex);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusLinkButton - Hyperlink-style button
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusLinkButton : ModuleUIElement
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public string LinkColor { get; set; }
        public string HoverColor { get; set; }
        public string VisitedColor { get; set; }
        public bool ShowUnderline { get; set; }
        public bool IsVisited { get; set; }
        
        public Action OnClick { get; set; }

        public NimbusLinkButton(string id) : base(id, "LinkButton")
        {
            Text = "Link";
            Url = "";
            LinkColor = "#6C63FF";
            HoverColor = "#9D97FF";
            VisitedColor = "#9D97FF";
            ShowUnderline = true;
            IsVisited = false;
            
            Height = "Auto";
            Cursor = "Hand";
            
            OnClick = null;
        }

        public override void Render()
        {
            Console.WriteLine("[LinkButton] Text=" + Text + " Url=" + Url);
        }
    }
}
