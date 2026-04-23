using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
// WpfUI.cs fayl boshiga qo'shing:
using System.Windows.Input;
namespace Nimbus.WPF
{
    public class WpfUI
    {
        private WpfEngine _engine;
        private Dictionary<string, StyleDefinition> _styles;
        
        public WpfUI(WpfEngine engine)  
        {
            _engine = engine;
            _styles = new Dictionary<string, StyleDefinition>();
        }
        
        public void ParseStyles(XmlNode stylesNode)
{
    if (stylesNode == null) return;
    
    foreach (XmlNode child in stylesNode.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;
        
        if (child.Name == "Style")
        {
            string name = GetAttribute(child, "Name", "");
            string targetType = GetAttribute(child, "TargetType", "");
            
            if (!string.IsNullOrEmpty(name))
            {
                StyleDefinition style = new StyleDefinition();
                style.Name = name;
                style.TargetType = targetType;
                style.Properties = new Dictionary<string, string>();
                
                foreach (XmlNode propNode in child.ChildNodes)
                {
                    if (propNode.Name == "Setter")
                    {
                        string prop = GetAttribute(propNode, "Property", "");
                        string val = GetAttribute(propNode, "Value", "");
                        if (!string.IsNullOrEmpty(prop))
                        {
                            style.Properties[prop] = val;
                        }
                    }
                }
                
                _styles[name] = style;
            }
        }
    }
}
        
        public Window CreateWindow(XmlNode rootNode, XmlNode uiNode)
{
    // inModule=true bo'lsa bu method chaqirilmasligi kerak, lekin guard qolamiz
    if (rootNode != null)
    {
        bool useInModule = GetBoolAttribute(rootNode, "inModule", false);
        if (useInModule)
        {
            _engine.Log("WARN", "WpfUI.CreateWindow called with inModule=true - returning null");
            return null;
        }
    }

    string xaml = ConvertToXaml(rootNode, uiNode);

    try { File.WriteAllText("debug_output.xaml", xaml, Encoding.UTF8); } catch { }

    Window window = null;
    try
    {
        using (StringReader stringReader = new StringReader(xaml))
        using (XmlReader xmlReader = XmlReader.Create(stringReader))
        {
            window = (Window)XamlReader.Load(xmlReader);
        }
    }
    catch (Exception ex)
    {
        window = CreateErrorWindow(ex, xaml);
    }

    // Register controls IMMEDIATELY after XAML load, not in Loaded event
    if (window != null && window.Content != null)
    {
        RegisterNamedControlsFromContent(window);
    }

    return window;
}

private void RegisterNamedControlsFromContent(Window window)
{
    if (window.Content is FrameworkElement)
    {
        RegisterFromLogicalTree((FrameworkElement)window.Content);
    }
}

private void RegisterFromLogicalTree(FrameworkElement element)
{
    if (element == null) return;

    if (!string.IsNullOrEmpty(element.Name))
    {
        _engine.RegisterControl(element.Name, element);
    }

    if (element is Panel)
    {
        Panel panel = (Panel)element;
        foreach (UIElement child in panel.Children)
        {
            if (child is FrameworkElement)
            {
                RegisterFromLogicalTree((FrameworkElement)child);
            }
        }
    }
    else if (element is ContentControl)
    {
        ContentControl cc = (ContentControl)element;
        if (cc.Content is FrameworkElement)
        {
            RegisterFromLogicalTree((FrameworkElement)cc.Content);
        }
    }
    else if (element is Decorator)
    {
        Decorator dec = (Decorator)element;
        if (dec.Child is FrameworkElement)
        {
            RegisterFromLogicalTree((FrameworkElement)dec.Child);
        }
    }
    else if (element is ItemsControl)
    {
        ItemsControl ic = (ItemsControl)element;
        for (int i = 0; i < ic.Items.Count; i++)
        {
            if (ic.Items[i] is FrameworkElement)
            {
                RegisterFromLogicalTree((FrameworkElement)ic.Items[i]);
            }
        }
    }
    else if (element is ScrollViewer)
    {
        ScrollViewer sv = (ScrollViewer)element;
        if (sv.Content is FrameworkElement)
        {
            RegisterFromLogicalTree((FrameworkElement)sv.Content);
        }
    }
}
        
        private Window CreateErrorWindow(Exception ex, string xaml)
        {
            Window window = new Window();
            window.Title = "XAML Error";
            window.Width = 700;
            window.Height = 500;
            window.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            
            ScrollViewer scroll = new ScrollViewer();
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(20);
            
            TextBlock errorTitle = new TextBlock();
            errorTitle.Text = "XAML Parse Error:";
            errorTitle.Foreground = Brushes.Red;
            errorTitle.FontSize = 18;
            errorTitle.FontWeight = FontWeights.Bold;
            panel.Children.Add(errorTitle);
            
            TextBox errorText = new TextBox();
            errorText.Text = ex.Message + "\n\n" + ex.StackTrace;
            errorText.Foreground = Brushes.White;
            errorText.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));
            errorText.IsReadOnly = true;
            errorText.TextWrapping = TextWrapping.Wrap;
            errorText.AcceptsReturn = true;
            errorText.Height = 150;
            errorText.Margin = new Thickness(0, 10, 0, 10);
            panel.Children.Add(errorText);
            
            TextBlock xamlTitle = new TextBlock();
            xamlTitle.Text = "Generated XAML:";
            xamlTitle.Foreground = Brushes.Yellow;
            xamlTitle.FontSize = 14;
            panel.Children.Add(xamlTitle);
            
            TextBox xamlText = new TextBox();
            xamlText.Text = xaml;
            xamlText.Foreground = Brushes.LightGreen;
            xamlText.Background = new SolidColorBrush(Color.FromRgb(25, 25, 25));
            xamlText.FontFamily = new FontFamily("Consolas");
            xamlText.IsReadOnly = true;
            xamlText.TextWrapping = TextWrapping.Wrap;
            xamlText.AcceptsReturn = true;
            xamlText.Height = 250;
            panel.Children.Add(xamlText);
            
            scroll.Content = panel;
            window.Content = scroll;
            
            return window;
        }
        
        private string ConvertToXaml(XmlNode rootNode, XmlNode uiNode)
        {
            StringBuilder xaml = new StringBuilder();
            
            string title = GetAttribute(rootNode, "Name", "Nimbus App");
            string width = GetAttribute(rootNode, "Width", "800");
            string height = GetAttribute(rootNode, "Height", "600");
            string theme = GetAttribute(rootNode, "Theme", "Dark").ToLower();
            
            string bgColor = theme == "dark" ? "#1E1E1E" : "#F5F5F5";
            string fgColor = theme == "dark" ? "#FFFFFF" : "#000000";
            
            xaml.AppendLine("<Window");
            xaml.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xaml.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            xaml.AppendLine("    Title=\"" + EscapeXml(title) + "\"");
            xaml.AppendLine("    Width=\"" + width + "\"");
            xaml.AppendLine("    Height=\"" + height + "\"");
            xaml.AppendLine("    Background=\"" + bgColor + "\"");
            xaml.AppendLine("    Foreground=\"" + fgColor + "\"");
            xaml.AppendLine("    WindowStartupLocation=\"CenterScreen\">");
            
            // Convert UI content
            foreach (XmlNode child in uiNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    ConvertElement(child, xaml, 1);
                    break; // Only first root element
                }
            }
            
            xaml.AppendLine("</Window>");
            
            return xaml.ToString();
        }
        



private int _autoNameIndex = 0;

// ConvertElement metodining boshida (attributes ishlanishidan OLDIN):

private void ConvertElement(XmlNode node, StringBuilder xaml, int indent)
{
    if (node == null || node.NodeType != XmlNodeType.Element) return;
    if (node.Name.StartsWith("#")) return;

    string indentStr = new string(' ', indent * 4);
    string originalName = node.Name;

    // Custom component check
    if (IsCustomComponent(originalName))
    {
        ConvertCustomComponent(node, xaml, indent);
        return;
    }

    string elementName = ConvertElementName(originalName);

    // Skip logic elements
    if (elementName == "Logic" || elementName == "Handler" || elementName == "Styles" ||
        elementName == "Style" || elementName == "Var" || elementName == "Variable" ||
        elementName == "Shortcuts" || elementName == "KeyBinding" ||
        elementName == "Bindings" || elementName == "Bind" || elementName == "Timer" ||
        elementName == "Include" || elementName == "ManualC" || elementName == "CSharp" ||
        elementName == "Code" || elementName == "Plugin" || elementName == "UsePlugin" ||
        elementName == "Components" || elementName == "Colors" || elementName == "Resources")
        return;

    // Grid definitions
    if (elementName == "Grid.RowDefinitions" || elementName == "Grid.ColumnDefinitions")
    {
        ConvertGridDefinitions(node, xaml, indentStr, elementName);
        return;
    }

    // Property elements
    if (node.Name.Contains(".") && !node.Name.StartsWith("Grid."))
    {
        ConvertPropertyElement(node, xaml, indent);
        return;
    }

    // Button/TextBox
    if (elementName == "Button" || elementName == "CustomButton")
    {
        bool hasPropertyElements = false;
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element && child.Name.Contains("."))
            {
                hasPropertyElements = true;
                break;
            }
        }
        if (hasPropertyElements)
        {
            ConvertNativeElement(node, xaml, indent);
            return;
        }
        ConvertCustomButton(node, xaml, indentStr);
        return;
    }

    if (elementName == "TextBox" || elementName == "PasswordBox")
    {
        ConvertCustomTextBox(node, xaml, indentStr, elementName);
        return;
    }

    // ══════════════════════════════════════════════════════════════
    // AUTO-NAME: Agar onClick bor lekin Name yo'q bo'lsa
    // ══════════════════════════════════════════════════════════════
    string existingName = GetAttribute(node, "Name", "");
    if (string.IsNullOrEmpty(existingName)) existingName = GetAttribute(node, "x:Name", "");

    string onClick = GetAttribute(node, "onClick", "");
    string onRightClick = GetAttribute(node, "onRightClick", "");

    bool needsAutoName = string.IsNullOrEmpty(existingName) &&
                         (!string.IsNullOrEmpty(onClick) || !string.IsNullOrEmpty(onRightClick));

    string autoGeneratedName = "";
    if (needsAutoName)
    {
        _autoNameIndex++;
        autoGeneratedName = "_nimbus_" + elementName.ToLower() + "_" + _autoNameIndex;
    }

    // ══════════════════════════════════════════════════════════════


    xaml.Append(indentStr + "<" + elementName);

    // Auto-generated name qo'shish
    if (needsAutoName)
    {
        xaml.Append(" Name=\"" + autoGeneratedName + "\"");
    }

    // Attributes (except RowDefinitions/ColumnDefinitions)
    bool hasBackground = false;
    bool hasForeground = false;
    string rowDefs = null;
    string colDefs = null;
    if (node.Attributes != null)
    {
        foreach (XmlAttribute attr in node.Attributes)
        {
            string lowerName = attr.Name.ToLower();
            if (lowerName == "background" || lowerName == "bgcolor") hasBackground = true;
            if (lowerName == "foreground" || lowerName == "color") hasForeground = true;
            if (elementName == "Grid" && lowerName == "rowdefinitions")
            {
                rowDefs = attr.Value;
                continue;
            }
            if (elementName == "Grid" && lowerName == "columndefinitions")
            {
                colDefs = attr.Value;
                continue;
            }
            string xamlAttr = ConvertAttributeToXaml(elementName, attr.Name, attr.Value);
            if (!string.IsNullOrEmpty(xamlAttr)) xaml.Append(xamlAttr);
        }
    }

    // Default foreground for text elements
    if (elementName == "TextBlock" && !hasForeground)
    {
        xaml.Append(" Foreground=\"#FFFFFF\"");
    }
    if (elementName == "Label" && !hasForeground)
    {
        xaml.Append(" Foreground=\"#FFFFFF\"");
    }

    // Check children

    bool hasChildElements = false;
    bool hasContextMenu = false;
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType == XmlNodeType.Element && !child.Name.StartsWith("#") &&
            child.Name != "Shadow" && child.Name != "BlurEffect" &&
            child.Name != "Glow" && child.Name != "ContextMenu")
        {
            hasChildElements = true;
        }
        if (child.Name == "ContextMenu") hasContextMenu = true;
    }

    bool hasContentAttr = false;
    if (node.Attributes != null)
    {
        foreach (XmlAttribute attr in node.Attributes)
        {
            if (attr.Name.ToLower() == "content") { hasContentAttr = true; break; }
        }
    }

    if (hasChildElements || hasContextMenu || (elementName == "Grid" && (rowDefs != null || colDefs != null)))
    {
        xaml.AppendLine(">");

        // For Grid: add RowDefinitions/ColumnDefinitions as child elements
        if (elementName == "Grid")
        {
            if (rowDefs != null)
            {
                xaml.AppendLine(indentStr + "    <Grid.RowDefinitions>");
                foreach (var row in rowDefs.Split(','))
                {
                    string val = row.Trim();
                    if (val == "*")
                        xaml.AppendLine(indentStr + "        <RowDefinition Height=\"*\" />");
                    else if (val.ToLower() == "auto")
                        xaml.AppendLine(indentStr + "        <RowDefinition Height=\"Auto\" />");
                    else if (!string.IsNullOrEmpty(val))
                        xaml.AppendLine(indentStr + "        <RowDefinition Height=\"" + val + "\" />");
                }
                xaml.AppendLine(indentStr + "    </Grid.RowDefinitions>");
            }
            if (colDefs != null)
            {
                xaml.AppendLine(indentStr + "    <Grid.ColumnDefinitions>");
                foreach (var col in colDefs.Split(','))
                {
                    string val = col.Trim();
                    if (val == "*")
                        xaml.AppendLine(indentStr + "        <ColumnDefinition Width=\"*\" />");
                    else if (val.ToLower() == "auto")
                        xaml.AppendLine(indentStr + "        <ColumnDefinition Width=\"Auto\" />");
                    else if (!string.IsNullOrEmpty(val))
                        xaml.AppendLine(indentStr + "        <ColumnDefinition Width=\"" + val + "\" />");
                }
                xaml.AppendLine(indentStr + "    </Grid.ColumnDefinitions>");
            }
        }

        if (hasContextMenu)
        {
            XmlNode cmNode = node.SelectSingleNode("ContextMenu");
            ConvertContextMenu(cmNode, xaml, indent + 1);
        }

        // For Grid children: auto-assign Grid.Row/Column if not set
        int gridRow = 0;
        int gridCol = 0;
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element &&
                child.Name != "Shadow" && child.Name != "BlurEffect" &&
                child.Name != "Glow" && child.Name != "ContextMenu")
            {
                if (elementName == "Grid")
                {
                    // Only assign Grid.Row if not set
                    bool hasRow = false;
                    if (child.Attributes != null)
                    {
                        foreach (XmlAttribute attr in child.Attributes)
                        {
                            if (attr.Name.ToLower() == "grid.row" || attr.Name.ToLower() == "row")
                            {
                                hasRow = true;
                                break;
                            }
                        }
                    }
                    if (!hasRow)
                    {
                        // Add Grid.Row attribute
                        XmlAttribute rowAttr = node.OwnerDocument.CreateAttribute("Grid.Row");
                        rowAttr.Value = gridRow.ToString();
                        child.Attributes.Append(rowAttr);
                    }
                    gridRow++;
                }
                ConvertElement(child, xaml, indent + 1);
            }
        }
        xaml.AppendLine(indentStr + "</" + elementName + ">");
    }
    else if (!string.IsNullOrWhiteSpace(node.InnerText) && !node.Name.Contains(".") && !hasContentAttr)
    {
        xaml.AppendLine(">" + EscapeXml(node.InnerText.Trim()) + "</" + elementName + ">");
    }
    else
    {
        xaml.AppendLine(" />");
    }
}

// ═══════════════════════════════════════════════════════════
// CUSTOM COMPONENT CHECK
// ═══════════════════════════════════════════════════════════
// WpfUI.cs da IsCustomComponent metodini yangilang:

private bool IsCustomComponent(string name)
{
    switch (name)
    {
        case "GlassCard":
        case "GlassButton":
        case "StatCard":
        case "ActionCard":
        case "Avatar":
        case "Badge":
        case "StatusIndicator":
        case "IconButton":
        case "NavItem":
        case "SearchBar":
        case "Divider":
        case "Win11Button":
        case "iOSLiquidCard":
            return true;
        default:
            // COMMENT QILINDI - ComponentSystem yo'q
            // if (_engine.Components != null)
            // {
            //     return _engine.Components.HasComponent(name);
            // }
            return false;
    }
}

// ═══════════════════════════════════════════════════════════
// CUSTOM COMPONENT CONVERTER
// ═══════════════════════════════════════════════════════════
private string ResolveValue(string value)
{
    // if (_engine.Components != null)
    // {
    //     return _engine.Components.ResolveAllReferences(value);
    // }
    return value;
}
private void ConvertCustomComponent(XmlNode node, StringBuilder xaml, int indent)
{
    string componentName = node.Name;
    string indentStr = new string(' ', indent * 4);

    // Collect parameters
    Dictionary<string, string> p = new Dictionary<string, string>();
    if (node.Attributes != null)
    {
        foreach (XmlAttribute attr in node.Attributes)
        {
            // MUHIM: Har bir atribut qiymatini resolve qilish kerak!
            // {Color.BgDark} -> #05050A ga aylantiriladi
            
            p[attr.Name] = ResolveValue(attr.Value);
        }
    }

    switch (componentName)
    {
        case "GlassCard":
            GenerateGlassCard(xaml, p, node, indent);
            break;
        case "GlassButton":
            GenerateGlassButton(xaml, p, indent);
            break;
        case "StatCard":
            GenerateStatCard(xaml, p, indent);
            break;
        case "ActionCard":
            GenerateActionCard(xaml, p, indent);
            break;
        case "Avatar":
            GenerateAvatar(xaml, p, indent);
            break;
        case "Badge":
            GenerateBadge(xaml, p, indent);
            break;
        case "StatusIndicator":
            GenerateStatusIndicator(xaml, p, indent);
            break;
        case "IconButton":
            GenerateIconButton(xaml, p, indent);
            break;
        case "NavItem":
            GenerateNavItem(xaml, p, indent);
            break;
        case "SearchBar":
            GenerateSearchBar(xaml, p, indent);
            break;
        case "Divider":
            GenerateDivider(xaml, p, indent);
            break;
        case "Win11Button":
            GenerateWin11Button(xaml, p, indent);
            break;
        

        default:
            // Unknown component - render as error box
            xaml.AppendLine(indentStr + "<Border Background=\"#FF000030\" Padding=\"10\" CornerRadius=\"8\" Margin=\"4\">");
            xaml.AppendLine(indentStr + "    <TextBlock Text=\"Unknown: " + componentName + "\" Foreground=\"#FF6666\" FontSize=\"12\"/>");
            xaml.AppendLine(indentStr + "</Border>");
            break;
    }
}

// ═══════════════════════════════════════════════════════════
// HELPER METHOD
// ═══════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════
// WINDOWS 11 COLOR PALETTE (Barcha komponentlar uchun)
// ═══════════════════════════════════════════════════════════

// Default rang konstantalari (class boshida qo'shing):
private static class Win11Colors
{
    // Background
    public const string BgSolid = "#202020";
    public const string BgSubtle = "#FFFFFF05";
    public const string BgSecondary = "#FFFFFF08";
    public const string BgTertiary = "#FFFFFF0A";
    public const string BgHover = "#FFFFFF0F";
    public const string BgPressed = "#FFFFFF06";
    
    // Accent
    public const string AccentDefault = "#60CDFF";      // Windows 11 blue
    public const string AccentLight = "#99EBFF";
    public const string AccentDark = "#0078D4";
    
    // Text
    public const string TextPrimary = "#FFFFFF";
    public const string TextSecondary = "#FFFFFFB3";    // 70% white
    public const string TextTertiary = "#FFFFFF8A";     // 54% white  
    public const string TextDisabled = "#FFFFFF5C";     // 36% white
    
    // Border
    public const string BorderDefault = "#FFFFFF14";
    public const string BorderSubtle = "#FFFFFF0F";
    public const string BorderStrong = "#FFFFFF1E";
    
    // Surface
    public const string SurfaceCard = "#FFFFFF05";
    public const string SurfaceCardHover = "#FFFFFF08";
    
    // Status
    public const string Success = "#6CCB5F";
    public const string Warning = "#FCE100";
    public const string Error = "#FF99A4";
    public const string Info = "#60CDFF";
}

// ═══════════════════════════════════════════════════════════
// STAT CARD - Windows 11 ranglar bilan
// ═══════════════════════════════════════════════════════════
private void GenerateStatCard(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "📊");
    string value = GetParam(p, "Value", "0");
    string label = GetParam(p, "Label", "Stat");
    string accentColor = GetParam(p, "AccentColor", "#0078D4");
    string name = GetParam(p, "Name", "");
    string gridCol = GetParam(p, "Grid.Column", "");

    // Accent rang asosida background yaratish
    string iconBg = GetAccentBackground(accentColor);

    xaml.AppendLine(ind + "<Border" + 
        (!string.IsNullOrEmpty(gridCol) ? " Grid.Column=\"" + gridCol + "\"" : "") +
        " CornerRadius=\"8\" Background=\"#2D2D2D\"");
    xaml.AppendLine(ind + "        BorderBrush=\"#3D3D3D\" BorderThickness=\"1\" Padding=\"16\">");
    xaml.AppendLine(ind + "    <StackPanel>");
    
    xaml.AppendLine(ind + "        <Border Width=\"32\" Height=\"32\" CornerRadius=\"6\"");
    xaml.AppendLine(ind + "                Background=\"" + iconBg + "\" HorizontalAlignment=\"Left\">");
    xaml.AppendLine(ind + "            <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"16\"");
    xaml.AppendLine(ind + "                       HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    xaml.AppendLine(ind + "        </Border>");
    
    xaml.AppendLine(ind + "        <TextBlock" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " Text=\"" + EscapeXml(value) + "\" FontSize=\"22\"");
    xaml.AppendLine(ind + "                   FontWeight=\"SemiBold\" Foreground=\"#FFFFFF\" Margin=\"0,10,0,2\"/>");
    
    xaml.AppendLine(ind + "        <TextBlock Text=\"" + EscapeXml(label) + "\" FontSize=\"12\" Foreground=\"#AAAAAA\"/>");
    
    xaml.AppendLine(ind + "    </StackPanel>");
    xaml.AppendLine(ind + "</Border>");
}
/// <summary>
/// Accent rang uchun dark background yaratish
/// </summary>
private string GetAccentBackground(string accentColor)
{
    if (string.IsNullOrEmpty(accentColor)) return "#3D3D3D";
    
    // Avval solid rangga aylantirish
    accentColor = ConvertToSolidColor(accentColor);
    
    if (!accentColor.StartsWith("#") || accentColor.Length < 7) return "#3D3D3D";
    
    try
    {
        string hex = accentColor.Substring(1);
        if (hex.Length > 6) hex = hex.Substring(0, 6);
        
        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        
        // 25% yorqinlik bilan dark background
        int newR = (int)(r * 0.25 + 35);
        int newG = (int)(g * 0.25 + 35);
        int newB = (int)(b * 0.25 + 35);
        
        return string.Format("#{0:X2}{1:X2}{2:X2}", 
            Math.Min(255, Math.Max(0, newR)), 
            Math.Min(255, Math.Max(0, newG)), 
            Math.Min(255, Math.Max(0, newB)));
    }
    catch
    {
        return "#3D3D3D";
    }
}

// ═══════════════════════════════════════════════════════════
// ACTION CARD - Windows 11 ranglar bilan
// ═══════════════════════════════════════════════════════════
private void GenerateActionCard(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "");
    string title = GetParam(p, "Title", "");
    string subtitle = GetParam(p, "Subtitle", "");
    string accentColor = GetParam(p, "AccentColor", "#0078D4");
    string name = GetParam(p, "Name", "");

    string iconBg = GetAccentBackground(accentColor);

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " CornerRadius=\"8\" Background=\"#2D2D2D\"");
    xaml.AppendLine(ind + "        BorderBrush=\"#3D3D3D\" BorderThickness=\"1\" Padding=\"14\" Margin=\"0,0,0,6\" Cursor=\"Hand\">");
    xaml.AppendLine(ind + "    <Grid>");
    xaml.AppendLine(ind + "        <Grid.ColumnDefinitions>");
    xaml.AppendLine(ind + "            <ColumnDefinition Width=\"Auto\"/>");
    xaml.AppendLine(ind + "            <ColumnDefinition Width=\"*\"/>");
    xaml.AppendLine(ind + "            <ColumnDefinition Width=\"Auto\"/>");
    xaml.AppendLine(ind + "        </Grid.ColumnDefinitions>");
    
    if (!string.IsNullOrEmpty(icon))
    {
        xaml.AppendLine(ind + "        <Border Grid.Column=\"0\" Width=\"40\" Height=\"40\" CornerRadius=\"8\"");
        xaml.AppendLine(ind + "                Background=\"" + iconBg + "\" VerticalAlignment=\"Center\">");
        xaml.AppendLine(ind + "            <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"18\"");
        xaml.AppendLine(ind + "                       HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
        xaml.AppendLine(ind + "        </Border>");
    }
    
    xaml.AppendLine(ind + "        <StackPanel Grid.Column=\"1\" VerticalAlignment=\"Center\" Margin=\"12,0,0,0\">");
    xaml.AppendLine(ind + "            <TextBlock Text=\"" + EscapeXml(title) + "\" FontSize=\"14\" FontWeight=\"SemiBold\" Foreground=\"#FFFFFF\"/>");
    if (!string.IsNullOrEmpty(subtitle))
    {
        xaml.AppendLine(ind + "            <TextBlock Text=\"" + EscapeXml(subtitle) + "\" FontSize=\"12\" Foreground=\"#AAAAAA\" Margin=\"0,2,0,0\"/>");
    }
    xaml.AppendLine(ind + "        </StackPanel>");
    
    xaml.AppendLine(ind + "        <TextBlock Grid.Column=\"2\" Text=\"›\" FontSize=\"18\" Foreground=\"#808080\"");
    xaml.AppendLine(ind + "                   VerticalAlignment=\"Center\"/>");
    
    xaml.AppendLine(ind + "    </Grid>");
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// GLASS CARD - Windows 11 ranglar
// ═══════════════════════════════════════════════════════════
private void GenerateGlassCard(StringBuilder xaml, Dictionary<string, string> p, XmlNode node, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string bg = GetParam(p, "Background", "#2D2D2D");
    string cornerRadius = GetParam(p, "CornerRadius", "8");
    string borderColor = GetParam(p, "BorderColor", "#3D3D3D");
    string padding = GetParam(p, "Padding", "16");
    string margin = GetParam(p, "Margin", "0");
    string name = GetParam(p, "Name", "");

    bg = ConvertToSolidColor(bg);
    borderColor = ConvertToSolidColor(borderColor);

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " CornerRadius=\"" + cornerRadius + "\" Background=\"" + bg + "\"");
    xaml.AppendLine(ind + "        BorderBrush=\"" + borderColor + "\" BorderThickness=\"1\"");
    xaml.AppendLine(ind + "        Padding=\"" + padding + "\"" + 
        (!string.IsNullOrEmpty(margin) && margin != "0" ? " Margin=\"" + margin + "\"" : "") + ">");
    
    if (node != null)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                ConvertElement(child, xaml, indent + 1);
            }
        }
    }
    
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// GLASS BUTTON - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateGlassButton(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "");
    string text = GetParam(p, "Text", "");
    string accentColor = GetParam(p, "AccentColor", "#0078D4");
    string size = GetParam(p, "Size", "36");
    string cornerRadius = GetParam(p, "CornerRadius", "8");
    string name = GetParam(p, "Name", "");

    double sizeVal = 36;
    double.TryParse(size, out sizeVal);

    string bg = GetAccentBackground(accentColor);
    string borderCol = ConvertToSolidColor(accentColor);

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " Width=\"" + size + "\" Height=\"" + size + "\" CornerRadius=\"" + cornerRadius + "\"");
    xaml.AppendLine(ind + "        Background=\"" + bg + "\" BorderBrush=\"" + borderCol + "\"");
    xaml.AppendLine(ind + "        BorderThickness=\"1\" Cursor=\"Hand\">");
    
    if (!string.IsNullOrEmpty(icon))
    {
        xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"" + (sizeVal * 0.45).ToString("F0") + "\"");
        xaml.AppendLine(ind + "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    }
    else if (!string.IsNullOrEmpty(text))
    {
        xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(text) + "\" Foreground=\"#FFFFFF\"");
        xaml.AppendLine(ind + "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" FontSize=\"12\"/>");
    }
    
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// NAV ITEM - Windows 11 style  
// ═══════════════════════════════════════════════════════════
private void GenerateNavItem(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "🏠");
    string label = GetParam(p, "Label", "");
    string active = GetParam(p, "Active", "false").ToLower();
    string accentColor = GetParam(p, "AccentColor", "#0078D4");
    string name = GetParam(p, "Name", "");
    string gridCol = GetParam(p, "Grid.Column", "");

    bool isActive = active == "true";
    string bg = isActive ? GetAccentBackground(accentColor) : "#2D2D2D";
    string textColor = isActive ? "#FFFFFF" : "#AAAAAA";
    string borderCol = isActive ? ConvertToSolidColor(accentColor) : "#3D3D3D";

    xaml.AppendLine(ind + "<Border Margin=\"4,2\" CornerRadius=\"6\" Padding=\"0,8\"" + 
        (!string.IsNullOrEmpty(gridCol) ? " Grid.Column=\"" + gridCol + "\"" : "") +
        " Background=\"" + bg + "\" BorderBrush=\"" + borderCol + "\" BorderThickness=\"1\"" +
        (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") +
        " Cursor=\"Hand\">");
    
    xaml.AppendLine(ind + "    <StackPanel HorizontalAlignment=\"Center\">");
    xaml.AppendLine(ind + "        <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"18\" HorizontalAlignment=\"Center\"/>");
    
    if (!string.IsNullOrEmpty(label))
    {
        xaml.AppendLine(ind + "        <TextBlock Text=\"" + EscapeXml(label) + "\" FontSize=\"9\"");
        xaml.AppendLine(ind + "                   Foreground=\"" + textColor + "\" HorizontalAlignment=\"Center\" Margin=\"0,3,0,0\"/>");
    }
    
    xaml.AppendLine(ind + "    </StackPanel>");
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// ICON BUTTON - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateIconButton(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "⚙️");
    string size = GetParam(p, "Size", "36");
    string iconSize = GetParam(p, "IconSize", "16");
    string bg = GetParam(p, "Background", "#3D3D3D");
    string cornerRadius = GetParam(p, "CornerRadius", "6");
    string name = GetParam(p, "Name", "");

    bg = ConvertToSolidColor(bg);

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " Width=\"" + size + "\" Height=\"" + size + "\"");
    xaml.AppendLine(ind + "        CornerRadius=\"" + cornerRadius + "\" Background=\"" + bg + "\"");
    xaml.AppendLine(ind + "        BorderBrush=\"#4D4D4D\" BorderThickness=\"1\" Cursor=\"Hand\">");
    xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"" + iconSize + "\"");
    xaml.AppendLine(ind + "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// AVATAR - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateAvatar(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string icon = GetParam(p, "Icon", "👤");
    string size = GetParam(p, "Size", "36");
    string bg = GetParam(p, "Background", "#3D3D3D");
    string name = GetParam(p, "Name", "");

    double sizeVal = 36;
    double.TryParse(size, out sizeVal);

    bg = ConvertToSolidColor(bg);

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") + 
                    " Width=\"" + size + "\" Height=\"" + size + "\" CornerRadius=\"" + (sizeVal / 2).ToString("F0") + "\"");
    xaml.AppendLine(ind + "        Background=\"" + bg + "\" BorderBrush=\"#5D5D5D\" BorderThickness=\"1\">");
    xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"" + (sizeVal * 0.45).ToString("F0") + "\"");
    xaml.AppendLine(ind + "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// DIVIDER - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateDivider(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string color = GetParam(p, "Color", "#3D3D3D");
    string height = GetParam(p, "Height", "1");
    string margin = GetParam(p, "Margin", "0,8");

    color = ConvertToSolidColor(color);

    xaml.AppendLine(ind + "<Border Height=\"" + height + "\" Background=\"" + color + "\"");
    xaml.AppendLine(ind + "        Margin=\"" + margin + "\"/>");
}

// ═══════════════════════════════════════════════════════════
// STATUS INDICATOR - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateStatusIndicator(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string status = GetParam(p, "Status", "active").ToLower();
    string size = GetParam(p, "Size", "8");
    string margin = GetParam(p, "Margin", "0");

    string color;
    switch (status)
    {
        case "active": case "online": case "success": color = "#4CAF50"; break;
        case "warning": case "away": case "busy": color = "#FFC107"; break;
        case "error": case "offline": case "danger": color = "#F44336"; break;
        default: color = "#808080"; break;
    }

    double sizeVal = 8;
    double.TryParse(size, out sizeVal);

    xaml.AppendLine(ind + "<Border Width=\"" + size + "\" Height=\"" + size + "\"");
    xaml.AppendLine(ind + "        CornerRadius=\"" + (sizeVal / 2).ToString("F0") + "\" Background=\"" + color + "\"" +
        (!string.IsNullOrEmpty(margin) && margin != "0" ? " Margin=\"" + margin + "\"" : "") + 
        " VerticalAlignment=\"Center\"/>");
}

// ═══════════════════════════════════════════════════════════
// BADGE - Windows 11 style
// ═══════════════════════════════════════════════════════════
private void GenerateBadge(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string text = GetParam(p, "Text", "");
    string count = GetParam(p, "Count", "0");
    string bg = GetParam(p, "Background", "#F44336");
    string fg = GetParam(p, "Foreground", "#FFFFFF");
    string size = GetParam(p, "Size", "18");
    string margin = GetParam(p, "Margin", "0");

    string displayText = !string.IsNullOrEmpty(text) ? text : count;
    
    int sizeInt = 18;
    int.TryParse(size, out sizeInt);

    bg = ConvertToSolidColor(bg);

    xaml.AppendLine(ind + "<Border MinWidth=\"" + size + "\" Height=\"" + size + "\"");
    xaml.AppendLine(ind + "        CornerRadius=\"" + (sizeInt / 2) + "\" Background=\"" + bg + "\"");
    xaml.AppendLine(ind + "        Padding=\"4,0\"" + 
                    (!string.IsNullOrEmpty(margin) && margin != "0" ? " Margin=\"" + margin + "\"" : "") + ">");
    xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(displayText) + "\" FontSize=\"10\" FontWeight=\"SemiBold\"");
    xaml.AppendLine(ind + "               Foreground=\"" + fg + "\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    xaml.AppendLine(ind + "</Border>");
}

// ═══════════════════════════════════════════════════════════
// STATUS INDICATOR
// ═══════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════
// ICON BUTTON
// ═══════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════
// NAV ITEM
// ═══════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════
// SEARCH BAR
// ═══════════════════════════════════════════════════════════
// WpfUI.cs ichida GenerateSearchBar metodini BUTUNLAY almashtiring:

private void GenerateSearchBar(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string placeholder = GetParam(p, "Placeholder", "Search...");
    string icon = GetParam(p, "Icon", "🔍");
    string showButton = GetParam(p, "ShowButton", "true");
    string buttonIcon = GetParam(p, "ButtonIcon", "➤");
    string accentColor = GetParam(p, "AccentColor", "#0078D4");
    string name = GetParam(p, "Name", "");
    string height = GetParam(p, "Height", "44");

    accentColor = ConvertToSolidColor(accentColor);

    xaml.AppendLine(ind + "<Grid Margin=\"0,4,0,8\">");
    
    xaml.AppendLine(ind + "    <Border Height=\"" + height + "\" CornerRadius=\"6\" Background=\"#2D2D2D\"");
    xaml.AppendLine(ind + "            BorderBrush=\"#4D4D4D\" BorderThickness=\"1\">");
    xaml.AppendLine(ind + "        <Grid>");
    xaml.AppendLine(ind + "            <Grid.ColumnDefinitions>");
    xaml.AppendLine(ind + "                <ColumnDefinition Width=\"40\"/>");
    xaml.AppendLine(ind + "                <ColumnDefinition Width=\"*\"/>");
    if (showButton.ToLower() == "true")
    {
        xaml.AppendLine(ind + "                <ColumnDefinition Width=\"Auto\"/>");
    }
    xaml.AppendLine(ind + "            </Grid.ColumnDefinitions>");
    
    xaml.AppendLine(ind + "            <TextBlock Grid.Column=\"0\" Text=\"" + EscapeXml(icon) + "\" FontSize=\"14\"");
    xaml.AppendLine(ind + "                       Foreground=\"#808080\"");
    xaml.AppendLine(ind + "                       HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    
    xaml.AppendLine(ind + "            <TextBox" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") +
                    " Grid.Column=\"1\"");
    xaml.AppendLine(ind + "                     Background=\"Transparent\" Foreground=\"#FFFFFF\"");
    xaml.AppendLine(ind + "                     BorderThickness=\"0\" VerticalAlignment=\"Center\"");
    xaml.AppendLine(ind + "                     VerticalContentAlignment=\"Center\" FontSize=\"14\"/>");
    
    xaml.AppendLine(ind + "            <TextBlock Grid.Column=\"1\" Text=\"" + EscapeXml(placeholder) + "\"");
    xaml.AppendLine(ind + "                       Foreground=\"#808080\" FontSize=\"14\"");
    xaml.AppendLine(ind + "                       VerticalAlignment=\"Center\"");
    xaml.AppendLine(ind + "                       IsHitTestVisible=\"False\"/>");
    
    if (showButton.ToLower() == "true")
    {
        xaml.AppendLine(ind + "            <Border Grid.Column=\"2\" Width=\"32\" Height=\"32\" CornerRadius=\"4\"");
        xaml.AppendLine(ind + "                    Background=\"" + accentColor + "\" Margin=\"0,0,6,0\" Cursor=\"Hand\">");
        xaml.AppendLine(ind + "                <TextBlock Text=\"" + EscapeXml(buttonIcon) + "\" FontSize=\"12\" Foreground=\"#FFFFFF\"");
        xaml.AppendLine(ind + "                           HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
        xaml.AppendLine(ind + "            </Border>");
    }
    
    xaml.AppendLine(ind + "        </Grid>");
    xaml.AppendLine(ind + "    </Border>");
    xaml.AppendLine(ind + "</Grid>");
}

// ═══════════════════════════════════════════════════════════
// DIVIDER
// ═══════════════════════════════════════════════════════════
private void GenerateWin11Button(StringBuilder xaml, Dictionary<string, string> p, int indent)
{
    string ind = new string(' ', indent * 4);
    
    string text = GetParam(p, "Text", "Button");
    string icon = GetParam(p, "Icon", "");
    string bg = GetParam(p, "Background", "#FFFFFF08");
    string fg = GetParam(p, "Foreground", "#FFFFFF");
    string cornerRadius = GetParam(p, "CornerRadius", "6");
    string padding = GetParam(p, "Padding", "14,8");
    string name = GetParam(p, "Name", "");

    xaml.AppendLine(ind + "<Border" + (!string.IsNullOrEmpty(name) ? " Name=\"" + name + "\"" : "") +
                    " CornerRadius=\"" + cornerRadius + "\" Background=\"" + bg + "\"");
    xaml.AppendLine(ind + "        BorderBrush=\"#FFFFFF0A\" BorderThickness=\"1\" Padding=\"" + padding + "\" Cursor=\"Hand\">");
    
    if (!string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(text))
    {
        xaml.AppendLine(ind + "    <StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Center\">");
        xaml.AppendLine(ind + "        <TextBlock Text=\"" + EscapeXml(icon) + "\" FontSize=\"14\" Margin=\"0,0,6,0\" VerticalAlignment=\"Center\"/>");
        xaml.AppendLine(ind + "        <TextBlock Text=\"" + EscapeXml(text) + "\" Foreground=\"" + fg + "\" FontSize=\"13\" VerticalAlignment=\"Center\"/>");
        xaml.AppendLine(ind + "    </StackPanel>");
    }
    else if (!string.IsNullOrEmpty(icon))
    {
        xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(icon) + "\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
    }
    else
    {
        xaml.AppendLine(ind + "    <TextBlock Text=\"" + EscapeXml(text) + "\" Foreground=\"" + fg + "\" FontSize=\"13\" HorizontalAlignment=\"Center\"/>");
    }
    
    xaml.AppendLine(ind + "</Border>");
}

private string GetParam(Dictionary<string, string> p, string key, string defaultValue)
{
    if (p.ContainsKey(key) && !string.IsNullOrEmpty(p[key]))
        return p[key];
    return defaultValue;
}
        
private void ConvertPropertyElement(XmlNode node, StringBuilder xaml, int indent)
{
    string indentStr = new string(' ', indent * 4);

    xaml.AppendLine(indentStr + "<" + node.Name + ">");

    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType == XmlNodeType.Element)
        {
            ConvertNativeXamlElement(child, xaml, indent + 1);
        }
    }

    xaml.AppendLine(indentStr + "</" + node.Name + ">");
}
private void ConvertNativeXamlElement(XmlNode node, StringBuilder xaml, int indent)
{
    if (node == null || node.NodeType != XmlNodeType.Element) return;

    string indentStr = new string(' ', indent * 4);
    string tagName = node.Name;

    xaml.Append(indentStr + "<" + tagName);

    if (node.Attributes != null)
    {
        foreach (XmlAttribute attr in node.Attributes)
        {
            xaml.Append(" " + attr.Name + "=\"" + EscapeXml(attr.Value) + "\"");
        }
    }

    bool hasChildren = false;
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType == XmlNodeType.Element)
        {
            hasChildren = true;
            break;
        }
    }

    if (hasChildren)
    {
        xaml.AppendLine(">");
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                ConvertNativeXamlElement(child, xaml, indent + 1);
            }
        }
        xaml.AppendLine(indentStr + "</" + tagName + ">");
    }
    else if (!string.IsNullOrWhiteSpace(node.InnerText))
    {
        xaml.AppendLine(">" + EscapeXml(node.InnerText.Trim()) + "</" + tagName + ">");
    }
    else
    {
        xaml.AppendLine(" />");
    }
}
private void ConvertNativeElement(XmlNode node, StringBuilder xaml, int indent)
{
    string indentStr = new string(' ', indent * 4);
    string elementName = ConvertElementName(node.Name);

    xaml.Append(indentStr + "<" + elementName);

    // Standard attributlarni qo'shish
    if (node.Attributes != null)
    {
        foreach (XmlAttribute attr in node.Attributes)
        {
            string lowerName = attr.Name.ToLower();

            // Skip custom Nimbus attributes
            if (lowerName == "onclick" || lowerName == "onrightclick" || lowerName == "hovercolor" ||
                lowerName == "pressedcolor" || lowerName == "cornerradius" ||
                lowerName == "shadow" || lowerName == "focusbordercolor" ||
                lowerName == "placeholder" || lowerName == "placeholdercolor" ||
                lowerName == "hoverbordercolor" || lowerName == "accentbottom" ||
                lowerName == "disabledbackground" || lowerName == "disabledforeground" ||
                lowerName == "danger" || lowerName == "multiline" ||
                lowerName == "selectionbrush" || lowerName == "caretbrush")
            {
                continue;
            }

            string xamlAttr = ConvertAttributeToXaml(elementName, attr.Name, attr.Value);
            if (!string.IsNullOrEmpty(xamlAttr)) xaml.Append(xamlAttr);
        }
    }

    // Children
    bool hasChildren = false;
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType == XmlNodeType.Element)
        {
            hasChildren = true;
            break;
        }
    }

    if (hasChildren)
    {
        xaml.AppendLine(">");

        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                // Property elements (Button.Background) - native pass
                if (child.Name.Contains("."))
                {
                    ConvertPropertyElement(child, xaml, indent + 1);
                }
                else
                {
                    ConvertElement(child, xaml, indent + 1);
                }
            }
        }

        xaml.AppendLine(indentStr + "</" + elementName + ">");
    }
    else if (!string.IsNullOrWhiteSpace(node.InnerText) && !node.Name.Contains("."))
    {
        xaml.AppendLine(">" + EscapeXml(node.InnerText.Trim()) + "</" + elementName + ">");
    }
    else
    {
        xaml.AppendLine(" />");
    }
}

// ---------------------------------------------------------
// CUSTOM COMPONENT GENERATORS
// ---------------------------------------------------------

// ConvertCustomButton metodini BUTUNLAY almashtiring:

private void ConvertCustomButton(XmlNode node, StringBuilder xaml, string indentStr)
{
    // Default ranglar — FAQAT 6 xonali HEX!
    string bg = GetAttribute(node, "Background", "");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string radius = GetAttribute(node, "CornerRadius", "6");
    string hover = GetAttribute(node, "HoverColor", "");
    string pressed = GetAttribute(node, "PressedColor", "");
    string border = GetAttribute(node, "BorderBrush", "");
    string thick = GetAttribute(node, "BorderThickness", "1");
    string padding = GetAttribute(node, "Padding", "16,8");
    string content = GetAttribute(node, "Content", node.InnerText.Trim());
    string name = GetAttribute(node, "Name", "");
    string width = GetAttribute(node, "Width", "Auto");
    string height = GetAttribute(node, "Height", "Auto");
    string margin = GetAttribute(node, "Margin", "0");
    string fontSize = GetAttribute(node, "FontSize", "14");
    string fontWeight = GetAttribute(node, "FontWeight", "Normal");
    string cursor = GetAttribute(node, "Cursor", "Hand");
    string shadow = GetAttribute(node, "Shadow", "false");
    string focusBorder = GetAttribute(node, "FocusBorderColor", "#0078D4");
    string disabledBg = GetAttribute(node, "DisabledBackground", "#2D2D2D");
    string disabledFg = GetAttribute(node, "DisabledForeground", "#6D6D6D");
    string fontFamily = GetAttribute(node, "FontFamily", "Segoe UI");
    string isEnabled = GetAttribute(node, "IsEnabled", "True");
    string opacity = GetAttribute(node, "Opacity", "1");

    // Agar rang berilmagan bo'lsa — Windows 11 default
    if (string.IsNullOrEmpty(bg)) bg = "#3D3D3D";
    if (string.IsNullOrEmpty(hover)) hover = "#4D4D4D";
    if (string.IsNullOrEmpty(pressed)) pressed = "#2D2D2D";
    if (string.IsNullOrEmpty(border)) border = "#5D5D5D";

    // 8 xonali ranglarni convert qilish
    bg = ConvertToSolidColor(bg);
    hover = ConvertToSolidColor(hover);
    pressed = ConvertToSolidColor(pressed);
    border = ConvertToSolidColor(border);
    fg = ConvertToSolidColor(fg);

    xaml.AppendLine(indentStr + "<Button");
    if (!string.IsNullOrEmpty(name)) xaml.AppendLine(indentStr + "    Name=\"" + name + "\"");
    xaml.AppendLine(indentStr + "    Width=\"" + width + "\" Height=\"" + height + "\"");
    xaml.AppendLine(indentStr + "    Margin=\"" + margin + "\"");
    xaml.AppendLine(indentStr + "    FontSize=\"" + fontSize + "\" FontWeight=\"" + fontWeight + "\"");
    xaml.AppendLine(indentStr + "    FontFamily=\"" + fontFamily + "\"");
    xaml.AppendLine(indentStr + "    Foreground=\"" + fg + "\"");
    xaml.AppendLine(indentStr + "    Cursor=\"" + cursor + "\"");
    if (opacity != "1") xaml.AppendLine(indentStr + "    Opacity=\"" + opacity + "\"");
    xaml.AppendLine(indentStr + "    IsEnabled=\"" + isEnabled + "\">");

    xaml.AppendLine(indentStr + "    <Button.Template>");
    xaml.AppendLine(indentStr + "        <ControlTemplate TargetType=\"Button\">");

    if (shadow.ToLower() == "true")
    {
        xaml.AppendLine(indentStr + "            <Border Padding=\"0,0,2,2\">");
        xaml.AppendLine(indentStr + "                <Border.Effect>");
        xaml.AppendLine(indentStr + "                    <DropShadowEffect Color=\"#000000\" BlurRadius=\"8\" ShadowDepth=\"2\" Opacity=\"0.3\" Direction=\"270\"/>");
        xaml.AppendLine(indentStr + "                </Border.Effect>");
    }

    xaml.AppendLine(indentStr + "            <Border x:Name=\"ButtonBorder\"");
    xaml.AppendLine(indentStr + "                    Background=\"" + bg + "\"");
    xaml.AppendLine(indentStr + "                    CornerRadius=\"" + radius + "\"");
    xaml.AppendLine(indentStr + "                    BorderBrush=\"" + border + "\"");
    xaml.AppendLine(indentStr + "                    BorderThickness=\"" + thick + "\"");
    xaml.AppendLine(indentStr + "                    SnapsToDevicePixels=\"True\">");
    xaml.AppendLine(indentStr + "                <ContentPresenter x:Name=\"ContentArea\"");
    xaml.AppendLine(indentStr + "                                  HorizontalAlignment=\"Center\"");
    xaml.AppendLine(indentStr + "                                  VerticalAlignment=\"Center\"");
    xaml.AppendLine(indentStr + "                                  Margin=\"" + padding + "\"/>");
    xaml.AppendLine(indentStr + "            </Border>");

    if (shadow.ToLower() == "true")
    {
        xaml.AppendLine(indentStr + "            </Border>");
    }

    xaml.AppendLine(indentStr + "            <ControlTemplate.Triggers>");

    xaml.AppendLine(indentStr + "                <Trigger Property=\"IsMouseOver\" Value=\"True\">");
    xaml.AppendLine(indentStr + "                    <Setter TargetName=\"ButtonBorder\" Property=\"Background\" Value=\"" + hover + "\"/>");
    xaml.AppendLine(indentStr + "                </Trigger>");

    xaml.AppendLine(indentStr + "                <Trigger Property=\"IsPressed\" Value=\"True\">");
    xaml.AppendLine(indentStr + "                    <Setter TargetName=\"ButtonBorder\" Property=\"Background\" Value=\"" + pressed + "\"/>");
    xaml.AppendLine(indentStr + "                </Trigger>");

    xaml.AppendLine(indentStr + "                <Trigger Property=\"IsFocused\" Value=\"True\">");
    xaml.AppendLine(indentStr + "                    <Setter TargetName=\"ButtonBorder\" Property=\"BorderBrush\" Value=\"" + focusBorder + "\"/>");
    xaml.AppendLine(indentStr + "                </Trigger>");

    xaml.AppendLine(indentStr + "                <Trigger Property=\"IsEnabled\" Value=\"False\">");
    xaml.AppendLine(indentStr + "                    <Setter TargetName=\"ButtonBorder\" Property=\"Background\" Value=\"" + disabledBg + "\"/>");
    xaml.AppendLine(indentStr + "                    <Setter TargetName=\"ButtonBorder\" Property=\"BorderBrush\" Value=\"#3D3D3D\"/>");
    xaml.AppendLine(indentStr + "                    <Setter Property=\"Foreground\" Value=\"" + disabledFg + "\"/>");
    xaml.AppendLine(indentStr + "                </Trigger>");

    xaml.AppendLine(indentStr + "            </ControlTemplate.Triggers>");
    xaml.AppendLine(indentStr + "        </ControlTemplate>");
    xaml.AppendLine(indentStr + "    </Button.Template>");

    if (!string.IsNullOrEmpty(content))
    {
        xaml.AppendLine(indentStr + "    " + EscapeXml(content));
    }

    xaml.AppendLine(indentStr + "</Button>");
}

/// <summary>
/// 8 xonali HEX ranglarni 6 xonali SOLID ranglarga aylantiradi
/// #AARRGGBB yoki #RRGGBBAA formatlarini qo'llab-quvvatlaydi
/// </summary>
private string ConvertToSolidColor(string color)
{
    if (string.IsNullOrEmpty(color)) return "#2D2D2D";
    
    color = color.Trim();
    
    // Transparent — dark gray ga aylantirish
    if (color.ToLower() == "transparent") return "Transparent";
    
    // Agar # bilan boshlanmasa
    if (!color.StartsWith("#")) 
    {
        // Named colors (White, Black, etc.) — qaytarish
        return color;
    }
    
    // Agar 4 xonali (#ARGB) — kengaytirish
    if (color.Length == 5)
    {
        // #ARGB -> #AARRGGBB
        char a = color[1];
        char r = color[2];
        char g = color[3];
        char b = color[4];
        color = "#" + a + a + r + r + g + g + b + b;
    }
    
    // Agar 3 xonali (#RGB) — kengaytirish
    if (color.Length == 4)
    {
        char r = color[1];
        char g = color[2];
        char b = color[3];
        return "#" + r + r + g + g + b + b;
    }
    
    // Agar 6 xonali (#RRGGBB) — qaytarish
    if (color.Length == 7)
    {
        return color;
    }
    
    // Agar 8 xonali (#AARRGGBB yoki #RRGGBBAA)
    if (color.Length == 9)
    {
        // Birinchi 2 ta raqamni tekshirish
        string first2 = color.Substring(1, 2).ToUpper();
        string last2 = color.Substring(7, 2).ToUpper();
        
        int firstVal = 0;
        int lastVal = 0;
        
        try { firstVal = Convert.ToInt32(first2, 16); } catch { }
        try { lastVal = Convert.ToInt32(last2, 16); } catch { }
        
        // Agar oxirgi 2 ta alpha bo'lsa (#RRGGBBAA format)
        // Odatda alpha 00-7F oralig'ida bo'lsa, bu AA
        // Masalan: #FFFFFF0F — bu #0FFFFFFF emas, #FFFFFF + 0F alpha
        
        string rgb;
        int alpha;
        
        // Heuristic: Agar oxirgi 2 ta kichik (00-4F) bo'lsa, bu alpha
        if (lastVal < 80)
        {
            // #RRGGBBAA format
            rgb = color.Substring(1, 6);
            alpha = lastVal;
        }
        else if (firstVal < 80)
        {
            // #AARRGGBB format
            rgb = color.Substring(3, 6);
            alpha = firstVal;
        }
        else
        {
            // Noaniq — faqat RGB ni olish
            rgb = color.Substring(1, 6);
            alpha = 255;
        }
        
        // Agar alpha juda past (< 30) — blend qilish
        if (alpha < 30)
        {
            return BlendWithBackground(rgb, alpha, "202020");
        }
        else if (alpha < 80)
        {
            return BlendWithBackground(rgb, alpha, "2D2D2D");
        }
        else if (alpha < 180)
        {
            return BlendWithBackground(rgb, alpha, "3D3D3D");
        }
        else
        {
            return "#" + rgb;
        }
    }
    
    return color;
}

/// <summary>
/// Foreground rangni background bilan blend qilib solid rang yaratish
/// </summary>
private string BlendWithBackground(string foregroundHex, int alpha, string backgroundHex)
{
    try
    {
        // Foreground RGB
        int fR = Convert.ToInt32(foregroundHex.Substring(0, 2), 16);
        int fG = Convert.ToInt32(foregroundHex.Substring(2, 2), 16);
        int fB = Convert.ToInt32(foregroundHex.Substring(4, 2), 16);
        
        // Background RGB
        int bR = Convert.ToInt32(backgroundHex.Substring(0, 2), 16);
        int bG = Convert.ToInt32(backgroundHex.Substring(2, 2), 16);
        int bB = Convert.ToInt32(backgroundHex.Substring(4, 2), 16);
        
        // Alpha 0-255 -> 0.0-1.0
        double a = alpha / 255.0;
        
        // Blend formula: result = foreground * alpha + background * (1 - alpha)
        int rR = (int)(fR * a + bR * (1.0 - a));
        int rG = (int)(fG * a + bG * (1.0 - a));
        int rB = (int)(fB * a + bB * (1.0 - a));
        
        // Clamp to 0-255
        rR = Math.Max(0, Math.Min(255, rR));
        rG = Math.Max(0, Math.Min(255, rG));
        rB = Math.Max(0, Math.Min(255, rB));
        
        return string.Format("#{0:X2}{1:X2}{2:X2}", rR, rG, rB);
    }
    catch
    {
        return "#3D3D3D";
    }
}

private void ConvertCustomTextBox(XmlNode node, StringBuilder xaml, string indentStr, string type)
{
    string bg = GetAttribute(node, "Background", "#2D2D2D");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string radius = GetAttribute(node, "CornerRadius", "6");
    string border = GetAttribute(node, "BorderBrush", "#5D5D5D");
    string thick = GetAttribute(node, "BorderThickness", "1");
    string padding = GetAttribute(node, "Padding", "10,8");
    string placeholder = GetAttribute(node, "Placeholder", "");
    string name = GetAttribute(node, "Name", "");
    string width = GetAttribute(node, "Width", "Auto");
    string height = GetAttribute(node, "Height", "Auto");
    string margin = GetAttribute(node, "Margin", "0");
    string text = GetAttribute(node, "Text", "");
    string fontSize = GetAttribute(node, "FontSize", "14");
    string fontFamily = GetAttribute(node, "FontFamily", "Segoe UI");
    string focusBorder = GetAttribute(node, "FocusBorderColor", "#0078D4");
    string hoverBorder = GetAttribute(node, "HoverBorderColor", "#7D7D7D");
    string selectionBrush = GetAttribute(node, "SelectionBrush", "#0078D4");
    string caretBrush = GetAttribute(node, "CaretBrush", "#FFFFFF");
    string multiline = GetAttribute(node, "Multiline", "false");
    string wrap = GetAttribute(node, "TextWrapping", "NoWrap");

    // 8 xonali ranglarni convert qilish
    bg = ConvertToSolidColor(bg);
    fg = ConvertToSolidColor(fg);
    border = ConvertToSolidColor(border);
    hoverBorder = ConvertToSolidColor(hoverBorder);

    if (multiline.ToLower() == "true") wrap = "Wrap";

    xaml.AppendLine(indentStr + "<Border Width=\"" + width + "\" Height=\"" + height + "\"");
    xaml.AppendLine(indentStr + "        Margin=\"" + margin + "\" Background=\"Transparent\">");

    xaml.AppendLine(indentStr + "    <" + type);
    if (!string.IsNullOrEmpty(name)) xaml.AppendLine(indentStr + "        Name=\"" + name + "\"");
    if (!string.IsNullOrEmpty(text) && type == "TextBox") xaml.AppendLine(indentStr + "        Text=\"" + EscapeXml(text) + "\"");
    xaml.AppendLine(indentStr + "        FontSize=\"" + fontSize + "\"");
    xaml.AppendLine(indentStr + "        FontFamily=\"" + fontFamily + "\"");
    xaml.AppendLine(indentStr + "        Foreground=\"" + fg + "\"");
    xaml.AppendLine(indentStr + "        Background=\"Transparent\"");
    xaml.AppendLine(indentStr + "        BorderThickness=\"0\"");
    xaml.AppendLine(indentStr + "        CaretBrush=\"" + caretBrush + "\"");
    xaml.AppendLine(indentStr + "        SelectionBrush=\"" + selectionBrush + "\"");
    xaml.AppendLine(indentStr + "        VerticalContentAlignment=\"Center\"");
    if (type == "TextBox")
    {
        xaml.AppendLine(indentStr + "        TextWrapping=\"" + wrap + "\"");
        if (multiline.ToLower() == "true")
        {
            xaml.AppendLine(indentStr + "        AcceptsReturn=\"True\"");
        }
    }
    xaml.AppendLine(indentStr + "        >");

    xaml.AppendLine(indentStr + "        <" + type + ".Style>");
    xaml.AppendLine(indentStr + "            <Style TargetType=\"{x:Type " + type + "}\">");
    xaml.AppendLine(indentStr + "                <Setter Property=\"Template\">");
    xaml.AppendLine(indentStr + "                    <Setter.Value>");
    xaml.AppendLine(indentStr + "                        <ControlTemplate TargetType=\"{x:Type " + type + "}\">");

    xaml.AppendLine(indentStr + "                            <Border x:Name=\"OuterBorder\"");
    xaml.AppendLine(indentStr + "                                    Background=\"" + bg + "\"");
    xaml.AppendLine(indentStr + "                                    CornerRadius=\"" + radius + "\"");
    xaml.AppendLine(indentStr + "                                    BorderBrush=\"" + border + "\"");
    xaml.AppendLine(indentStr + "                                    BorderThickness=\"" + thick + "\">");
    xaml.AppendLine(indentStr + "                                <Grid>");

    // Placeholder
    if (!string.IsNullOrEmpty(placeholder) && type == "TextBox")
    {
        xaml.AppendLine(indentStr + "                                    <TextBlock x:Name=\"PlaceholderText\"");
        xaml.AppendLine(indentStr + "                                               Text=\"" + EscapeXml(placeholder) + "\"");
        xaml.AppendLine(indentStr + "                                               Foreground=\"#808080\"");
        xaml.AppendLine(indentStr + "                                               Margin=\"" + padding + "\"");
        xaml.AppendLine(indentStr + "                                               VerticalAlignment=\"Center\"");
        xaml.AppendLine(indentStr + "                                               IsHitTestVisible=\"False\"");
        xaml.AppendLine(indentStr + "                                               Visibility=\"Collapsed\"/>");
    }

    xaml.AppendLine(indentStr + "                                    <ScrollViewer x:Name=\"PART_ContentHost\"");
    xaml.AppendLine(indentStr + "                                                  Margin=\"" + padding + "\"");
    xaml.AppendLine(indentStr + "                                                  VerticalAlignment=\"Center\"");
    xaml.AppendLine(indentStr + "                                                  Focusable=\"False\"/>");

    xaml.AppendLine(indentStr + "                                </Grid>");
    xaml.AppendLine(indentStr + "                            </Border>");

    // Triggers
    xaml.AppendLine(indentStr + "                            <ControlTemplate.Triggers>");

    if (!string.IsNullOrEmpty(placeholder) && type == "TextBox")
    {
        xaml.AppendLine(indentStr + "                                <Trigger Property=\"Text\" Value=\"\">");
        xaml.AppendLine(indentStr + "                                    <Setter TargetName=\"PlaceholderText\" Property=\"Visibility\" Value=\"Visible\"/>");
        xaml.AppendLine(indentStr + "                                </Trigger>");
    }

    xaml.AppendLine(indentStr + "                                <Trigger Property=\"IsMouseOver\" Value=\"True\">");
    xaml.AppendLine(indentStr + "                                    <Setter TargetName=\"OuterBorder\" Property=\"BorderBrush\" Value=\"" + hoverBorder + "\"/>");
    xaml.AppendLine(indentStr + "                                </Trigger>");

    xaml.AppendLine(indentStr + "                                <Trigger Property=\"IsKeyboardFocused\" Value=\"True\">");
    xaml.AppendLine(indentStr + "                                    <Setter TargetName=\"OuterBorder\" Property=\"BorderBrush\" Value=\"" + focusBorder + "\"/>");
    xaml.AppendLine(indentStr + "                                </Trigger>");

    xaml.AppendLine(indentStr + "                                <Trigger Property=\"IsEnabled\" Value=\"False\">");
    xaml.AppendLine(indentStr + "                                    <Setter TargetName=\"OuterBorder\" Property=\"Background\" Value=\"#252525\"/>");
    xaml.AppendLine(indentStr + "                                    <Setter TargetName=\"OuterBorder\" Property=\"BorderBrush\" Value=\"#3D3D3D\"/>");
    xaml.AppendLine(indentStr + "                                    <Setter Property=\"Foreground\" Value=\"#6D6D6D\"/>");
    xaml.AppendLine(indentStr + "                                </Trigger>");

    xaml.AppendLine(indentStr + "                            </ControlTemplate.Triggers>");
    xaml.AppendLine(indentStr + "                        </ControlTemplate>");
    xaml.AppendLine(indentStr + "                    </Setter.Value>");
    xaml.AppendLine(indentStr + "                </Setter>");
    xaml.AppendLine(indentStr + "            </Style>");
    xaml.AppendLine(indentStr + "        </" + type + ".Style>");
    xaml.AppendLine(indentStr + "    </" + type + ">");
    xaml.AppendLine(indentStr + "</Border>");
}

private void ConvertContextMenu(XmlNode node, StringBuilder xaml, int indent)
{
    if (node == null) return;
    string ind = new string(' ', indent * 4);

    string bgColor = GetAttribute(node, "Background", "#2C2C2C");
    string fgColor = GetAttribute(node, "Foreground", "#FFFFFF");
    string hoverBg = GetAttribute(node, "HoverBackground", "#FFFFFF14");
    string radius = GetAttribute(node, "CornerRadius", "8");
    string borderColor = GetAttribute(node, "BorderBrush", "#FFFFFF14");
    string shadowOpacity = GetAttribute(node, "ShadowOpacity", "0.32");
    string accentColor = GetAttribute(node, "AccentColor", "#60CDFF");
    string fontSize = GetAttribute(node, "FontSize", "14");
    string fontFamily = GetAttribute(node, "FontFamily", "Segoe UI");
    string menuPadding = GetAttribute(node, "MenuPadding", "4");
    string itemHeight = GetAttribute(node, "ItemHeight", "36");
    string iconSize = GetAttribute(node, "IconSize", "16");

    // Context Menu container
    xaml.AppendLine(ind + "<ContextMenu>");
    xaml.AppendLine(ind + "    <ContextMenu.Template>");
    xaml.AppendLine(ind + "        <ControlTemplate TargetType=\"ContextMenu\">");

    // Outer shadow border
    xaml.AppendLine(ind + "            <Border Padding=\"8\">");
    xaml.AppendLine(ind + "                <Border.Effect>");
    xaml.AppendLine(ind + "                    <DropShadowEffect Color=\"#000000\" BlurRadius=\"16\" ShadowDepth=\"4\" Opacity=\"" + shadowOpacity + "\" Direction=\"270\"/>");
    xaml.AppendLine(ind + "                </Border.Effect>");

    // Main background border
    xaml.AppendLine(ind + "                <Border Background=\"" + bgColor + "\"");
    xaml.AppendLine(ind + "                        CornerRadius=\"" + radius + "\"");
    xaml.AppendLine(ind + "                        BorderBrush=\"" + borderColor + "\"");
    xaml.AppendLine(ind + "                        BorderThickness=\"1\"");
    xaml.AppendLine(ind + "                        Padding=\"" + menuPadding + "\"");
    xaml.AppendLine(ind + "                        SnapsToDevicePixels=\"True\">");

    // Acrylic overlay effect
    xaml.AppendLine(ind + "                    <Border CornerRadius=\"" + radius + "\" Background=\"#0AFFFFFF\">");
    xaml.AppendLine(ind + "                        <StackPanel IsItemsHost=\"True\"/>");
    xaml.AppendLine(ind + "                    </Border>");

    xaml.AppendLine(ind + "                </Border>");
    xaml.AppendLine(ind + "            </Border>");
    xaml.AppendLine(ind + "        </ControlTemplate>");
    xaml.AppendLine(ind + "    </ContextMenu.Template>");

    // Menu Items
    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;

        if (child.Name == "MenuItem")
        {
            string header = GetAttribute(child, "Header", "");
            string icon = GetAttribute(child, "Icon", "");
            string shortcut = GetAttribute(child, "Shortcut", "");
            string onClick = GetAttribute(child, "onClick", "");
            string itemFg = GetAttribute(child, "Foreground", fgColor);
            string danger = GetAttribute(child, "Danger", "false");

            if (danger.ToLower() == "true") itemFg = "#FF4444";

            xaml.AppendLine(ind + "    <MenuItem Header=\"" + EscapeXml(header) + "\">");
            xaml.AppendLine(ind + "        <MenuItem.Template>");
            xaml.AppendLine(ind + "            <ControlTemplate TargetType=\"MenuItem\">");

            // Item border
            xaml.AppendLine(ind + "                <Border x:Name=\"ItemBorder\"");
            xaml.AppendLine(ind + "                        Height=\"" + itemHeight + "\"");
            xaml.AppendLine(ind + "                        Padding=\"12,0\"");
            xaml.AppendLine(ind + "                        Background=\"Transparent\"");
            xaml.AppendLine(ind + "                        CornerRadius=\"4\"");
            xaml.AppendLine(ind + "                        Margin=\"0,1\">");

            xaml.AppendLine(ind + "                    <Grid>");
            xaml.AppendLine(ind + "                        <Grid.ColumnDefinitions>");
            xaml.AppendLine(ind + "                            <ColumnDefinition Width=\"" + iconSize + "\"/>");
            xaml.AppendLine(ind + "                            <ColumnDefinition Width=\"12\"/>");
            xaml.AppendLine(ind + "                            <ColumnDefinition Width=\"*\"/>");
            xaml.AppendLine(ind + "                            <ColumnDefinition Width=\"Auto\"/>");
            xaml.AppendLine(ind + "                        </Grid.ColumnDefinitions>");

            // Icon column
            if (!string.IsNullOrEmpty(icon))
            {
                xaml.AppendLine(ind + "                        <TextBlock Grid.Column=\"0\" Text=\"" + EscapeXml(icon) + "\"");
                xaml.AppendLine(ind + "                                   FontFamily=\"Segoe MDL2 Assets\" FontSize=\"" + iconSize + "\"");
                xaml.AppendLine(ind + "                                   Foreground=\"" + itemFg + "\" VerticalAlignment=\"Center\"");
                xaml.AppendLine(ind + "                                   HorizontalAlignment=\"Center\"/>");
            }

            // Header text
            xaml.AppendLine(ind + "                        <TextBlock x:Name=\"HeaderText\" Grid.Column=\"2\"");
            xaml.AppendLine(ind + "                                   Text=\"{TemplateBinding Header}\"");
            xaml.AppendLine(ind + "                                   Foreground=\"" + itemFg + "\"");
            xaml.AppendLine(ind + "                                   FontSize=\"" + fontSize + "\"");
            xaml.AppendLine(ind + "                                   FontFamily=\"" + fontFamily + "\"");
            xaml.AppendLine(ind + "                                   VerticalAlignment=\"Center\"/>");

            // Shortcut text
            if (!string.IsNullOrEmpty(shortcut))
            {
                xaml.AppendLine(ind + "                        <TextBlock Grid.Column=\"3\"");
                xaml.AppendLine(ind + "                                   Text=\"" + EscapeXml(shortcut) + "\"");
                xaml.AppendLine(ind + "                                   Foreground=\"#FFFFFF5C\"");
                xaml.AppendLine(ind + "                                   FontSize=\"12\"");
                xaml.AppendLine(ind + "                                   FontFamily=\"" + fontFamily + "\"");
                xaml.AppendLine(ind + "                                   VerticalAlignment=\"Center\"");
                xaml.AppendLine(ind + "                                   Margin=\"24,0,0,0\"/>");
            }

            xaml.AppendLine(ind + "                    </Grid>");
            xaml.AppendLine(ind + "                </Border>");

            // Triggers
            xaml.AppendLine(ind + "                <ControlTemplate.Triggers>");

            // Hover
            xaml.AppendLine(ind + "                    <Trigger Property=\"IsHighlighted\" Value=\"True\">");
            xaml.AppendLine(ind + "                        <Setter TargetName=\"ItemBorder\" Property=\"Background\" Value=\"" + hoverBg + "\"/>");
            xaml.AppendLine(ind + "                    </Trigger>");

            // Pressed
            xaml.AppendLine(ind + "                    <Trigger Property=\"IsPressed\" Value=\"True\">");
            xaml.AppendLine(ind + "                        <Setter TargetName=\"ItemBorder\" Property=\"Background\" Value=\"#FFFFFF08\"/>");
            xaml.AppendLine(ind + "                        <Setter TargetName=\"HeaderText\" Property=\"Opacity\" Value=\"0.7\"/>");
            xaml.AppendLine(ind + "                    </Trigger>");

            // Disabled
            xaml.AppendLine(ind + "                    <Trigger Property=\"IsEnabled\" Value=\"False\">");
            xaml.AppendLine(ind + "                        <Setter TargetName=\"HeaderText\" Property=\"Foreground\" Value=\"#FFFFFF3D\"/>");
            xaml.AppendLine(ind + "                    </Trigger>");

            xaml.AppendLine(ind + "                </ControlTemplate.Triggers>");
            xaml.AppendLine(ind + "            </ControlTemplate>");
            xaml.AppendLine(ind + "        </MenuItem.Template>");
            xaml.AppendLine(ind + "    </MenuItem>");
        }
        else if (child.Name == "Separator")
        {
            xaml.AppendLine(ind + "    <Separator Margin=\"12,4\" Background=\"" + borderColor + "\" Height=\"1\"/>");
        }
    }

    xaml.AppendLine(ind + "</ContextMenu>");
}

// YANGI METOD: Button Template Generator
private void ConvertButtonWithTemplate(XmlNode node, StringBuilder xaml, string indentStr)
{
    string bg = GetAttribute(node, "Background", "#DDDDDD");
    string fg = GetAttribute(node, "Foreground", "#000000");
    string radius = GetAttribute(node, "CornerRadius", "0");
    string hoverColor = GetAttribute(node, "HoverColor", "");
    string pressedColor = GetAttribute(node, "PressedColor", "");
    string borderBrush = GetAttribute(node, "BorderBrush", "Transparent");
    string borderThick = GetAttribute(node, "BorderThickness", "0");
    string padding = GetAttribute(node, "Padding", "10,5");
    string content = GetAttribute(node, "Content", node.InnerText.Trim());
    string name = GetAttribute(node, "Name", "");
    string onClick = GetAttribute(node, "onClick", "");
    
    // Default hover/pressed logic if not specified
    if (string.IsNullOrEmpty(hoverColor)) 
    {
        // Simple darken logic could be here, but for now fallback to same or explicit
        hoverColor = bg; 
    }
    if (string.IsNullOrEmpty(pressedColor)) pressedColor = hoverColor;

    xaml.AppendLine(indentStr + "<Button");
    if (!string.IsNullOrEmpty(name)) xaml.AppendLine(indentStr + "    Name=\"" + name + "\"");
    
    // Add other attributes (Width, Height, Margin, etc)
    foreach (XmlAttribute attr in node.Attributes)
    {
        string n = attr.Name.ToLower();
        if (n != "background" && n != "foreground" && n != "cornerradius" && 
            n != "hovercolor" && n != "pressedcolor" && n != "borderbrush" && 
            n != "borderthickness" && n != "padding" && n != "content" && n != "name" && n != "onclick")
        {
            string xamlAttr = ConvertAttributeToXaml("Button", attr.Name, attr.Value);
            if (!string.IsNullOrEmpty(xamlAttr)) xaml.Append(indentStr + "    " + xamlAttr.Trim());
        }
    }
    xaml.AppendLine(">");

    xaml.AppendLine(indentStr + "    <Button.Template>");
    xaml.AppendLine(indentStr + "        <ControlTemplate TargetType=\"Button\">");
    xaml.AppendLine(indentStr + "            <Border x:Name=\"border\"");
    xaml.AppendLine(indentStr + "                    Background=\"" + bg + "\"");
    xaml.AppendLine(indentStr + "                    CornerRadius=\"" + radius + "\"");
    xaml.AppendLine(indentStr + "                    BorderBrush=\"" + borderBrush + "\"");
    xaml.AppendLine(indentStr + "                    BorderThickness=\"" + borderThick + "\">");
    xaml.AppendLine(indentStr + "                <ContentPresenter HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"" + padding + "\"/>");
    xaml.AppendLine(indentStr + "            </Border>");
    xaml.AppendLine(indentStr + "            <ControlTemplate.Triggers>");
    
    if (!string.IsNullOrEmpty(hoverColor))
    {
        xaml.AppendLine(indentStr + "                <Trigger Property=\"IsMouseOver\" Value=\"True\">");
        xaml.AppendLine(indentStr + "                    <Setter TargetName=\"border\" Property=\"Background\" Value=\"" + hoverColor + "\"/>");
        xaml.AppendLine(indentStr + "                    <Setter Property=\"Cursor\" Value=\"Hand\"/>");
        xaml.AppendLine(indentStr + "                </Trigger>");
    }
    
    if (!string.IsNullOrEmpty(pressedColor))
    {
        xaml.AppendLine(indentStr + "                <Trigger Property=\"IsPressed\" Value=\"True\">");
        xaml.AppendLine(indentStr + "                    <Setter TargetName=\"border\" Property=\"Background\" Value=\"" + pressedColor + "\"/>");
        xaml.AppendLine(indentStr + "                </Trigger>");
    }

    xaml.AppendLine(indentStr + "            </ControlTemplate.Triggers>");
    xaml.AppendLine(indentStr + "        </ControlTemplate>");
    xaml.AppendLine(indentStr + "    </Button.Template>");
    
    if (!string.IsNullOrEmpty(content))
    {
        xaml.AppendLine(indentStr + "    " + EscapeXml(content));
    }
    
    xaml.AppendLine(indentStr + "</Button>");
}
        
        private void ConvertGridDefinitions(XmlNode node, StringBuilder xaml, string indentStr, string elementName)
        {
            xaml.AppendLine(indentStr + "<" + elementName + ">");
            
            string content = node.InnerText.Trim();
            string[] parts = content.Split(',');
            
            if (elementName == "Grid.RowDefinitions")
            {
                foreach (string part in parts)
                {
                    string height = part.Trim();
                    if (!string.IsNullOrEmpty(height))
                    {
                        xaml.AppendLine(indentStr + "    <RowDefinition Height=\"" + ConvertGridLength(height) + "\" />");
                    }
                }
            }
            else
            {
                foreach (string part in parts)
                {
                    string width = part.Trim();
                    if (!string.IsNullOrEmpty(width))
                    {
                        xaml.AppendLine(indentStr + "    <ColumnDefinition Width=\"" + ConvertGridLength(width) + "\" />");
                    }
                }
            }
            
            xaml.AppendLine(indentStr + "</" + elementName + ">");
        }
        
        private string ConvertElementName(string name)
        {
            switch (name.ToLower())
            {
                // HTML elements
                case "div": return "StackPanel";
                case "span": return "TextBlock";
                case "p": return "TextBlock";
                case "h1": return "TextBlock";
                case "h2": return "TextBlock";
                case "h3": return "TextBlock";
                case "h4": return "TextBlock";
                case "h5": return "TextBlock";
                case "h6": return "TextBlock";
                case "a": return "TextBlock";
                case "label": return "Label";
                case "input": return "TextBox";
                case "textarea": return "TextBox";
                case "button": return "Button";
                case "btn": return "Button";
                case "select": return "ComboBox";
                case "option": return "ComboBoxItem";
                case "ul": return "StackPanel";
                case "ol": return "StackPanel";
                case "li": return "TextBlock";
                case "table": return "Grid";
                case "tr": return "StackPanel";
                case "td": return "TextBlock";
                case "th": return "TextBlock";
                case "img": return "Image";
                case "hr": return "Separator";
                case "br": return "Separator";
                case "form": return "StackPanel";
                case "nav": return "StackPanel";
                case "header": return "Border";
                case "footer": return "Border";
                case "section": return "StackPanel";
                case "article": return "StackPanel";
                case "aside": return "StackPanel";
                case "main": return "StackPanel";
                
                // Nimbus custom elements
                case "materialbutton": return "Button";
                case "card": return "Border";
                case "box": return "Border";
                case "container": return "Border";
                case "panel": return "StackPanel";
                case "row": return "StackPanel";
                case "column": return "StackPanel";
                case "col": return "StackPanel";
                case "view": return "ContentControl";
                case "list": return "ListBox";
                case "listitem": return "ListBoxItem";
                case "dropdown": return "ComboBox";
                case "combo": return "ComboBox";
                case "check": return "CheckBox";
                case "checkbox": return "CheckBox";
                case "radio": return "RadioButton";
                case "radiobutton": return "RadioButton";
                case "progress": return "ProgressBar";
                case "progressbar": return "ProgressBar";
                case "slider": return "Slider";
                case "range": return "Slider";
                case "scroll": return "ScrollViewer";
                case "scrollview": return "ScrollViewer";
                case "wrap": return "WrapPanel";
                case "wrappanel": return "WrapPanel";
                case "dock": return "DockPanel";
                case "dockpanel": return "DockPanel";
                case "canvas": return "Canvas";
                case "uniform": return "UniformGrid";
                case "uniformgrid": return "UniformGrid";
                case "expander": return "Expander";
                case "tab": return "TabControl";
                case "tabcontrol": return "TabControl";
                case "tabitem": return "TabItem";
                case "menu": return "Menu";
                case "menuitem": return "MenuItem";
                case "toolbar": return "ToolBar";
                case "statusbar": return "StatusBar";
                case "tooltip": return "ToolTip";
                case "popup": return "Popup";
                case "groupbox": return "GroupBox";
                case "group": return "GroupBox";
                case "separator": return "Separator";
                case "passwordbox": return "PasswordBox";
                case "password": return "PasswordBox";
                case "datepicker": return "DatePicker";
                case "date": return "DatePicker";
                case "calendar": return "Calendar";
                case "richtextbox": return "RichTextBox";
                case "richtext": return "RichTextBox";
                
                default: return name;
            }
        }
        
        private string ConvertAttributeToXaml(string elementName, string attrName, string attrValue)
{
    string lowerName = attrName.ToLower();
    string xamlName = attrName;
    string xamlValue = attrValue;

    // Skip event handlers and custom Nimbus attributes
    if (lowerName == "click" || lowerName == "onclick" ||
        lowerName == "mousedown" || lowerName == "onmousedown" || 
        lowerName == "mouseup" || lowerName == "onmouseup" ||
        lowerName == "leftclick" || lowerName == "onleftclick" ||
        lowerName == "rightclick" || lowerName == "onrightclick" ||
        lowerName == "textchanged" || lowerName == "ontextchanged" || lowerName == "onchange" ||
        lowerName == "onenter" || lowerName == "onvaluechanged" ||
        lowerName == "onmouseenter" || lowerName == "onmouseleave" ||
        lowerName == "onload" || lowerName == "onloaded" ||
        lowerName == "onkeydown" || lowerName == "onkeyup" ||
        lowerName == "onselectionchanged" ||
        lowerName == "placeholder" || lowerName == "hint" || lowerName == "watermark" ||
        lowerName == "style" || lowerName == "class" || lowerName == "css" ||
        lowerName == "type" || lowerName == "href" || lowerName == "src" ||
        lowerName == "alt" ||
        lowerName == "data" || lowerName == "value" || lowerName == "checked" ||
        lowerName == "selected" || lowerName == "disabled" || lowerName == "readonly" ||
        lowerName == "required" || lowerName == "autofocus" || lowerName == "tabindex" ||
        lowerName == "role" || lowerName == "aria-label" || lowerName == "for" ||
        lowerName == "action" || lowerName == "method" || lowerName == "enctype" ||
        lowerName == "target" || lowerName == "rel" || lowerName == "download" ||
        lowerName == "hovercolor" || lowerName == "pressedcolor" ||
        lowerName == "focusbordercolor" || lowerName == "placeholdercolor" ||
        lowerName == "hoverbordercolor" || lowerName == "accentbottom" ||
        lowerName == "caretbrush" || lowerName == "selectionbrush" ||
        lowerName == "multiline" || lowerName == "shadow" ||
        lowerName == "disabledbackground" || lowerName == "disabledforeground" ||
        lowerName == "danger" || lowerName == "glowcolor" || lowerName == "accentcolor" ||
        lowerName == "showglow" || lowerName == "blurradius")
    {
        return "";
    }

    // CornerRadius faqat Border uchun
    if (lowerName == "cornerradius" || lowerName == "radius")
    {
        if (elementName == "Border" || elementName == "border")
        {
            return " CornerRadius=\"" + EscapeXml(attrValue) + "\"";
        }
        return "";
    }

    switch (lowerName)
    {
        case "id":
        case "name":
        case "x:name":
            xamlName = "Name";
            break;

        case "text":
            // Only map Text to Content for Label and Button
            if (elementName == "Label" || elementName == "Button")
                xamlName = "Content";
            else
                xamlName = "Text";
            break;

        case "content":
            xamlName = "Content";
            break;
            
        // ═══════════════════════════════════════════════════════════
        // RANGLARNI CONVERT QILISH (8 xonali → 6 xonali)
        // ═══════════════════════════════════════════════════════════
        case "bgcolor":
        case "background":
            xamlName = "Background";
            xamlValue = ConvertToSolidColor(attrValue);
            break;
            
        case "color":
            if (elementName == "GradientStop")
                xamlName = "Color";
            else
                xamlName = "Foreground";
            xamlValue = ConvertToSolidColor(attrValue);
            break;
            
        case "fgcolor":
        case "foreground":
            xamlName = "Foreground";
            xamlValue = ConvertToSolidColor(attrValue);
            break;
            
        case "borderbrush":
        case "bordercolor":
            if (elementName == "TextBox" || elementName == "PasswordBox")
                return "";
            xamlName = "BorderBrush";
            xamlValue = ConvertToSolidColor(attrValue);
            break;
            
        case "fontsize":
            xamlName = "FontSize";
            break;
            
        case "fontweight":
            xamlName = "FontWeight";
            break;
            
        case "fontfamily":
            xamlName = "FontFamily";
            break;
            
        case "width":
            xamlName = "Width";
            break;
            
        case "height":
            xamlName = "Height";
            break;
            
        case "margin":
            xamlName = "Margin";
            break;
            
        case "padding":
            if (elementName == "TextBox" || elementName == "PasswordBox")
                return "";
            xamlName = "Padding";
            break;
            
        case "halign":
        case "horizontalalignment":
            xamlName = "HorizontalAlignment";
            xamlValue = ConvertAlignment(attrValue);
            break;
            
        case "valign":
        case "verticalalignment":
            xamlName = "VerticalAlignment";
            xamlValue = ConvertAlignment(attrValue);
            break;
            
        case "row":
        case "grid.row":
            xamlName = "Grid.Row";
            break;
            
        case "column":
        case "col":
        case "grid.column":
            xamlName = "Grid.Column";
            break;
            
        case "rowspan":
        case "grid.rowspan":
            xamlName = "Grid.RowSpan";
            break;
            
        case "colspan":
        case "columnspan":
        case "grid.columnspan":
            xamlName = "Grid.ColumnSpan";
            break;
            
        case "borderthickness":
            if (elementName == "TextBox" || elementName == "PasswordBox")
                return "";
            xamlName = "BorderThickness";
            break;
            
        case "orientation":
            xamlName = "Orientation";
            xamlValue = ConvertOrientation(attrValue);
            break;
            
        case "wrap":
        case "textwrapping":
            xamlName = "TextWrapping";
            xamlValue = attrValue.ToLower() == "true" || attrValue.ToLower() == "wrap" ? "Wrap" : "NoWrap";
            break;
            
        case "opacity":
            xamlName = "Opacity";
            break;
            
        case "visibility":
        case "visible":
            xamlName = "Visibility";
            if (attrValue.ToLower() == "false" || attrValue.ToLower() == "hidden" || attrValue.ToLower() == "collapsed")
                xamlValue = "Collapsed";
            else
                xamlValue = "Visible";
            break;
            
        case "cursor":
            xamlName = "Cursor";
            break;
            
        case "tooltip":
            xamlName = "ToolTip";
            break;
            
        case "textalignment":
            xamlName = "TextAlignment";
            break;
            
        case "isenabled":
            xamlName = "IsEnabled";
            break;
            
        default:
            xamlName = attrName;
            break;
    }

    return " " + xamlName + "=\"" + EscapeXml(xamlValue) + "\"";
}
        
        private string ConvertAlignment(string value)
        {
            switch (value.ToLower())
            {
                case "left": return "Left";
                case "right": return "Right";
                case "center": return "Center";
                case "stretch": return "Stretch";
                case "top": return "Top";
                case "bottom": return "Bottom";
                default: return value;
            }
        }
        
        private string ConvertOrientation(string value)
        {
            return (value.ToLower() == "horizontal" || value.ToLower() == "h") ? "Horizontal" : "Vertical";
        }
        
        private string ConvertGridLength(string value)
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value) || value == "*") return "*";
            if (value.ToLower() == "auto") return "Auto";
            return value;
        }
        
        private string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                       .Replace("\"", "&quot;").Replace("'", "&apos;");
        }
        
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
        
        private bool GetBoolAttribute(XmlNode node, string name, bool defaultValue)
        {
            string value = GetAttribute(node, name, "").ToLower();
            if (value == "true" || value == "1" || value == "yes")
                return true;
            if (value == "false" || value == "0" || value == "no")
                return false;
            return defaultValue;
        }
        
        private void RegisterNamedControls(Window window)
        {
            RegisterNamedControlsRecursive(window);
        }
        
        private void RegisterNamedControlsRecursive(DependencyObject parent)
        {
            if (parent == null) return;
            
            if (parent is FrameworkElement)
            {
                FrameworkElement fe = (FrameworkElement)parent;
                if (!string.IsNullOrEmpty(fe.Name))
                {
                    _engine.Controls[fe.Name] = fe;
                }
            }

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                RegisterNamedControlsRecursive(child);
            }
        }
        
        public void WireUpEventHandlers(Window window, XmlNode uiNode)
{
    // Kichik kutish - controllar to'liq register bo'lishi uchun
    window.Dispatcher.Invoke(new Action(delegate
    {
        WireUpEventHandlersRecursive(uiNode);
    }), System.Windows.Threading.DispatcherPriority.Loaded);
}
        private int _autoNameCounter = 0;

private void WireUpEventHandlersRecursive(XmlNode node)
{
    if (node == null) return;

    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;
        if (child.Name.StartsWith("#")) continue;
        if (IsLogicNode(child.Name)) continue;

        string name = GetAttribute(child, "Name", "");
        if (string.IsNullOrEmpty(name)) name = GetAttribute(child, "x:Name", "");

        string onClick = GetAttribute(child, "onClick", "");
        string onMouseDown = GetAttribute(child, "onMouseDown", ""); // <--- QO'SHILDI
        if (string.IsNullOrEmpty(onMouseDown)) onMouseDown = GetAttribute(child, "MouseDown", ""); // <--- QO'SHILDI
        
        string onRightClick = GetAttribute(child, "onRightClick", "");
        string onContextMenu = GetAttribute(child, "onContextMenu", "");
        string clickable = GetAttribute(child, "Clickable", "false");

        // Auto-generate name if onClick specified but no name
        if (!string.IsNullOrEmpty(onClick) && string.IsNullOrEmpty(name))
        {
            _autoNameCounter++;
            name = "_auto_" + child.Name.ToLower() + "_" + _autoNameCounter;
            // Control ni Name bilan register qilish kerak
            // Lekin control allaqachon yaratilgan, shuning uchun boshqacha yondashish kerak
        }

        FrameworkElement control = null;
        if (!string.IsNullOrEmpty(name))
        {
            control = _engine.GetControl(name);
        }

        // Agar name yo'q lekin onClick bor, visual tree dan topishga harakat qilish
        if (control == null && !string.IsNullOrEmpty(onClick))
        {
            // FindControlByAttributes orqali topish
            control = FindControlByAttributes(child);
        }

        if (control != null)
        {
            // LEFT CLICK
            if (!string.IsNullOrEmpty(onClick) || clickable.ToLower() == "true")
            {
                control.Cursor = System.Windows.Input.Cursors.Hand;

                if (!string.IsNullOrEmpty(onClick))
                {
                    string handlerName = onClick;

                    if (control is System.Windows.Controls.Primitives.ButtonBase)
                    {
                        ((System.Windows.Controls.Primitives.ButtonBase)control).Click += delegate
                        {
                            _engine.LogClick(name, control.GetType().Name);
                            _engine.ExecuteHandler(handlerName, control);
                        };
                    }
                    else
                    {
                        control.MouseLeftButtonUp += delegate
                        {
                            _engine.LogClick(name, control.GetType().Name);
                            _engine.ExecuteHandler(handlerName, control);
                        };
                    }
                }
            }

            // RIGHT CLICK
            if (!string.IsNullOrEmpty(onRightClick) || !string.IsNullOrEmpty(onContextMenu))
            {
                string rightHandler = !string.IsNullOrEmpty(onRightClick) ? onRightClick : onContextMenu;
                control.MouseRightButtonUp += delegate(object s, MouseButtonEventArgs e)
                {
                    _engine.ExecuteHandler(rightHandler, control);
                    e.Handled = true;
                };
            }

            // Hover Effects
            string hoverColor = GetAttribute(child, "HoverColor", "");
            if (!string.IsNullOrEmpty(hoverColor))
            {
                if (control is Control)
                {
                    Control ctrl = (Control)control;
                    Brush originalBrush = ctrl.Background;
                    Brush hoverBrush = null;
                    try { hoverBrush = (Brush)new BrushConverter().ConvertFromString(hoverColor); }
                    catch { }
                    if (hoverBrush != null)
                    {
                        ctrl.MouseEnter += delegate { ctrl.Background = hoverBrush; };
                        ctrl.MouseLeave += delegate { ctrl.Background = originalBrush; };
                    }
                }
                else if (control is Border)
                {
                    Border border = (Border)control;
                    Brush originalBrush = border.Background;
                    Brush hoverBrush = null;
                    try { hoverBrush = (Brush)new BrushConverter().ConvertFromString(hoverColor); }
                    catch { }
                    if (hoverBrush != null)
                    {
                        border.MouseEnter += delegate { border.Background = hoverBrush; };
                        border.MouseLeave += delegate { border.Background = originalBrush; };
                    }
                }
            }

            // Placeholder
            string placeholder = GetAttribute(child, "Placeholder", "");
            if (!string.IsNullOrEmpty(placeholder) && control is TextBox)
            {
                TextBox tb = (TextBox)control;
                bool hasText = !string.IsNullOrEmpty(tb.Text);

                if (!hasText)
                {
                    tb.Text = placeholder;
                    tb.Foreground = Brushes.Gray;
                }

                tb.GotFocus += delegate
                {
                    if (tb.Text == placeholder && tb.Foreground == Brushes.Gray)
                    {
                        tb.Text = "";
                        tb.Foreground = Brushes.White;
                    }
                };

                tb.LostFocus += delegate
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.Foreground = Brushes.Gray;
                    }
                };
            }

            // TextChanged
            string onTextChanged = GetAttribute(child, "onChange", "");
            if (string.IsNullOrEmpty(onTextChanged))
                onTextChanged = GetAttribute(child, "onTextChanged", "");
            if (!string.IsNullOrEmpty(onTextChanged) && control is TextBox)
            {
                string handler = onTextChanged;
                ((TextBox)control).TextChanged += delegate
                {
                    _engine.ExecuteHandler(handler, control);
                };
            }

            // ValueChanged
            string onValueChanged = GetAttribute(child, "onValueChanged", "");
            if (!string.IsNullOrEmpty(onValueChanged) && control is System.Windows.Controls.Primitives.RangeBase)
            {
                string handler = onValueChanged;
                ((System.Windows.Controls.Primitives.RangeBase)control).ValueChanged += delegate
                {
                    _engine.ExecuteHandler(handler, control);
                };
            }
        }

        WireUpEventHandlersRecursive(child);
    }
}

// YANGI helper metod
private FrameworkElement FindControlByAttributes(XmlNode node)
{
    // Bu metod Visual Tree ni scan qilib attributlarga mos control topadi
    // Hozircha null qaytaramiz, chunki XAML load bo'lganda name o'rnatilishi kerak
    // Yaxshiroq yechim: ConvertElement da auto-name qo'shish

    return null;
}

private bool IsLogicNode(string name)
{
    return name == "Logic" || name == "Handler" || name == "Var" || name == "Variable" ||
           name == "Styles" || name == "Style" || name == "Shortcuts" || name == "KeyBinding" ||
           name == "Bindings" || name == "Bind" || name == "Timer";
}
        public void WireUpShortcuts(Window window, XmlNode shortcutsNode)
        {
            if (shortcutsNode == null) return;

            window.PreviewKeyDown += delegate(object s, System.Windows.Input.KeyEventArgs e)
            {
                foreach (XmlNode child in shortcutsNode.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element) continue;
                    
                    if (child.Name == "KeyBinding")
                    {
                        string keyStr = GetAttribute(child, "Key", "");
                        string handler = GetAttribute(child, "Handler", "");
                        
                        try
                        {
                            System.Windows.Input.Key key = (System.Windows.Input.Key)Enum.Parse(typeof(System.Windows.Input.Key), keyStr);
                            
                            if (e.Key == key)
                            {
                                _engine.ExecuteHandler(handler, s);
                                e.Handled = true;
                            }
                        }
                        catch { }
                    }
                }
            };
        }
    }
    
    public class StyleDefinition
    {
        public string Name { get; set; }
        public string TargetType { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
