using System;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace Nimbus.WPF
{
    /// <summary>
    /// ModuleRenderer - Converts Nimbus XML to custom UIModule objects (no WPF)
    /// Used when inModule=true in App declaration
    /// </summary>
    public class ModuleRenderer
    {
        private WpfEngine _engine;
        private static readonly CultureInfo INV = CultureInfo.InvariantCulture;

        public ModuleRenderer(WpfEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Render XML to UIModule tree
        /// </summary>
        public IUIModule RenderUI(XmlNode rootNode, XmlNode uiNode)
        {
            if (uiNode == null) return null;

            IUIModule root = null;
            foreach (XmlNode child in uiNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    root = RenderNode(child);
                    break; // Only one root element
                }
            }

            return root;
        }

        /// <summary>
        /// Render single XML node to UIModule
        /// </summary>
        private IUIModule RenderNode(XmlNode node)
        {
            if (node == null || node.NodeType != XmlNodeType.Element)
                return null;

            string elementType = node.Name;
            string elementId = GetAttribute(node, "Name", elementType + "_" + Guid.NewGuid().ToString().Substring(0, 8));

            IUIModule element = CreateElement(elementType, elementId);
            if (element == null)
            {
                _engine.Log("MODULE", "Unknown element type: " + elementType);
                return null;
            }

            // Apply attributes
            ApplyAttributes(element, node);

            // Process children
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    IUIModule childElement = RenderNode(child);
                    if (childElement != null)
                    {
                        element.AddChild(childElement);
                    }
                }
            }

            _engine.Log("MODULE", "Rendered: " + elementType + " (id=" + elementId + ")");
            return element;
        }

        /// <summary>
        /// Create appropriate element based on type.
        /// Uses centralized UIModuleFactory which supports 50+ element types
        /// including all UILayout buttons, inputs, widgets, and layouts.
        /// </summary>
        private IUIModule CreateElement(string elementType, string elementId)
        {
            return UIModuleFactory.Create(elementType, elementId);
        }

        /// <summary>
        /// Apply XML attributes to element (comprehensive for modern UI)
        /// </summary>
        private void ApplyAttributes(IUIModule element, XmlNode node)
        {
            if (node.Attributes == null) return;
            ModuleUIElement modElement = element as ModuleUIElement;
            
            // Pre-declare all variables for C# 4.0 compatibility
            int intVal;
            double doubleVal;

            foreach (XmlAttribute attr in node.Attributes)
            {
                string attrName = attr.Name;
                string attrValue = ResolveValue(attr.Value);

                switch (attrName.ToLower())
                {
                    // ══════════════════════════ BASIC ══════════════════════════
                    case "name":
                        element.Id = attrValue;
                        break;
                    case "background":
                        if (modElement != null) modElement.Background = attrValue;
                        break;
                    case "foreground":
                        if (modElement != null) modElement.Foreground = attrValue;
                        break;
                    case "content":
                        if (modElement != null) modElement.Content = attrValue;
                        break;
                    
                    // ══════════════════════════ SIZING ══════════════════════════
                    case "width":
                        if (modElement != null) modElement.Width = attrValue;
                        break;
                    case "height":
                        if (modElement != null) modElement.Height = attrValue;
                        break;
                    case "minwidth":
                        if (modElement != null) modElement.MinWidth = attrValue;
                        break;
                    case "maxwidth":
                        if (modElement != null) modElement.MaxWidth = attrValue;
                        break;
                    case "minheight":
                        if (modElement != null) modElement.MinHeight = attrValue;
                        break;
                    case "maxheight":
                        if (modElement != null) modElement.MaxHeight = attrValue;
                        break;
                    
                    // ══════════════════════════ SPACING ══════════════════════════
                    case "margin":
                        if (modElement != null) modElement.Margin = attrValue;
                        break;
                    case "padding":
                        if (modElement != null) modElement.Padding = attrValue;
                        break;
                    
                    // ══════════════════════════ ALIGNMENT ══════════════════════════
                    case "horizontalalignment":
                        if (modElement != null) modElement.HorizontalAlignment = attrValue;
                        break;
                    case "verticalalignment":
                        if (modElement != null) modElement.VerticalAlignment = attrValue;
                        break;
                    
                    // ══════════════════════════ POSITIONING ══════════════════════════
                    case "position":
                        if (modElement != null) modElement.Position = attrValue;
                        break;
                    case "left":
                        if (modElement != null) modElement.Left = attrValue;
                        break;
                    case "top":
                        if (modElement != null) modElement.Top = attrValue;
                        break;
                    case "right":
                        if (modElement != null) modElement.Right = attrValue;
                        break;
                    case "bottom":
                        if (modElement != null) modElement.Bottom = attrValue;
                        break;
                    case "zindex":
                        if (modElement != null && int.TryParse(attrValue, out intVal))
                            modElement.ZIndex = intVal;
                        break;
                    
                    // ══════════════════════════ MODERN EFFECTS ══════════════════════════
                    case "cornerradius":
                        if (modElement != null && double.TryParse(attrValue, out doubleVal))
                            modElement.CornerRadius = doubleVal;
                        break;
                    case "shadow":
                        if (modElement != null) modElement.Shadow = attrValue;
                        break;
                    case "opacity":
                        if (modElement != null && double.TryParse(attrValue, out doubleVal))
                            modElement.Opacity = doubleVal;
                        break;
                    case "transition":
                        if (modElement != null) modElement.Transition = attrValue;
                        break;
                    case "gradient":
                        if (modElement != null) modElement.Gradient = attrValue;
                        break;
                    case "borderbrush":
                        if (modElement != null) modElement.BorderBrush = attrValue;
                        break;
                    case "borderthickness":
                        if (modElement != null && double.TryParse(attrValue, out doubleVal))
                            modElement.BorderThickness = doubleVal;
                        break;
                    
                    // ══════════════════════════ TYPOGRAPHY ══════════════════════════
                    case "fontfamily":
                        if (modElement != null) modElement.FontFamily = attrValue;
                        break;
                    case "fontsize":
                        if (modElement != null && double.TryParse(attrValue, out doubleVal))
                            modElement.FontSize = doubleVal;
                        break;
                    case "fontweight":
                        if (modElement != null) modElement.FontWeight = attrValue;
                        break;
                    case "fontstyle":
                        if (modElement != null) modElement.FontStyle = attrValue;
                        break;
                    case "textdecoration":
                        if (modElement != null) modElement.TextDecoration = attrValue;
                        break;
                    case "textalignment":
                        if (modElement != null) modElement.TextAlignment = attrValue;
                        break;
                    
                    // ══════════════════════════ INTERACTIONS ══════════════════════════
                    case "cursor":
                        if (modElement != null) modElement.Cursor = attrValue;
                        break;
                    case "icon":
                        if (modElement != null) modElement.Icon = attrValue;
                        break;
                    case "tooltip":
                        if (modElement != null) modElement.Tooltip = attrValue;
                        break;
                    case "isenabled":
                        if (modElement != null) modElement.IsEnabled = attrValue.ToLower() == "true";
                        break;
                    case "style":
                        if (modElement != null) modElement.Style = attrValue;
                        break;
                    case "theme":
                        if (modElement != null) modElement.Theme = attrValue;
                        break;
                    case "accentcolor":
                        if (modElement != null) modElement.AccentColor = attrValue;
                        break;
                    
                    // ══════════════════════════ ELEMENT-SPECIFIC (CORE) ══════════════════════════
                    case "text":
                        if (element is CustomUIButton)
                            ((CustomUIButton)element).Text = attrValue;
                        else if (element is CustomUILabel)
                            ((CustomUILabel)element).Text = attrValue;
                        else if (element is NimbusButton)
                            ((NimbusButton)element).Text = attrValue;
                        else if (element is NimbusChip)
                            ((NimbusChip)element).Text = attrValue;
                        else if (element is NimbusLinkButton)
                            ((NimbusLinkButton)element).Text = attrValue;
                        else if (element is NimbusDropdownButton)
                            ((NimbusDropdownButton)element).Text = attrValue;
                        else if (element is NimbusToggleButton)
                            ((NimbusToggleButton)element).Text = attrValue;
                        else if (element is NimbusRichText)
                            ((NimbusRichText)element).Text = attrValue;
                        break;
                    case "orientation":
                        if (element is CustomUIStackPanel)
                            ((CustomUIStackPanel)element).Orientation = attrValue;
                        else if (element is NimbusDivider)
                            ((NimbusDivider)element).Orientation = attrValue;
                        else if (element is NimbusRadioGroup)
                            ((NimbusRadioGroup)element).Orientation = attrValue;
                        break;
                    case "columndefinitions":
                        if (element is CustomUIGrid)
                            ((CustomUIGrid)element).ColumnDefinitions = attrValue;
                        break;
                    case "rowdefinitions":
                        if (element is CustomUIGrid)
                            ((CustomUIGrid)element).RowDefinitions = attrValue;
                        break;
                    case "direction":
                        if (element is CustomUIFlexPanel)
                            ((CustomUIFlexPanel)element).Direction = attrValue;
                        else if (element is NimbusWrapPanel)
                            ((NimbusWrapPanel)element).Direction = attrValue;
                        break;
                    case "gap":
                        if (element is CustomUIFlexPanel)
                            ((CustomUIFlexPanel)element).Gap = attrValue;
                        break;
                    case "justifycontent":
                        if (element is CustomUIFlexPanel)
                            ((CustomUIFlexPanel)element).JustifyContent = attrValue;
                        break;
                    case "alignitems":
                        if (element is CustomUIFlexPanel)
                            ((CustomUIFlexPanel)element).AlignItems = attrValue;
                        else if (element is NimbusGridLayout)
                            ((NimbusGridLayout)element).AlignItems = attrValue;
                        break;
                    case "wrap":
                        if (element is CustomUIFlexPanel)
                            ((CustomUIFlexPanel)element).Wrap = attrValue.ToLower() == "true";
                        break;
                    case "ischecked":
                        if (element is CustomUIToggle)
                            ((CustomUIToggle)element).IsChecked = attrValue.ToLower() == "true";
                        else if (element is NimbusCheckBox)
                            ((NimbusCheckBox)element).IsChecked = attrValue.ToLower() == "true";
                        break;
                    case "value":
                        if (element is CustomUISlider && double.TryParse(attrValue, out doubleVal))
                            ((CustomUISlider)element).Value = doubleVal;
                        else if (element is CustomUIInput)
                            ((CustomUIInput)element).Value = attrValue;
                        else if (element is NimbusTextInput)
                            ((NimbusTextInput)element).Value = attrValue;
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).Value = attrValue;
                        else if (element is NimbusSearchInput)
                            ((NimbusSearchInput)element).Value = attrValue;
                        else if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).Value = attrValue;
                        else if (element is NimbusNumberInput && double.TryParse(attrValue, out doubleVal))
                            ((NimbusNumberInput)element).Value = doubleVal;
                        else if (element is NimbusRadioButton)
                            ((NimbusRadioButton)element).Value = attrValue;
                        break;
                    case "minimum":
                        if (element is CustomUISlider && double.TryParse(attrValue, out doubleVal))
                            ((CustomUISlider)element).Minimum = doubleVal;
                        else if (element is NimbusNumberInput && double.TryParse(attrValue, out doubleVal))
                            ((NimbusNumberInput)element).Minimum = doubleVal;
                        else if (element is NimbusRangeSlider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusRangeSlider)element).Minimum = doubleVal;
                        break;
                    case "maximum":
                        if (element is CustomUISlider && double.TryParse(attrValue, out doubleVal))
                            ((CustomUISlider)element).Maximum = doubleVal;
                        else if (element is NimbusNumberInput && double.TryParse(attrValue, out doubleVal))
                            ((NimbusNumberInput)element).Maximum = doubleVal;
                        else if (element is NimbusRangeSlider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusRangeSlider)element).Maximum = doubleVal;
                        break;
                    case "progress":
                        if (element is CustomUIProgressBar && double.TryParse(attrValue, out doubleVal))
                            ((CustomUIProgressBar)element).Progress = doubleVal;
                        else if (element is NimbusCircularProgress && double.TryParse(attrValue, out doubleVal))
                            ((NimbusCircularProgress)element).Progress = doubleVal;
                        break;
                    case "placeholder":
                        if (element is CustomUIInput)
                            ((CustomUIInput)element).Placeholder = attrValue;
                        else if (element is NimbusTextInput)
                            ((NimbusTextInput)element).Placeholder = attrValue;
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).Placeholder = attrValue;
                        else if (element is NimbusSearchInput)
                            ((NimbusSearchInput)element).Placeholder = attrValue;
                        else if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).Placeholder = attrValue;
                        else if (element is NimbusNumberInput)
                            ((NimbusNumberInput)element).Placeholder = attrValue;
                        else if (element is NimbusDatePicker)
                            ((NimbusDatePicker)element).Placeholder = attrValue;
                        break;
                    case "inputtype":
                        if (element is CustomUIInput)
                            ((CustomUIInput)element).InputType = attrValue;
                        break;
                    case "badgestyle":
                        if (element is CustomUIBadge)
                            ((CustomUIBadge)element).BadgeStyle = attrValue;
                        break;

                    // ══════════════════════════ UILAYOUT BUTTON ATTRIBUTES ══════════════════════════
                    case "buttonstyle":
                        if (element is NimbusButton)
                        {
                            NimbusButtonStyle bStyle;
                            switch (attrValue.ToLower())
                            {
                                case "filled": bStyle = NimbusButtonStyle.Filled; break;
                                case "outlined": bStyle = NimbusButtonStyle.Outlined; break;
                                case "text": bStyle = NimbusButtonStyle.Text; break;
                                case "elevated": bStyle = NimbusButtonStyle.Elevated; break;
                                case "tonal": bStyle = NimbusButtonStyle.Tonal; break;
                                case "ghost": bStyle = NimbusButtonStyle.Ghost; break;
                                default: bStyle = NimbusButtonStyle.Filled; break;
                            }
                            ((NimbusButton)element).ButtonStyle = bStyle;
                        }
                        break;
                    case "buttonsize":
                        if (element is NimbusButton)
                        {
                            NimbusButtonSize bSize;
                            switch (attrValue.ToLower())
                            {
                                case "small": bSize = NimbusButtonSize.Small; break;
                                case "large": bSize = NimbusButtonSize.Large; break;
                                case "xlarge": bSize = NimbusButtonSize.XLarge; break;
                                default: bSize = NimbusButtonSize.Medium; break;
                            }
                            ((NimbusButton)element).ButtonSize = bSize;
                        }
                        break;
                    case "buttoncolor":
                        if (element is NimbusButton)
                            ((NimbusButton)element).ButtonColor = attrValue;
                        else if (element is NimbusIconButton)
                            ((NimbusIconButton)element).ButtonColor = attrValue;
                        else if (element is NimbusDropdownButton)
                            ((NimbusDropdownButton)element).ButtonColor = attrValue;
                        break;
                    case "textcolor":
                        if (element is NimbusButton)
                            ((NimbusButton)element).TextColor = attrValue;
                        break;
                    case "hovercolor":
                        if (element is NimbusButton)
                            ((NimbusButton)element).HoverColor = attrValue;
                        else if (element is NimbusIconButton)
                            ((NimbusIconButton)element).HoverColor = attrValue;
                        else if (element is NimbusLinkButton)
                            ((NimbusLinkButton)element).HoverColor = attrValue;
                        break;
                    case "iconleft":
                        if (element is NimbusButton)
                            ((NimbusButton)element).IconLeft = attrValue;
                        else if (element is NimbusTextInput)
                            ((NimbusTextInput)element).IconLeft = attrValue;
                        break;
                    case "iconright":
                        if (element is NimbusButton)
                            ((NimbusButton)element).IconRight = attrValue;
                        else if (element is NimbusTextInput)
                            ((NimbusTextInput)element).IconRight = attrValue;
                        break;
                    case "isloading":
                        if (element is NimbusButton)
                            ((NimbusButton)element).IsLoading = attrValue.ToLower() == "true";
                        break;
                    case "loadingtext":
                        if (element is NimbusButton)
                            ((NimbusButton)element).LoadingText = attrValue;
                        break;
                    case "rippleenabled":
                        if (element is NimbusButton)
                            ((NimbusButton)element).RippleEnabled = attrValue.ToLower() == "true";
                        break;
                    case "iscircular":
                        if (element is NimbusIconButton)
                            ((NimbusIconButton)element).IsCircular = attrValue.ToLower() == "true";
                        break;
                    case "iconsize":
                        if (element is NimbusIconButton && double.TryParse(attrValue, out doubleVal))
                            ((NimbusIconButton)element).IconSize = doubleVal;
                        break;
                    case "isextended":
                        if (element is NimbusFloatingActionButton)
                            ((NimbusFloatingActionButton)element).IsExtended = attrValue.ToLower() == "true";
                        break;
                    case "ismini":
                        if (element is NimbusFloatingActionButton)
                            ((NimbusFloatingActionButton)element).IsMini = attrValue.ToLower() == "true";
                        break;
                    case "fabcolor":
                        if (element is NimbusFloatingActionButton)
                            ((NimbusFloatingActionButton)element).FabColor = attrValue;
                        break;
                    case "istoggled":
                        if (element is NimbusToggleButton)
                            ((NimbusToggleButton)element).IsToggled = attrValue.ToLower() == "true";
                        else if (element is NimbusIconButton)
                            ((NimbusIconButton)element).IsToggled = attrValue.ToLower() == "true";
                        break;
                    case "activecolor":
                        if (element is NimbusToggleButton)
                            ((NimbusToggleButton)element).ActiveColor = attrValue;
                        else if (element is NimbusBottomNav)
                            ((NimbusBottomNav)element).ActiveColor = attrValue;
                        else if (element is NimbusStepper)
                            ((NimbusStepper)element).ActiveColor = attrValue;
                        else if (element is NimbusRadioButton)
                            ((NimbusRadioButton)element).ActiveColor = attrValue;
                        else if (element is NimbusSwitch)
                            ((NimbusSwitch)element).ActiveColor = attrValue;
                        break;
                    case "inactivecolor":
                        if (element is NimbusToggleButton)
                            ((NimbusToggleButton)element).InactiveColor = attrValue;
                        else if (element is NimbusBottomNav)
                            ((NimbusBottomNav)element).InactiveColor = attrValue;
                        else if (element is NimbusSwitch)
                            ((NimbusSwitch)element).InactiveColor = attrValue;
                        break;
                    case "showunderline":
                        if (element is NimbusLinkButton)
                            ((NimbusLinkButton)element).ShowUnderline = attrValue.ToLower() == "true";
                        break;
                    case "url":
                        if (element is NimbusLinkButton)
                            ((NimbusLinkButton)element).Url = attrValue;
                        break;

                    // ══════════════════════════ UILAYOUT INPUT ATTRIBUTES ══════════════════════════
                    case "label":
                        if (element is CustomUIToggle)
                            ((CustomUIToggle)element).Label = attrValue;
                        else if (element is NimbusTextInput)
                            ((NimbusTextInput)element).Label = attrValue;
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).Label = attrValue;
                        else if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).Label = attrValue;
                        else if (element is NimbusNumberInput)
                            ((NimbusNumberInput)element).Label = attrValue;
                        else if (element is NimbusComboBox)
                            ((NimbusComboBox)element).Label = attrValue;
                        else if (element is NimbusSwitch)
                            ((NimbusSwitch)element).Label = attrValue;
                        else if (element is NimbusCheckBox)
                            ((NimbusCheckBox)element).Label = attrValue;
                        else if (element is NimbusRadioButton)
                            ((NimbusRadioButton)element).Label = attrValue;
                        else if (element is NimbusDatePicker)
                            ((NimbusDatePicker)element).Label = attrValue;
                        else if (element is NimbusColorPicker)
                            ((NimbusColorPicker)element).Label = attrValue;
                        else if (element is NimbusRangeSlider)
                            ((NimbusRangeSlider)element).Label = attrValue;
                        break;
                    case "helpertext":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).HelperText = attrValue;
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).HelperText = attrValue;
                        break;
                    case "errortext":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).ErrorText = attrValue;
                        else if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).ErrorText = attrValue;
                        else if (element is NimbusComboBox)
                            ((NimbusComboBox)element).ErrorText = attrValue;
                        else if (element is NimbusDatePicker)
                            ((NimbusDatePicker)element).ErrorText = attrValue;
                        break;
                    case "prefix":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).Prefix = attrValue;
                        break;
                    case "suffix":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).Suffix = attrValue;
                        break;
                    case "maxlength":
                        if (element is NimbusTextInput && int.TryParse(attrValue, out intVal))
                            ((NimbusTextInput)element).MaxLength = intVal;
                        else if (element is NimbusTextArea && int.TryParse(attrValue, out intVal))
                            ((NimbusTextArea)element).MaxLength = intVal;
                        else if (element is NimbusPasswordInput && int.TryParse(attrValue, out intVal))
                            ((NimbusPasswordInput)element).MaxLength = intVal;
                        break;
                    case "minlength":
                        if (element is NimbusTextInput && int.TryParse(attrValue, out intVal))
                            ((NimbusTextInput)element).MinLength = intVal;
                        else if (element is NimbusPasswordInput && int.TryParse(attrValue, out intVal))
                            ((NimbusPasswordInput)element).MinLength = intVal;
                        break;
                    case "showcharcount":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).ShowCharCount = attrValue.ToLower() == "true";
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).ShowCharCount = attrValue.ToLower() == "true";
                        break;
                    case "isreadonly":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).IsReadOnly = attrValue.ToLower() == "true";
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).IsReadOnly = attrValue.ToLower() == "true";
                        break;
                    case "isclearable":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).IsClearable = attrValue.ToLower() == "true";
                        break;
                    case "floatinglabel":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).FloatingLabel = attrValue.ToLower() == "true";
                        break;
                    case "focusbordercolor":
                        if (element is NimbusTextInput)
                            ((NimbusTextInput)element).FocusBorderColor = attrValue;
                        else if (element is NimbusTextArea)
                            ((NimbusTextArea)element).FocusBorderColor = attrValue;
                        break;
                    case "rows":
                        if (element is NimbusTextArea && int.TryParse(attrValue, out intVal))
                            ((NimbusTextArea)element).Rows = intVal;
                        break;
                    case "maxrows":
                        if (element is NimbusTextArea && int.TryParse(attrValue, out intVal))
                            ((NimbusTextArea)element).MaxRows = intVal;
                        break;
                    case "autoresize":
                        if (element is NimbusTextArea)
                            ((NimbusTextArea)element).AutoResize = attrValue.ToLower() == "true";
                        break;
                    case "showsearchicon":
                        if (element is NimbusSearchInput)
                            ((NimbusSearchInput)element).ShowSearchIcon = attrValue.ToLower() == "true";
                        break;
                    case "showclearbutton":
                        if (element is NimbusSearchInput)
                            ((NimbusSearchInput)element).ShowClearButton = attrValue.ToLower() == "true";
                        break;
                    case "debouncems":
                        if (element is NimbusSearchInput && int.TryParse(attrValue, out intVal))
                            ((NimbusSearchInput)element).DebounceMs = intVal;
                        break;
                    case "ispasswordvisible":
                        if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).IsPasswordVisible = attrValue.ToLower() == "true";
                        break;
                    case "shouwtogglebutton":
                    case "showtogglebutton":
                        if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).ShowToggleButton = attrValue.ToLower() == "true";
                        break;
                    case "showstrengthindicator":
                        if (element is NimbusPasswordInput)
                            ((NimbusPasswordInput)element).ShowStrengthIndicator = attrValue.ToLower() == "true";
                        break;
                    case "step":
                        if (element is NimbusNumberInput && double.TryParse(attrValue, out doubleVal))
                            ((NimbusNumberInput)element).Step = doubleVal;
                        else if (element is CustomUISlider && double.TryParse(attrValue, out doubleVal))
                            ((CustomUISlider)element).Step = doubleVal;
                        else if (element is NimbusRangeSlider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusRangeSlider)element).Step = doubleVal;
                        break;
                    case "showstepper":
                        if (element is NimbusNumberInput)
                            ((NimbusNumberInput)element).ShowStepper = attrValue.ToLower() == "true";
                        break;
                    case "unit":
                        if (element is NimbusNumberInput)
                            ((NimbusNumberInput)element).Unit = attrValue;
                        break;
                    case "decimalplaces":
                        if (element is NimbusNumberInput && int.TryParse(attrValue, out intVal))
                            ((NimbusNumberInput)element).DecimalPlaces = intVal;
                        break;
                    case "issearchable":
                        if (element is NimbusComboBox)
                            ((NimbusComboBox)element).IsSearchable = attrValue.ToLower() == "true";
                        break;
                    case "allowmultiple":
                        if (element is NimbusComboBox)
                            ((NimbusComboBox)element).AllowMultiple = attrValue.ToLower() == "true";
                        else if (element is NimbusButtonGroup)
                            ((NimbusButtonGroup)element).AllowMultiple = attrValue.ToLower() == "true";
                        break;
                    case "ison":
                        if (element is NimbusSwitch)
                            ((NimbusSwitch)element).IsOn = attrValue.ToLower() == "true";
                        break;
                    case "onlabel":
                        if (element is NimbusSwitch)
                            ((NimbusSwitch)element).OnLabel = attrValue;
                        break;
                    case "offlabel":
                        if (element is NimbusSwitch)
                            ((NimbusSwitch)element).OffLabel = attrValue;
                        break;
                    case "thumbcolor":
                        if (element is NimbusSwitch)
                            ((NimbusSwitch)element).ThumbColor = attrValue;
                        else if (element is NimbusRangeSlider)
                            ((NimbusRangeSlider)element).ThumbColor = attrValue;
                        break;
                    case "labelposition":
                        if (element is NimbusSwitch)
                            ((NimbusSwitch)element).LabelPosition = attrValue;
                        break;
                    case "isindeterminate":
                        if (element is NimbusCheckBox)
                            ((NimbusCheckBox)element).IsIndeterminate = attrValue.ToLower() == "true";
                        else if (element is NimbusCircularProgress)
                            ((NimbusCircularProgress)element).IsIndeterminate = attrValue.ToLower() == "true";
                        break;
                    case "checkcolor":
                        if (element is NimbusCheckBox)
                            ((NimbusCheckBox)element).CheckColor = attrValue;
                        break;
                    case "checksize":
                        if (element is NimbusCheckBox && double.TryParse(attrValue, out doubleVal))
                            ((NimbusCheckBox)element).CheckSize = doubleVal;
                        break;
                    case "isselected":
                        if (element is NimbusRadioButton)
                            ((NimbusRadioButton)element).IsSelected = attrValue.ToLower() == "true";
                        break;
                    case "groupname":
                        if (element is NimbusRadioButton)
                            ((NimbusRadioButton)element).GroupName = attrValue;
                        else if (element is NimbusRadioGroup)
                            ((NimbusRadioGroup)element).GroupName = attrValue;
                        break;
                    case "selectedindex":
                        if (element is NimbusComboBox && int.TryParse(attrValue, out intVal))
                            ((NimbusComboBox)element).SelectedIndex = intVal;
                        else if (element is NimbusRadioGroup && int.TryParse(attrValue, out intVal))
                            ((NimbusRadioGroup)element).SelectedIndex = intVal;
                        else if (element is NimbusButtonGroup && int.TryParse(attrValue, out intVal))
                            ((NimbusButtonGroup)element).SelectedIndex = intVal;
                        else if (element is NimbusBottomNav && int.TryParse(attrValue, out intVal))
                            ((NimbusBottomNav)element).SelectedIndex = intVal;
                        else if (element is CustomUITabs && int.TryParse(attrValue, out intVal))
                            ((CustomUITabs)element).SelectedTabIndex = intVal;
                        break;
                    case "lowvalue":
                        if (element is NimbusRangeSlider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusRangeSlider)element).LowValue = doubleVal;
                        break;
                    case "highvalue":
                        if (element is NimbusRangeSlider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusRangeSlider)element).HighValue = doubleVal;
                        break;
                    case "selectedcolor":
                        if (element is NimbusColorPicker)
                            ((NimbusColorPicker)element).SelectedColor = attrValue;
                        break;
                    case "selecteddate":
                        if (element is NimbusDatePicker)
                            ((NimbusDatePicker)element).SelectedDate = attrValue;
                        break;
                    case "dateformat":
                        if (element is NimbusDatePicker)
                            ((NimbusDatePicker)element).DateFormat = attrValue;
                        break;

                    // ══════════════════════════ UILAYOUT WIDGET ATTRIBUTES ══════════════════════════
                    case "title":
                        if (element is CustomUIModal)
                            ((CustomUIModal)element).Title = attrValue;
                        else if (element is NimbusDialog)
                            ((NimbusDialog)element).Title = attrValue;
                        else if (element is NimbusAppBar)
                            ((NimbusAppBar)element).Title = attrValue;
                        else if (element is NimbusListTile)
                            ((NimbusListTile)element).Title = attrValue;
                        break;
                    case "subtitle":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).Subtitle = attrValue;
                        else if (element is NimbusAppBar)
                            ((NimbusAppBar)element).Subtitle = attrValue;
                        break;
                    case "message":
                        if (element is NimbusSnackbar)
                            ((NimbusSnackbar)element).Message = attrValue;
                        else if (element is NimbusDialog)
                            ((NimbusDialog)element).Message = attrValue;
                        break;
                    case "isvisible":
                        if (element is CustomUIModal)
                            ((CustomUIModal)element).IsVisible = attrValue.ToLower() == "true";
                        else if (element is NimbusSnackbar)
                            ((NimbusSnackbar)element).IsVisible = attrValue.ToLower() == "true";
                        else if (element is NimbusDialog)
                            ((NimbusDialog)element).IsVisible = attrValue.ToLower() == "true";
                        break;
                    case "isexpanded":
                        if (element is NimbusExpander)
                            ((NimbusExpander)element).IsExpanded = attrValue.ToLower() == "true";
                        break;
                    case "headertext":
                        if (element is NimbusExpander)
                            ((NimbusExpander)element).HeaderText = attrValue;
                        break;
                    case "initials":
                        if (element is NimbusAvatar)
                            ((NimbusAvatar)element).Initials = attrValue;
                        break;
                    case "avatarcolor":
                        if (element is NimbusAvatar)
                            ((NimbusAvatar)element).AvatarColor = attrValue;
                        break;
                    case "size":
                        if (element is NimbusAvatar && double.TryParse(attrValue, out doubleVal))
                            ((NimbusAvatar)element).Size = doubleVal;
                        else if (element is NimbusCircularProgress && double.TryParse(attrValue, out doubleVal))
                            ((NimbusCircularProgress)element).Size = doubleVal;
                        break;
                    case "shape":
                        if (element is NimbusAvatar)
                            ((NimbusAvatar)element).Shape = attrValue;
                        break;
                    case "statusdot":
                        if (element is NimbusAvatar)
                            ((NimbusAvatar)element).StatusDot = attrValue;
                        break;
                    case "imagesource":
                    case "source":
                    case "src":
                        if (element is NimbusAvatar)
                            ((NimbusAvatar)element).ImageSource = attrValue;
                        else if (element is NimbusImage)
                            ((NimbusImage)element).Source = attrValue;
                        break;
                    case "isdeletable":
                        if (element is NimbusChip)
                            ((NimbusChip)element).IsDeletable = attrValue.ToLower() == "true";
                        break;
                    case "chipstyle":
                        if (element is NimbusChip)
                            ((NimbusChip)element).ChipStyle = attrValue;
                        break;
                    case "chipcolor":
                        if (element is NimbusChip)
                            ((NimbusChip)element).ChipColor = attrValue;
                        break;
                    case "leadingicon":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).LeadingIcon = attrValue;
                        else if (element is NimbusAppBar)
                            ((NimbusAppBar)element).LeadingIcon = attrValue;
                        break;
                    case "trailingicon":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).TrailingIcon = attrValue;
                        break;
                    case "trailingtext":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).TrailingText = attrValue;
                        break;
                    case "showdivider":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).ShowDivider = attrValue.ToLower() == "true";
                        else if (element is NimbusExpander)
                            ((NimbusExpander)element).ShowDivider = attrValue.ToLower() == "true";
                        break;
                    case "dense":
                        if (element is NimbusListTile)
                            ((NimbusListTile)element).Dense = attrValue.ToLower() == "true";
                        break;
                    case "snackbartype":
                        if (element is NimbusSnackbar)
                            ((NimbusSnackbar)element).SnackbarType = attrValue;
                        break;
                    case "durationms":
                        if (element is NimbusSnackbar && int.TryParse(attrValue, out intVal))
                            ((NimbusSnackbar)element).DurationMs = intVal;
                        break;
                    case "actiontext":
                        if (element is NimbusSnackbar)
                            ((NimbusSnackbar)element).ActionText = attrValue;
                        break;
                    case "snackbarposition":
                        if (element is NimbusSnackbar)
                            ((NimbusSnackbar)element).SnackbarPosition = attrValue;
                        break;
                    case "centertitle":
                        if (element is NimbusAppBar)
                            ((NimbusAppBar)element).CenterTitle = attrValue.ToLower() == "true";
                        break;
                    case "appbarstyle":
                        if (element is NimbusAppBar)
                            ((NimbusAppBar)element).AppBarStyle = attrValue;
                        break;
                    case "showlabels":
                        if (element is NimbusBottomNav)
                            ((NimbusBottomNav)element).ShowLabels = attrValue.ToLower() == "true";
                        else if (element is NimbusStepper)
                            ((NimbusStepper)element).ShowLabels = attrValue.ToLower() == "true";
                        break;
                    case "dialogtype":
                        if (element is NimbusDialog)
                            ((NimbusDialog)element).DialogType = attrValue;
                        break;
                    case "primarybuttontext":
                        if (element is NimbusDialog)
                            ((NimbusDialog)element).PrimaryButtonText = attrValue;
                        break;
                    case "secondarybuttontext":
                        if (element is NimbusDialog)
                            ((NimbusDialog)element).SecondaryButtonText = attrValue;
                        break;
                    case "primarybuttoncolor":
                        if (element is NimbusDialog)
                            ((NimbusDialog)element).PrimaryButtonColor = attrValue;
                        break;
                    case "dismissonoverlayclick":
                        if (element is NimbusDialog)
                            ((NimbusDialog)element).DismissOnOverlayClick = attrValue.ToLower() == "true";
                        break;
                    case "progresscolor":
                        if (element is CustomUIProgressBar)
                            ((CustomUIProgressBar)element).ProgressColor = attrValue;
                        else if (element is NimbusCircularProgress)
                            ((NimbusCircularProgress)element).ProgressColor = attrValue;
                        break;
                    case "strokewidth":
                        if (element is NimbusCircularProgress && double.TryParse(attrValue, out doubleVal))
                            ((NimbusCircularProgress)element).StrokeWidth = doubleVal;
                        break;
                    case "fit":
                        if (element is NimbusImage)
                            ((NimbusImage)element).Fit = attrValue;
                        break;
                    case "alttext":
                        if (element is NimbusImage)
                            ((NimbusImage)element).AltText = attrValue;
                        break;
                    case "scrolldirection":
                        if (element is NimbusScrollView)
                            ((NimbusScrollView)element).ScrollDirection = attrValue;
                        break;
                    case "showscrollbar":
                        if (element is NimbusScrollView)
                            ((NimbusScrollView)element).ShowScrollbar = attrValue.ToLower() == "true";
                        break;
                    case "sortable":
                        if (element is NimbusDataTable)
                            ((NimbusDataTable)element).Sortable = attrValue.ToLower() == "true";
                        break;
                    case "stripedrows":
                        if (element is NimbusDataTable)
                            ((NimbusDataTable)element).StripedRows = attrValue.ToLower() == "true";
                        break;
                    case "hoverable":
                        if (element is NimbusDataTable)
                            ((NimbusDataTable)element).Hoverable = attrValue.ToLower() == "true";
                        break;
                    case "selectable":
                        if (element is NimbusDataTable)
                            ((NimbusDataTable)element).Selectable = attrValue.ToLower() == "true";
                        else if (element is NimbusRichText)
                            ((NimbusRichText)element).Selectable = attrValue.ToLower() == "true";
                        break;
                    case "skeletontype":
                        if (element is NimbusSkeleton)
                            ((NimbusSkeleton)element).SkeletonType = attrValue;
                        break;
                    case "isanimated":
                        if (element is NimbusSkeleton)
                            ((NimbusSkeleton)element).IsAnimated = attrValue.ToLower() == "true";
                        break;
                    case "currentstep":
                        if (element is NimbusStepper && int.TryParse(attrValue, out intVal))
                            ((NimbusStepper)element).CurrentStep = intVal;
                        break;
                    case "dividercolor":
                        if (element is NimbusDivider)
                            ((NimbusDivider)element).DividerColor = attrValue;
                        break;
                    case "dividerstyle":
                        if (element is NimbusDivider)
                            ((NimbusDivider)element).DividerStyle = attrValue;
                        break;
                    case "dividertext":
                        if (element is NimbusDivider)
                            ((NimbusDivider)element).DividerText = attrValue;
                        break;
                    case "thickness":
                        if (element is NimbusDivider && double.TryParse(attrValue, out doubleVal))
                            ((NimbusDivider)element).Thickness = doubleVal;
                        break;

                    // ══════════════════════════ UILAYOUT LAYOUT ATTRIBUTES ══════════════════════════
                    case "hgap":
                        if (element is NimbusWrapPanel && double.TryParse(attrValue, out doubleVal))
                            ((NimbusWrapPanel)element).HGap = doubleVal;
                        break;
                    case "vgap":
                        if (element is NimbusWrapPanel && double.TryParse(attrValue, out doubleVal))
                            ((NimbusWrapPanel)element).VGap = doubleVal;
                        break;
                    case "templatecolumns":
                        if (element is NimbusGridLayout)
                            ((NimbusGridLayout)element).TemplateColumns = attrValue;
                        break;
                    case "templaterows":
                        if (element is NimbusGridLayout)
                            ((NimbusGridLayout)element).TemplateRows = attrValue;
                        break;
                    case "colgap":
                        if (element is NimbusGridLayout && double.TryParse(attrValue, out doubleVal))
                            ((NimbusGridLayout)element).ColGap = doubleVal;
                        break;
                    case "rowgap":
                        if (element is NimbusGridLayout && double.TryParse(attrValue, out doubleVal))
                            ((NimbusGridLayout)element).RowGap = doubleVal;
                        break;
                    case "maxcontainerwidth":
                        if (element is NimbusContainer && double.TryParse(attrValue, out doubleVal))
                            ((NimbusContainer)element).MaxContainerWidth = doubleVal;
                        break;
                    case "centercontent":
                        if (element is NimbusContainer)
                            ((NimbusContainer)element).CenterContent = attrValue.ToLower() == "true";
                        break;
                    case "flexfactor":
                        if (element is NimbusSpacer && double.TryParse(attrValue, out doubleVal))
                            ((NimbusSpacer)element).FlexFactor = doubleVal;
                        break;
                    case "ratio":
                        if (element is NimbusAspectRatio && double.TryParse(attrValue, out doubleVal))
                            ((NimbusAspectRatio)element).Ratio = doubleVal;
                        break;

                    // ══════════════════════════ TYPOGRAPHY STYLE ══════════════════════════
                    case "textstyle":
                    case "typestyle":
                        if (modElement != null)
                        {
                            NimbusTextStyle tStyle = NimbusTypography.Resolve(attrValue);
                            if (tStyle != null)
                                NimbusTypography.Apply(modElement, tStyle);
                        }
                        break;
                    
                    default:
                        element.SetProperty(attrName, attrValue);
                        break;
                }
            }

            // Process inline text content
            if (node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text)
            {
                string text = node.InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    string resolvedText = ResolveValue(text);
                    if (element is CustomUILabel)
                        ((CustomUILabel)element).Text = resolvedText;
                    else if (element is CustomUIButton)
                        ((CustomUIButton)element).Text = resolvedText;
                    else if (element is NimbusButton)
                        ((NimbusButton)element).Text = resolvedText;
                    else if (element is CustomUIBadge)
                        ((CustomUIBadge)element).Content = resolvedText;
                    else if (element is NimbusChip)
                        ((NimbusChip)element).Text = resolvedText;
                    else if (element is NimbusLinkButton)
                        ((NimbusLinkButton)element).Text = resolvedText;
                    else if (element is NimbusRichText)
                        ((NimbusRichText)element).Text = resolvedText;
                    else if (element is NimbusSnackbar)
                        ((NimbusSnackbar)element).Message = resolvedText;
                    else if (element is NimbusExpander)
                        ((NimbusExpander)element).HeaderText = resolvedText;
                    else if (element is NimbusDivider)
                        ((NimbusDivider)element).DividerText = resolvedText;
                    else if (modElement != null)
                        modElement.Content = resolvedText;
                }
            }
        }

        /// <summary>
        /// Resolve {Color.Name} or {Variable} references
        /// </summary>
        private string ResolveValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Resolve color references {Color.ColorName}
            if (value.StartsWith("{Color.") && value.EndsWith("}"))
            {
                string colorName = value.Substring(7, value.Length - 8);
                if (_engine.ComponentSystem.Colors.ContainsKey(colorName))
                    return _engine.ComponentSystem.Colors[colorName];
            }

            // Resolve variable references {VariableName}
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                string varName = value.Substring(1, value.Length - 2);
                if (_engine.Variables.ContainsKey(varName))
                {
                    object val = _engine.Variables[varName];
                    return val != null ? val.ToString() : value;
                }
            }

            return value;
        }

        /// <summary>
        /// Get attribute value
        /// </summary>
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null)
                return defaultValue;

            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
    }
}
