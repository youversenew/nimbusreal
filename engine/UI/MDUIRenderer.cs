using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Nimbus.MDUI;

namespace Nimbus.WPF
{
    /// <summary>
    /// MDui Renderer - Converts Nimbus XML to MDui components
    /// Supports Material Design UI rendering using GDI+
    /// </summary>
    public class MDUIRenderer
    {
        private MDEngine _engine;
        private Form _form;
        private Dictionary<string, MDElement> _components;
        private XmlParser _xmlParser;
        
        public MDUIRenderer(Form form)
        {
            _form = form;
            _components = new Dictionary<string, MDElement>();
            _engine = new MDEngine(form);
            _xmlParser = new XmlParser(null);
        }
        
        /// <summary>
        /// Parse MDui XML and create components
        /// </summary>
        public MDEngine CreateUI(XmlNode uiNode)
        {
            if (uiNode == null) return _engine;
            
            // Parse all UI elements recursively
            foreach (XmlNode child in uiNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    var element = ParseElement(child);
                    if (element != null)
                    {
                        _engine.AddElement(element);
                    }
                }
            }
            
            return _engine;
        }
        
        /// <summary>
        /// Parse single XML element and convert to MDui component
        /// </summary>
        private MDElement ParseElement(XmlNode node)
        {
            if (node == null) return null;
            
            string name = node.Name.ToLower();
            string id = GetAttribute(node, "Name", GetAttribute(node, "ID", ""));
            
            MDElement element = null;
            
            switch (name)
            {
                case "button":
                    element = ParseButton(node);
                    break;
                case "label":
                    element = ParseLabel(node);
                    break;
                case "textbox":
                case "textinput":
                    element = ParseTextBox(node);
                    break;
                case "checkbox":
                    element = ParseCheckBox(node);
                    break;
                case "slider":
                    element = ParseSlider(node);
                    break;
                case "progressbar":
                case "progress":
                    element = ParseProgressBar(node);
                    break;
                case "panel":
                case "card":
                    element = ParsePanel(node);
                    break;
                case "linearlayout":
                case "stackpanel":
                    element = ParseLinearLayout(node);
                    break;
                case "gridlayout":
                case "grid":
                    element = ParseGridLayout(node);
                    break;
                default:
                    return null;
            }
            
            if (element != null)
            {
                element.Name = id;
                if (!string.IsNullOrEmpty(id))
                {
                    _components[id] = element;
                }
                
                // Parse and add children
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        var childElement = ParseElement(child);
                        if (childElement != null)
                        {
                            element.AddChild(childElement);
                        }
                    }
                }
            }
            
            return element;
        }
        
        private MDButton ParseButton(XmlNode node)
        {
            var button = new MDButton
            {
                Text = GetAttribute(node, "Text", GetAttribute(node, "Content", "Button")),
                Bounds = ParseRectangle(node),
                ButtonType = ParseButtonType(GetAttribute(node, "Type", "contained")),
                TextColor = ParseColor(GetAttribute(node, "TextColor", ""), MDColors.TextPrimary),
                CornerRadius = GetIntAttribute(node, "CornerRadius", 4),
                Enabled = GetBoolAttribute(node, "Enabled", true),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            // Parse button-specific colors
            string containedColor = GetAttribute(node, "ContainedColor", "");
            if (!string.IsNullOrEmpty(containedColor))
                button.ContainedColor = ParseColor(containedColor, MDColors.Primary);
            
            string outlineColor = GetAttribute(node, "OutlineColor", "");
            if (!string.IsNullOrEmpty(outlineColor))
                button.OutlineColor = ParseColor(outlineColor, MDColors.Primary);
            
            return button;
        }
        
        private MDLabel ParseLabel(XmlNode node)
        {
            var label = new MDLabel
            {
                Text = GetAttribute(node, "Text", "Label"),
                Bounds = ParseRectangle(node),
                ForegroundColor = ParseColor(GetAttribute(node, "Foreground", ""), MDColors.TextPrimary),
                TextFont = ParseFont(node),
                AutoSize = GetBoolAttribute(node, "AutoSize", true),
                WordWrap = GetBoolAttribute(node, "WordWrap", true),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            return label;
        }
        
        private MDTextBox ParseTextBox(XmlNode node)
        {
            var textBox = new MDTextBox
            {
                Text = GetAttribute(node, "Text", ""),
                Placeholder = GetAttribute(node, "Placeholder", "Enter text..."),
                Bounds = ParseRectangle(node),
                IsPassword = GetBoolAttribute(node, "IsPassword", false),
                TextFont = ParseFont(node),
                Enabled = GetBoolAttribute(node, "Enabled", true),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string focusColor = GetAttribute(node, "FocusColor", "");
            if (!string.IsNullOrEmpty(focusColor))
                textBox.FocusBorderColor = ParseColor(focusColor, MDColors.Primary);
            
            return textBox;
        }
        
        private MDCheckBox ParseCheckBox(XmlNode node)
        {
            var checkbox = new MDCheckBox
            {
                Text = GetAttribute(node, "Text", "Checkbox"),
                Bounds = ParseRectangle(node),
                Checked = GetBoolAttribute(node, "Checked", false),
                TextFont = ParseFont(node),
                Enabled = GetBoolAttribute(node, "Enabled", true),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string checkColor = GetAttribute(node, "CheckColor", "");
            if (!string.IsNullOrEmpty(checkColor))
                checkbox.CheckColor = ParseColor(checkColor, MDColors.Primary);
            
            return checkbox;
        }
        
        private MDSlider ParseSlider(XmlNode node)
        {
            var slider = new MDSlider
            {
                Value = (float)GetDoubleAttribute(node, "Value", 50),
                MinValue = (float)GetDoubleAttribute(node, "MinValue", 0),
                MaxValue = (float)GetDoubleAttribute(node, "MaxValue", 100),
                Bounds = ParseRectangle(node),
                ShowLabel = GetBoolAttribute(node, "ShowLabel", true),
                Enabled = GetBoolAttribute(node, "Enabled", true),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string trackColor = GetAttribute(node, "TrackColor", "");
            if (!string.IsNullOrEmpty(trackColor))
                slider.TrackActiveColor = ParseColor(trackColor, MDColors.Primary);
            
            return slider;
        }
        
        private MDProgressBar ParseProgressBar(XmlNode node)
        {
            var progressBar = new MDProgressBar
            {
                Value = (float)GetDoubleAttribute(node, "Value", 0),
                MaxValue = (float)GetDoubleAttribute(node, "MaxValue", 100),
                Bounds = ParseRectangle(node),
                Indeterminate = GetBoolAttribute(node, "Indeterminate", false),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string progressColor = GetAttribute(node, "ProgressColor", "");
            if (!string.IsNullOrEmpty(progressColor))
                progressBar.ProgressColor = ParseColor(progressColor, MDColors.Primary);
            
            return progressBar;
        }
        
        private MDPanel ParsePanel(XmlNode node)
        {
            var panel = new MDPanel
            {
                Bounds = ParseRectangle(node),
                BackgroundColor = ParseColor(GetAttribute(node, "Background", ""), MDColors.Surface),
                BorderColor = ParseColor(GetAttribute(node, "BorderColor", ""), MDColors.SurfaceVariant),
                BorderWidth = GetIntAttribute(node, "BorderWidth", 1),
                CornerRadius = GetIntAttribute(node, "CornerRadius", 8),
                Elevation = ParseElevation(GetAttribute(node, "Elevation", "Level1")),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            return panel;
        }
        
        private MDLinearLayout ParseLinearLayout(XmlNode node)
        {
            string orientationStr = GetAttribute(node, "Orientation", "vertical").ToLower();
            var orientation = orientationStr.Contains("horizontal") ? 
                MDLinearLayout.Orientation.Horizontal : MDLinearLayout.Orientation.Vertical;
            
            var layout = new MDLinearLayout
            {
                Bounds = ParseRectangle(node),
                LayoutOrientation = orientation,
                Spacing = GetIntAttribute(node, "Spacing", 8),
                BackgroundColor = ParseColor(GetAttribute(node, "Background", ""), Color.Transparent),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string padding = GetAttribute(node, "Padding", "8");
            int padValue = int.Parse(padding);
            layout.Padding = new Padding(padValue);
            
            return layout;
        }
        
        private MDGridLayout ParseGridLayout(XmlNode node)
        {
            var grid = new MDGridLayout
            {
                Bounds = ParseRectangle(node),
                Columns = GetIntAttribute(node, "Columns", 1),
                Rows = GetIntAttribute(node, "Rows", 1),
                CellSpacing = GetIntAttribute(node, "CellSpacing", 8),
                BackgroundColor = ParseColor(GetAttribute(node, "Background", ""), Color.Transparent),
                Visible = GetBoolAttribute(node, "Visible", true)
            };
            
            string padding = GetAttribute(node, "Padding", "8");
            int padValue = int.Parse(padding);
            grid.Padding = new Padding(padValue);
            
            return grid;
        }
        
        #region Helper Methods
        
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node?.Attributes?[name] != null)
                return node.Attributes[name].Value;
            return defaultValue;
        }
        
        private int GetIntAttribute(XmlNode node, string name, int defaultValue)
        {
            string value = GetAttribute(node, name, defaultValue.ToString());
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }
        
        private double GetDoubleAttribute(XmlNode node, string name, double defaultValue)
        {
            string value = GetAttribute(node, name, defaultValue.ToString());
            if (double.TryParse(value, out double result))
                return result;
            return defaultValue;
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
        
        private Rectangle ParseRectangle(XmlNode node)
        {
            int x = GetIntAttribute(node, "X", 0);
            int y = GetIntAttribute(node, "Y", 0);
            int width = GetIntAttribute(node, "Width", 200);
            int height = GetIntAttribute(node, "Height", 48);
            
            return new Rectangle(x, y, width, height);
        }
        
        private Color ParseColor(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrEmpty(colorStr))
                return defaultColor;
            
            colorStr = colorStr.Trim();
            
            // Predefined colors
            switch (colorStr.ToLower())
            {
                case "primary": return MDColors.Primary;
                case "secondary": return MDColors.Secondary;
                case "tertiary": return MDColors.Tertiary;
                case "success": return MDColors.Success;
                case "error": return MDColors.Error;
                case "warning": return MDColors.Warning;
                case "surface": return MDColors.Surface;
                case "background": return MDColors.Background;
                case "white": return Color.White;
                case "black": return Color.Black;
            }
            
            // Hex color
            if (colorStr.StartsWith("#"))
            {
                try
                {
                    return ColorTranslator.FromHtml(colorStr);
                }
                catch { }
            }
            
            // RGB format: "255,100,50"
            if (colorStr.Contains(","))
            {
                try
                {
                    string[] parts = colorStr.Split(',');
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    return Color.FromArgb(r, g, b);
                }
                catch { }
            }
            
            return defaultColor;
        }
        
        private Font ParseFont(XmlNode node)
        {
            string fontFamily = GetAttribute(node, "FontFamily", "Segoe UI");
            int fontSize = GetIntAttribute(node, "FontSize", 14);
            bool isBold = GetAttribute(node, "FontWeight", "").ToLower().Contains("bold");
            
            FontStyle style = isBold ? FontStyle.Bold : FontStyle.Regular;
            return new Font(fontFamily, fontSize, style);
        }
        
        private MDButtonType ParseButtonType(string typeStr)
        {
            switch (typeStr.ToLower())
            {
                case "outlined": return MDButtonType.Outlined;
                case "text": return MDButtonType.Text;
                case "elevated": return MDButtonType.Elevated;
                case "tonal": return MDButtonType.Tonal;
                default: return MDButtonType.Contained;
            }
        }
        
        private ElevationLevel ParseElevation(string elevStr)
        {
            switch (elevStr.ToLower())
            {
                case "level1": return ElevationLevel.Level1;
                case "level2": return ElevationLevel.Level2;
                case "level3": return ElevationLevel.Level3;
                case "level4": return ElevationLevel.Level4;
                case "level5": return ElevationLevel.Level5;
                default: return ElevationLevel.Level0;
            }
        }
        
        #endregion
        
        /// <summary>
        /// Get component by name
        /// </summary>
        public MDElement GetComponent(string name)
        {
            if (_components.ContainsKey(name))
                return _components[name];
            return null;
        }
        
        /// <summary>
        /// Get MDEngine
        /// </summary>
        public MDEngine GetEngine()
        {
            return _engine;
        }
    }
}
