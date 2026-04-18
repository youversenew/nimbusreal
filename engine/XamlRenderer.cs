using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    /// <summary>
    /// XAML Renderer - Converts Nimbus XML to standard XAML
    /// Fully integrated with ComponentSystem for resolving resources and styles
    /// </summary>
    public class XamlRenderer
    {
        private WpfEngine _engine;
        
        public XamlRenderer(WpfEngine engine)
        {
            _engine = engine;
        }
        
        /// <summary>
        /// Convert Nimbus XML to XAML
        /// </summary>
        public string ConvertToXaml(XmlDocument xmlDoc)
        {
            XmlNode root = xmlDoc.DocumentElement;
            StringBuilder xaml = new StringBuilder();
            
            // Resolve App level properties
            string title = ResolveValue(GetAttribute(root, "Name", "Nimbus App"));
            string width = ResolveValue(GetAttribute(root, "Width", "800"));
            string height = ResolveValue(GetAttribute(root, "Height", "600"));
            string bg = ResolveValue(GetAttribute(root, "Background", "#1E1E1E"));
            string fg = ResolveValue(GetAttribute(root, "Foreground", "#FFFFFF"));

            xaml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xaml.AppendLine("<Window");
            xaml.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xaml.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            
            xaml.AppendLine(string.Format("    Title=\"{0}\"", EscapeXml(title)));
            xaml.AppendLine(string.Format("    Width=\"{0}\"", width));
            xaml.AppendLine(string.Format("    Height=\"{0}\"", height));
            xaml.AppendLine(string.Format("    Background=\"{0}\"", bg));
            xaml.AppendLine(string.Format("    Foreground=\"{0}\"", fg));
            xaml.AppendLine("    WindowStartupLocation=\"CenterScreen\">");
            
            XmlNode uiNode = root.SelectSingleNode("UI");
            if (uiNode != null && uiNode.FirstChild != null)
            {
                // UI content conversion starts here
                // We use WpfUI's logic if possible, but here we do raw XML to XAML
                // for build/export purposes.
                foreach (XmlNode child in uiNode.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        ConvertNode(child, xaml, 1);
                        break; // Only one root element allowed in Window
                    }
                }
            }
            
            xaml.AppendLine("</Window>");
            
            return xaml.ToString();
        }
        
        /// <summary>
        /// Save XAML to file
        /// </summary>
        public void SaveXaml(string xamlContent, string outputPath)
        {
            File.WriteAllText(outputPath, xamlContent, Encoding.UTF8);
        }
        
        /// <summary>
        /// Convert XML node to XAML
        /// </summary>
        private void ConvertNode(XmlNode node, StringBuilder xaml, int indent)
        {
            ConvertNode(node, xaml, indent, false, 0);
        }

        private void ConvertNode(XmlNode node, StringBuilder xaml, int indent, bool isGridChild, int gridChildIndex)
        {
            if (node == null || node.NodeType != XmlNodeType.Element) return;
            if (node.Name.StartsWith("#")) return;

            string indentStr = new string(' ', indent * 4);
            string elementName = node.Name;
            string xamlTagName = ConvertElementName(elementName);
            
            xaml.Append(string.Format("{0}<{1}", indentStr, xamlTagName));
            
            // Add Grid.Row if this is a Grid child and element doesn't already have Row
            if (isGridChild)
            {
                bool hasRow = false;
                bool hasColumn = false;
                if (node.Attributes != null)
                {
                    XmlAttribute rowAttr = node.Attributes["Grid.Row"];
                    if (rowAttr == null) rowAttr = node.Attributes["Row"];
                    if (rowAttr != null) hasRow = true;
                    
                    XmlAttribute colAttr = node.Attributes["Grid.Column"];
                    if (colAttr == null) colAttr = node.Attributes["Column"];
                    if (colAttr != null) hasColumn = true;
                }
                if (!hasRow)
                {
                    xaml.Append(string.Format(" Grid.Row=\"{0}\"", gridChildIndex));
                }
            }
            
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    // Skip Grid Row/Column if we already added them
                    if ((attr.Name == "Grid.Row" || attr.Name == "Row") && isGridChild) continue;
                    if ((attr.Name == "Grid.Column" || attr.Name == "Column") && isGridChild) continue;
                    
                    string attrName = ConvertAttributeName(attr.Name);
                    if (!string.IsNullOrEmpty(attrName))
                    {
                        string attrValue = ResolveValue(attr.Value);
                        xaml.Append(string.Format(" {0}=\"{1}\"", attrName, EscapeXml(attrValue)));
                    }
                }
            }
            
            if (HasElementChildren(node))
            {
                xaml.AppendLine(">");
                
                int childIndex = 0;
                bool childrenAreGridItems = elementName.ToLower() == "grid";
                
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        ConvertNode(child, xaml, indent + 1, childrenAreGridItems, childIndex);
                        if (childrenAreGridItems) childIndex++;
                    }
                }
                
                xaml.AppendLine(string.Format("{0}</{1}>", indentStr, xamlTagName));
            }
            else if (!string.IsNullOrWhiteSpace(node.InnerText) && node.ChildNodes.Count == 1)
            {
                string textContent = ResolveValue(node.InnerText.Trim());
                xaml.AppendLine(string.Format(">{0}</{1}>", EscapeXml(textContent), xamlTagName));
            }
            else
            {
                xaml.AppendLine(" />");
            }
        }

        /// <summary>
        /// Resolve resource references using ComponentSystem
        /// </summary>
        private string ResolveValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Resolve color references {Color.ColorName}
            if (value.StartsWith("{Color.") && value.EndsWith("}"))
            {
                string colorName = value.Substring(7, value.Length - 8);
                if (_engine != null && _engine.ComponentSystem != null && 
                    _engine.ComponentSystem.Colors.ContainsKey(colorName))
                {
                    return _engine.ComponentSystem.Colors[colorName];
                }
                // If not found, return as-is (will fail gracefully in XAML)
                return value;
            }

            // Resolve variable references {VariableName}
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                string varName = value.Substring(1, value.Length - 2);
                if (_engine != null && _engine.Variables.ContainsKey(varName))
                {
                    object val = _engine.Variables[varName];
                    return val != null ? val.ToString() : value;
                }
            }

            return value;
        }
        
        /// <summary>
        /// Convert attribute names from Nimbus to XAML
        /// </summary>
        private string ConvertAttributeName(string name)
        {
            string lowerName = name.ToLower();
            
            // Skip event handlers, internal properties, and Grid definitions
            if (lowerName == "onclick" || lowerName == "click" ||
                lowerName == "onchange" || lowerName == "ontextchanged" ||
                lowerName == "onenter" || lowerName == "onvaluechanged" ||
                lowerName == "onmouseenter" || lowerName == "onmouseleave" ||
                lowerName == "name" && name == "Name" || // Name is handled, but check duplication
                lowerName == "id" ||
                lowerName == "rowdefinitions" ||
                lowerName == "columndefinitions")
            {
                return "";
            }

            // Standard mapping
            switch (lowerName)
            {
                case "name": return "x:Name";
                case "bgcolor": return "Background";
                case "fgcolor": return "Foreground";
                case "halign": return "HorizontalAlignment";
                case "valign": return "VerticalAlignment";
                case "row": return "Grid.Row";
                case "column":
                case "col": return "Grid.Column";
                case "rowspan": return "Grid.RowSpan";
                case "colspan":
                case "columnspan": return "Grid.ColumnSpan";
                case "radius":
                case "cornerradius": return "CornerRadius";
                case "bordercolor": return "BorderBrush";
                case "wrap": return "TextWrapping";
                default: return name;
            }
        }

        private string ConvertElementName(string name)
        {
            // Simple mapping, can be expanded
            switch (name)
            {
                case "Container": return "Grid";
                case "Panel": return "StackPanel";
                case "Label": return "TextBlock";
                case "Input": return "TextBox";
                case "Btn": return "Button";
                default: return name;
            }
        }
        
        /// <summary>
        /// Check if node has element children
        /// </summary>
        private bool HasElementChildren(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get attribute with default value
        /// </summary>
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            if (attr == null) return defaultValue;
            return attr.Value;
        }
        
        /// <summary>
        /// Escape XML special characters
        /// </summary>
        private string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;");
        }
    }
}