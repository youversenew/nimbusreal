using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  NimbusDivider - Horizontal or vertical separator
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusDivider : ModuleUIElement
    {
        public string Orientation { get; set; }     // Horizontal, Vertical
        public double Thickness { get; set; }
        public string DividerColor { get; set; }
        public string DividerStyle { get; set; }    // Solid, Dashed, Dotted
        public string DividerText { get; set; }     // Text in middle of divider (e.g., "OR")
        public double Indent { get; set; }          // Left indent

        public NimbusDivider(string id) : base(id, "Divider")
        {
            Orientation = "Horizontal";
            Thickness = 1;
            DividerColor = "#3E3E42";
            DividerStyle = "Solid";
            DividerText = null;
            Indent = 0;
            
            Height = "1";
            Width = "Auto";
            Margin = "0,8";
        }

        public override void Render()
        {
            Console.WriteLine("[Divider] Orientation=" + Orientation + " Thickness=" + Thickness);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusAvatar - User avatar/profile picture
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusAvatar : ModuleUIElement
    {
        public string ImageSource { get; set; }
        public string Initials { get; set; }       // Fallback when no image
        public string AvatarColor { get; set; }    // Background for initials
        public double Size { get; set; }
        public string Shape { get; set; }          // Circle, Square, Rounded
        public string StatusDot { get; set; }       // Online, Offline, Away, DND, null
        public string StatusDotColor { get; set; }
        public string BorderColor { get; set; }
        public double AvatarBorderWidth { get; set; }

        public NimbusAvatar(string id) : base(id, "Avatar")
        {
            ImageSource = null;
            Initials = "N";
            AvatarColor = "#6C63FF";
            Size = 40;
            Shape = "Circle";
            StatusDot = null;
            StatusDotColor = "#4CAF50";
            BorderColor = null;
            AvatarBorderWidth = 0;
            
            Width = "40";
            Height = "40";
            CornerRadius = 20;
        }

        public override void Render()
        {
            Console.WriteLine("[Avatar] Initials=" + Initials + " Size=" + Size + " Shape=" + Shape);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusChip - Tag/chip component
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusChip : ModuleUIElement
    {
        public string Text { get; set; }
        public string ChipColor { get; set; }
        public string ChipTextColor { get; set; }
        public string ChipStyle { get; set; }      // Filled, Outlined
        public bool IsDeletable { get; set; }
        public bool IsSelected { get; set; }
        public string Icon { get; set; }
        public string AvatarText { get; set; }     // Small avatar on left

        public Action OnClick { get; set; }
        public Action OnDelete { get; set; }

        public NimbusChip(string id) : base(id, "Chip")
        {
            Text = "Chip";
            ChipColor = "#3E3E42";
            ChipTextColor = "#E0E0E0";
            ChipStyle = "Filled";
            IsDeletable = false;
            IsSelected = false;
            Icon = null;
            AvatarText = null;
            
            Height = "32";
            Padding = "4,12";
            CornerRadius = 16;
            FontSize = 13;
            
            OnClick = null;
            OnDelete = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Chip] Text=" + Text + " Deletable=" + IsDeletable);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusListTile - List item with leading, title, subtitle, trailing
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusListTile : ModuleUIElement
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string LeadingIcon { get; set; }
        public string LeadingAvatarText { get; set; }
        public string TrailingText { get; set; }
        public string TrailingIcon { get; set; }
        public bool ShowDivider { get; set; }
        public bool IsSelected { get; set; }
        public string SelectedColor { get; set; }
        public double TileHeight { get; set; }
        public bool Dense { get; set; }            // Compact mode

        public Action OnTap { get; set; }
        public Action OnLongPress { get; set; }

        public NimbusListTile(string id) : base(id, "ListTile")
        {
            Title = "";
            Subtitle = "";
            LeadingIcon = null;
            LeadingAvatarText = null;
            TrailingText = null;
            TrailingIcon = null;
            ShowDivider = true;
            IsSelected = false;
            SelectedColor = "#6C63FF1A";
            TileHeight = 56;
            Dense = false;
            
            Height = "56";
            Padding = "16";
            Background = "Transparent";
            Foreground = "#E0E0E0";
            
            OnTap = null;
            OnLongPress = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ListTile] Title=" + Title + " Subtitle=" + Subtitle);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSnackbar - Toast notification bar
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSnackbar : ModuleUIElement
    {
        public string Message { get; set; }
        public string ActionText { get; set; }
        public string SnackbarType { get; set; }   // Info, Success, Warning, Error
        public int DurationMs { get; set; }
        public bool IsVisible { get; set; }
        public string SnackbarPosition { get; set; }  // TopCenter, BottomCenter, TopRight, etc.
        public bool ShowCloseButton { get; set; }
        public string Icon { get; set; }

        public Action OnAction { get; set; }
        public Action OnDismissed { get; set; }

        public NimbusSnackbar(string id) : base(id, "Snackbar")
        {
            Message = "";
            ActionText = null;
            SnackbarType = "Info";
            DurationMs = 4000;
            IsVisible = false;
            SnackbarPosition = "BottomCenter";
            ShowCloseButton = true;
            Icon = null;
            
            Height = "48";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#323232";
            Foreground = "#FFFFFF";
            Shadow = "true";
            ZIndex = 9000;
            
            OnAction = null;
            OnDismissed = null;
        }

        /// <summary>Get the background color based on snackbar type</summary>
        public string GetTypeColor()
        {
            switch (SnackbarType.ToLower())
            {
                case "success": return "#2E7D32";
                case "warning": return "#F57F17";
                case "error": return "#C62828";
                case "info":
                default: return "#323232";
            }
        }

        public override void Render()
        {
            Console.WriteLine("[Snackbar] Message=" + Message + " Type=" + SnackbarType + " Visible=" + IsVisible);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusAppBar - Top app bar / navigation bar
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusAppBar : ModuleUIElement
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string LeadingIcon { get; set; }    // Back/menu icon
        public List<string> ActionIcons { get; set; }
        public string AppBarColor { get; set; }
        public double Elevation { get; set; }
        public bool CenterTitle { get; set; }
        public string AppBarStyle { get; set; }    // Standard, Prominent, Dense

        public Action OnLeadingPressed { get; set; }
        public Action<int> OnActionPressed { get; set; }

        public NimbusAppBar(string id) : base(id, "AppBar")
        {
            Title = "App";
            Subtitle = null;
            LeadingIcon = null;
            ActionIcons = new List<string>();
            AppBarColor = "#1E1E1E";
            Elevation = 4;
            CenterTitle = false;
            AppBarStyle = "Standard";
            
            Height = "56";
            Width = "Auto";
            Padding = "4,16";
            Background = "#1E1E1E";
            Foreground = "#FFFFFF";
            Shadow = "true";
            ZIndex = 500;
            
            OnLeadingPressed = null;
            OnActionPressed = null;
        }

        public override void Render()
        {
            Console.WriteLine("[AppBar] Title=" + Title + " Actions=" + ActionIcons.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusBottomNav - Bottom navigation bar
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusBottomNav : ModuleUIElement
    {
        public List<NimbusNavItem> Items { get; set; }
        public int SelectedIndex { get; set; }
        public string NavColor { get; set; }
        public string ActiveColor { get; set; }
        public string InactiveColor { get; set; }
        public bool ShowLabels { get; set; }
        public double Elevation { get; set; }

        public Action<int> OnItemSelected { get; set; }

        public NimbusBottomNav(string id) : base(id, "BottomNav")
        {
            Items = new List<NimbusNavItem>();
            SelectedIndex = 0;
            NavColor = "#1E1E1E";
            ActiveColor = "#6C63FF";
            InactiveColor = "#9E9E9E";
            ShowLabels = true;
            Elevation = 8;
            
            Height = "56";
            Width = "Auto";
            Background = "#1E1E1E";
            Shadow = "true";
            ZIndex = 500;
            Position = "Absolute";
            Bottom = "0";
            Left = "0";
            Right = "0";
            
            OnItemSelected = null;
        }

        public override void Render()
        {
            Console.WriteLine("[BottomNav] Items=" + Items.Count + " Selected=" + SelectedIndex);
        }
    }

    public class NimbusNavItem
    {
        public string Icon { get; set; }
        public string ActiveIcon { get; set; }
        public string Label { get; set; }
        public int BadgeCount { get; set; }

        public NimbusNavItem()
        {
            Icon = "●";
            ActiveIcon = null;
            Label = "";
            BadgeCount = 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusExpander - Expandable/collapsible section (Accordion)
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusExpander : ModuleUIElement
    {
        public string HeaderText { get; set; }
        public string HeaderIcon { get; set; }
        public bool IsExpanded { get; set; }
        public string ExpandIcon { get; set; }
        public string CollapseIcon { get; set; }
        public bool ShowDivider { get; set; }
        public string HeaderBackground { get; set; }
        public string ContentBackground { get; set; }

        public Action<bool> OnExpandChanged { get; set; }

        public NimbusExpander(string id) : base(id, "Expander")
        {
            HeaderText = "Section";
            HeaderIcon = null;
            IsExpanded = false;
            ExpandIcon = "▶";
            CollapseIcon = "▼";
            ShowDivider = true;
            HeaderBackground = "#2D2D30";
            ContentBackground = "#252525";
            
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#3E3E42";
            BorderThickness = 1;
            
            OnExpandChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Expander] Header=" + HeaderText + " Expanded=" + IsExpanded);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusDialog - Modal dialog with action buttons
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusDialog : ModuleUIElement
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string DialogIcon { get; set; }
        public bool IsVisible { get; set; }
        public string DialogType { get; set; }     // Info, Confirm, Warning, Error, Custom
        public string PrimaryButtonText { get; set; }
        public string SecondaryButtonText { get; set; }
        public string PrimaryButtonColor { get; set; }
        public bool ShowOverlay { get; set; }
        public bool DismissOnOverlayClick { get; set; }
        public double DialogWidth { get; set; }

        public Action OnPrimaryAction { get; set; }
        public Action OnSecondaryAction { get; set; }
        public Action OnDismissed { get; set; }

        public NimbusDialog(string id) : base(id, "Dialog")
        {
            Title = "Dialog";
            Message = "";
            DialogIcon = null;
            IsVisible = false;
            DialogType = "Info";
            PrimaryButtonText = "OK";
            SecondaryButtonText = "Cancel";
            PrimaryButtonColor = "#6C63FF";
            ShowOverlay = true;
            DismissOnOverlayClick = true;
            DialogWidth = 400;
            
            Background = "#2D2D30";
            CornerRadius = 12;
            Shadow = "true";
            Padding = "24";
            ZIndex = 10000;
            Position = "Absolute";
            
            OnPrimaryAction = null;
            OnSecondaryAction = null;
            OnDismissed = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Dialog] Title=" + Title + " Type=" + DialogType + " Visible=" + IsVisible);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusCircularProgress - Circular progress/loading indicator
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusCircularProgress : ModuleUIElement
    {
        public double Progress { get; set; }       // 0-100, -1 for indeterminate
        public bool IsIndeterminate { get; set; }
        public double StrokeWidth { get; set; }
        public double Size { get; set; }
        public string ProgressColor { get; set; }
        public string TrackColor { get; set; }
        public bool ShowLabel { get; set; }
        public string LabelFormat { get; set; }    // "{0}%" or custom

        public NimbusCircularProgress(string id) : base(id, "CircularProgress")
        {
            Progress = 0;
            IsIndeterminate = false;
            StrokeWidth = 4;
            Size = 40;
            ProgressColor = "#6C63FF";
            TrackColor = "#3E3E42";
            ShowLabel = false;
            LabelFormat = "{0}%";
            
            Width = "40";
            Height = "40";
        }

        public override void Render()
        {
            Console.WriteLine("[CircularProgress] Progress=" + Progress + " Indeterminate=" + IsIndeterminate);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusImage - Image display component
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusImage : ModuleUIElement
    {
        public string Source { get; set; }
        public string AltText { get; set; }
        public string Fit { get; set; }            // Cover, Contain, Fill, None
        public string PlaceholderColor { get; set; }
        public bool ShowLoadingIndicator { get; set; }
        public bool IsLoaded { get; set; }

        public Action OnLoaded { get; set; }
        public Action OnError { get; set; }
        public Action OnClick { get; set; }

        public NimbusImage(string id) : base(id, "Image")
        {
            Source = null;
            AltText = "";
            Fit = "Cover";
            PlaceholderColor = "#3E3E42";
            ShowLoadingIndicator = true;
            IsLoaded = false;
            
            Width = "Auto";
            Height = "Auto";
            
            OnLoaded = null;
            OnError = null;
            OnClick = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Image] Source=" + Source + " Fit=" + Fit);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusScrollView - Scrollable container
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusScrollView : ModuleUIElement
    {
        public string ScrollDirection { get; set; }  // Vertical, Horizontal, Both
        public bool ShowScrollbar { get; set; }
        public double ScrollPosition { get; set; }
        public string ScrollbarColor { get; set; }
        public double ScrollbarWidth { get; set; }
        public bool SmoothScroll { get; set; }

        public Action<double> OnScroll { get; set; }
        public Action OnScrollEnd { get; set; }

        public NimbusScrollView(string id) : base(id, "ScrollView")
        {
            ScrollDirection = "Vertical";
            ShowScrollbar = true;
            ScrollPosition = 0;
            ScrollbarColor = "#555555";
            ScrollbarWidth = 6;
            SmoothScroll = true;
            
            Background = "Transparent";
            
            OnScroll = null;
            OnScrollEnd = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ScrollView] Direction=" + ScrollDirection + " Position=" + ScrollPosition);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusDataTable - Data table/grid component
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusDataTable : ModuleUIElement
    {
        public List<string> Columns { get; set; }
        public List<List<string>> Rows { get; set; }
        public bool Sortable { get; set; }
        public int SortColumn { get; set; }
        public bool SortAscending { get; set; }
        public bool ShowBorders { get; set; }
        public bool StripedRows { get; set; }
        public bool Hoverable { get; set; }
        public bool Selectable { get; set; }
        public List<int> SelectedRows { get; set; }
        public string HeaderBackground { get; set; }
        public string HeaderForeground { get; set; }
        public double RowHeight { get; set; }
        public double HeaderHeight { get; set; }
        
        public Action<int> OnRowSelected { get; set; }
        public Action<int, bool> OnSortChanged { get; set; }

        public NimbusDataTable(string id) : base(id, "DataTable")
        {
            Columns = new List<string>();
            Rows = new List<List<string>>();
            Sortable = false;
            SortColumn = -1;
            SortAscending = true;
            ShowBorders = true;
            StripedRows = true;
            Hoverable = true;
            Selectable = false;
            SelectedRows = new List<int>();
            HeaderBackground = "#2D2D30";
            HeaderForeground = "#E0E0E0";
            RowHeight = 40;
            HeaderHeight = 44;
            
            Background = "#1E1E1E";
            CornerRadius = 8;
            BorderBrush = "#3E3E42";
            BorderThickness = 1;
            
            OnRowSelected = null;
            OnSortChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[DataTable] Columns=" + Columns.Count + " Rows=" + Rows.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusTreeView - Hierarchical tree view
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusTreeView : ModuleUIElement
    {
        public List<NimbusTreeNode> Nodes { get; set; }
        public string SelectedNodeId { get; set; }
        public bool ShowIcons { get; set; }
        public bool ShowConnectors { get; set; }    // Tree lines
        public double IndentSize { get; set; }

        public Action<string> OnNodeSelected { get; set; }
        public Action<string, bool> OnNodeExpanded { get; set; }

        public NimbusTreeView(string id) : base(id, "TreeView")
        {
            Nodes = new List<NimbusTreeNode>();
            SelectedNodeId = null;
            ShowIcons = true;
            ShowConnectors = true;
            IndentSize = 20;
            
            Background = "Transparent";
            Foreground = "#E0E0E0";
            
            OnNodeSelected = null;
            OnNodeExpanded = null;
        }

        public override void Render()
        {
            Console.WriteLine("[TreeView] Nodes=" + Nodes.Count + " Selected=" + SelectedNodeId);
        }
    }

    public class NimbusTreeNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public bool IsExpanded { get; set; }
        public List<NimbusTreeNode> Children { get; set; }

        public NimbusTreeNode()
        {
            Id = "";
            Text = "";
            Icon = null;
            IsExpanded = false;
            Children = new List<NimbusTreeNode>();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusRichText - Rich text display component
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusRichText : ModuleUIElement
    {
        public string Text { get; set; }
        public List<NimbusTextSpan> Spans { get; set; }
        public bool Selectable { get; set; }
        public double LineHeight { get; set; }
        public int MaxLines { get; set; }
        public string Overflow { get; set; }       // Ellipsis, Clip, Visible

        public NimbusRichText(string id) : base(id, "RichText")
        {
            Text = "";
            Spans = new List<NimbusTextSpan>();
            Selectable = true;
            LineHeight = 1.5;
            MaxLines = -1;
            Overflow = "Visible";
            
            Foreground = "#E0E0E0";
            FontSize = 14;
        }

        public override void Render()
        {
            Console.WriteLine("[RichText] Text=" + (Text.Length > 30 ? Text.Substring(0, 30) + "..." : Text));
        }
    }

    public class NimbusTextSpan
    {
        public string Text { get; set; }
        public string Color { get; set; }
        public double FontSize { get; set; }
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
        public string Decoration { get; set; }     // Underline, Strikethrough
        public string Url { get; set; }            // If this span is a link

        public NimbusTextSpan()
        {
            Text = "";
            Color = null;
            FontSize = 14;
            FontWeight = "Normal";
            FontStyle = "Normal";
            Decoration = "None";
            Url = null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSkeleton - Skeleton loading placeholder
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSkeleton : ModuleUIElement
    {
        public string SkeletonType { get; set; }   // Text, Circle, Rectangle, Card
        public bool IsAnimated { get; set; }
        public string BaseColor { get; set; }
        public string HighlightColor { get; set; }
        public int LineCount { get; set; }         // For text skeleton

        public NimbusSkeleton(string id) : base(id, "Skeleton")
        {
            SkeletonType = "Rectangle";
            IsAnimated = true;
            BaseColor = "#2D2D30";
            HighlightColor = "#3E3E42";
            LineCount = 3;
            
            CornerRadius = 4;
            Height = "20";
            Width = "Auto";
        }

        public override void Render()
        {
            Console.WriteLine("[Skeleton] Type=" + SkeletonType + " Animated=" + IsAnimated);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusStepper - Step progress indicator
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusStepper : ModuleUIElement
    {
        public List<string> Steps { get; set; }
        public int CurrentStep { get; set; }
        public string Orientation { get; set; }    // Horizontal, Vertical
        public string ActiveColor { get; set; }
        public string CompletedColor { get; set; }
        public string InactiveColor { get; set; }
        public bool ShowLabels { get; set; }
        public bool Clickable { get; set; }

        public Action<int> OnStepChanged { get; set; }

        public NimbusStepper(string id) : base(id, "Stepper")
        {
            Steps = new List<string>();
            CurrentStep = 0;
            Orientation = "Horizontal";
            ActiveColor = "#6C63FF";
            CompletedColor = "#4CAF50";
            InactiveColor = "#555555";
            ShowLabels = true;
            Clickable = false;
            
            Height = "60";
            
            OnStepChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Stepper] Steps=" + Steps.Count + " Current=" + CurrentStep);
        }
    }
}
