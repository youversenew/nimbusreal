using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Nimbus.WPF
{
    /// <summary>
    /// ModuleWindow - Displays custom UIModule hierarchy
    /// Converts UIModule to WPF controls at runtime
    /// </summary>
    public class ModuleWindow : Window
    {
        private WpfEngine _engine;
        private IUIModule _rootModule;
        private Dictionary<string, FrameworkElement> _controlMap;

        public ModuleWindow(WpfEngine engine, XmlNode rootNode, XmlNode uiNode, IUIModule rootModule)
        {
            _engine = engine;
            _rootModule = rootModule;
            _controlMap = new Dictionary<string, FrameworkElement>();

            // Configure window
            ConfigureWindow(rootNode);

            // Build WPF controls from UIModule tree
            if (_rootModule != null)
            {
                FrameworkElement wpfContent = ConvertModuleToWpf(_rootModule);
                if (wpfContent != null)
                {
                    this.Content = wpfContent;
                    RegisterControlsFromWpf(wpfContent);
                }
            }

            _engine.Log("MODULE", "ModuleWindow created with root: " + (_rootModule != null ? _rootModule.ElementType : "null"));
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

        /// <summary>
        /// Convert UIModule tree to WPF controls
        /// </summary>
        private FrameworkElement ConvertModuleToWpf(IUIModule module)
        {
            if (module == null) return null;

            FrameworkElement element = null;

            switch (module.ElementType.ToLower())
            {
                case "grid":
                    element = ConvertGrid((CustomUIGrid)module);
                    break;
                case "stackpanel":
                    element = ConvertStackPanel((CustomUIStackPanel)module);
                    break;
                case "button":
                    element = ConvertButton((CustomUIButton)module);
                    break;
                case "label":
                case "text":
                case "textblock":
                    element = ConvertLabel((CustomUILabel)module);
                    break;
                default:
                    element = ConvertGeneric((ModuleUIElement)module);
                    break;
            }

            if (element != null && !string.IsNullOrEmpty(module.Id))
            {
                element.Name = module.Id;
                _controlMap[module.Id] = element;
            }

            return element;
        }

        /// <summary>
        /// Convert Grid module
        /// </summary>
        private FrameworkElement ConvertGrid(CustomUIGrid gridModule)
        {
            Grid grid = new Grid();
            ApplyCommonProperties(grid, gridModule);

            // Parse row definitions
            if (!string.IsNullOrEmpty(gridModule.RowDefinitions))
            {
                string[] rows = gridModule.RowDefinitions.Split(',');
                foreach (string row in rows)
                {
                    string rowDef = row.Trim();
                    GridLength length = ParseGridLength(rowDef);
                    grid.RowDefinitions.Add(new RowDefinition { Height = length });
                }
            }

            // Parse column definitions
            if (!string.IsNullOrEmpty(gridModule.ColumnDefinitions))
            {
                string[] cols = gridModule.ColumnDefinitions.Split(',');
                foreach (string col in cols)
                {
                    string colDef = col.Trim();
                    GridLength length = ParseGridLength(colDef);
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = length });
                }
            }

            // Add children
            int childIndex = 0;
            foreach (var child in gridModule.Children)
            {
                FrameworkElement childElement = ConvertModuleToWpf(child);
                if (childElement != null)
                {
                    grid.Children.Add(childElement);
                    
                    // Set Grid.Row and Grid.Column if specified
                    if (child.Properties.ContainsKey("Grid.Row"))
                    {
                        int row;
                        if (int.TryParse(child.Properties["Grid.Row"].ToString(), out row))
                            Grid.SetRow(childElement, row);
                    }
                    
                    if (child.Properties.ContainsKey("Grid.Column"))
                    {
                        int col;
                        if (int.TryParse(child.Properties["Grid.Column"].ToString(), out col))
                            Grid.SetColumn(childElement, col);
                    }
                    
                    childIndex++;
                }
            }

            return grid;
        }

        /// <summary>
        /// Convert StackPanel module
        /// </summary>
        private FrameworkElement ConvertStackPanel(CustomUIStackPanel stackModule)
        {
            StackPanel panel = new StackPanel();
            ApplyCommonProperties(panel, stackModule);

            if (stackModule.Orientation.ToLower() == "horizontal")
                panel.Orientation = Orientation.Horizontal;
            else
                panel.Orientation = Orientation.Vertical;

            if (stackModule.Spacing > 0)
                panel.Margin = new Thickness(stackModule.Spacing);

            // Add children
            foreach (var child in stackModule.Children)
            {
                FrameworkElement childElement = ConvertModuleToWpf(child);
                if (childElement != null)
                {
                    panel.Children.Add(childElement);
                }
            }

            return panel;
        }

        /// <summary>
        /// Convert Button module
        /// </summary>
        private FrameworkElement ConvertButton(CustomUIButton buttonModule)
        {
            Button button = new Button();
            button.Content = buttonModule.Text;
            ApplyCommonProperties(button, buttonModule);

            // Add click handler if exists
            if (buttonModule.OnClick != null)
            {
                button.Click += (s, e) => buttonModule.OnClick();
            }

            return button;
        }

        /// <summary>
        /// Convert Label module
        /// </summary>
        private FrameworkElement ConvertLabel(CustomUILabel labelModule)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = labelModule.Text;
            textBlock.FontSize = labelModule.FontSize;
            ApplyCommonProperties(textBlock, labelModule);

            return textBlock;
        }

        /// <summary>
        /// Convert generic module to Border
        /// </summary>
        private FrameworkElement ConvertGeneric(ModuleUIElement moduleElement)
        {
            Border border = new Border();
            ApplyCommonProperties(border, moduleElement);

            // If has text content, add it
            if (!string.IsNullOrEmpty(moduleElement.Content))
            {
                TextBlock tb = new TextBlock { Text = moduleElement.Content };
                border.Child = tb;
            }

            return border;
        }

        /// <summary>
        /// Apply common properties to WPF element
        /// </summary>
        private void ApplyCommonProperties(FrameworkElement element, ModuleUIElement moduleElement)
        {
            // Background - only for Control and Panel
            if (!string.IsNullOrEmpty(moduleElement.Background) && moduleElement.Background != "Transparent")
            {
                try
                {
                    Brush bgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(moduleElement.Background));
                    if (element is Control)
                        ((Control)element).Background = bgBrush;
                    else if (element is Panel)
                        ((Panel)element).Background = bgBrush;
                    else if (element is Border)
                        ((Border)element).Background = bgBrush;
                }
                catch { }
            }

            // Foreground - only for Control
            if (!string.IsNullOrEmpty(moduleElement.Foreground))
            {
                try
                {
                    if (element is Control)
                        ((Control)element).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(moduleElement.Foreground));
                }
                catch { }
            }

            // Size - parse string values (px, %, Auto, *)
            if (moduleElement.Width != null && moduleElement.Width != "Auto")
            {
                double widthVal;
                if (double.TryParse(moduleElement.Width, out widthVal))
                    element.Width = widthVal;
                else if (moduleElement.Width == "*")
                    element.Width = double.NaN;  // Stretch
            }
            
            if (moduleElement.Height != null && moduleElement.Height != "Auto")
            {
                double heightVal;
                if (double.TryParse(moduleElement.Height, out heightVal))
                    element.Height = heightVal;
                else if (moduleElement.Height == "*")
                    element.Height = double.NaN;  // Stretch
            }

            // Margins - parse comma-separated values
            if (moduleElement.Margin != null && moduleElement.Margin != "0")
            {
                double marginVal;
                if (double.TryParse(moduleElement.Margin, out marginVal) && marginVal > 0)
                    element.Margin = new Thickness(marginVal);
            }

            // Alignment
            element.HorizontalAlignment = ParseHorizontalAlignment(moduleElement.HorizontalAlignment);
            element.VerticalAlignment = ParseVerticalAlignment(moduleElement.VerticalAlignment);
        }

        /// <summary>
        /// Register all named controls from WPF tree
        /// </summary>
        private void RegisterControlsFromWpf(FrameworkElement root)
        {
            if (root == null) return;

            if (!string.IsNullOrEmpty(root.Name))
            {
                _engine.RegisterControl(root.Name, root);
            }

            // Recursively register children
            if (root is Panel)
            {
                Panel panel = (Panel)root;
                foreach (UIElement child in panel.Children)
                {
                    if (child is FrameworkElement)
                        RegisterControlsFromWpf((FrameworkElement)child);
                }
            }
            else if (root is ContentControl)
            {
                ContentControl cc = (ContentControl)root;
                if (cc.Content is FrameworkElement)
                    RegisterControlsFromWpf((FrameworkElement)cc.Content);
            }
            else if (root is Decorator)
            {
                Decorator dec = (Decorator)root;
                if (dec.Child is FrameworkElement)
                    RegisterControlsFromWpf((FrameworkElement)dec.Child);
            }
        }

        /// <summary>
        /// Parse GridLength from string (*, 200, Auto)
        /// </summary>
        private GridLength ParseGridLength(string value)
        {
            if (value == "*")
                return new GridLength(1, GridUnitType.Star);
            if (value == "Auto")
                return GridLength.Auto;
            
            double num;
            if (double.TryParse(value, out num))
                return new GridLength(num);
            
            return new GridLength(1, GridUnitType.Star);
        }

        /// <summary>
        /// Parse HorizontalAlignment
        /// </summary>
        private HorizontalAlignment ParseHorizontalAlignment(string value)
        {
            switch (value.ToLower())
            {
                case "left": return HorizontalAlignment.Left;
                case "right": return HorizontalAlignment.Right;
                case "center": return HorizontalAlignment.Center;
                case "stretch": default: return HorizontalAlignment.Stretch;
            }
        }

        /// <summary>
        /// Parse VerticalAlignment
        /// </summary>
        private VerticalAlignment ParseVerticalAlignment(string value)
        {
            switch (value.ToLower())
            {
                case "top": return VerticalAlignment.Top;
                case "bottom": return VerticalAlignment.Bottom;
                case "center": return VerticalAlignment.Center;
                case "stretch": default: return VerticalAlignment.Stretch;
            }
        }

        /// <summary>
        /// Get attribute from XML node
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
