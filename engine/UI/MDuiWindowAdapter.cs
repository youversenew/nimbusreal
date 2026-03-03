using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using Nimbus.MDUI;

namespace Nimbus.WPF
{
    /// <summary>
    /// Adapter class to make MDui Form (Windows Forms) compatible with WPF Window interface
    /// Bridges MDui and WPF ecosystems
    /// </summary>
    public class MDuiWindowAdapter : Window
    {
        private System.Windows.Forms.Form _mduiForm;
        private MDEngine _mdEngine;
        private WindowsFormsHost _host;
        private System.Windows.Controls.Panel _container;
        
        public MDuiWindowAdapter(System.Windows.Forms.Form form, MDEngine engine)
        {
            _mduiForm = form;
            _mdEngine = engine;
            
            InitializeAdapter();
        }
        
        private void InitializeAdapter()
        {
            // Setup WPF Window
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Width = _mduiForm.Width;
            this.Height = _mduiForm.Height;
            this.Title = _mduiForm.Text;
            this.Background = new SolidColorBrush(Colors.White);
            
            // Create container
            _container = new Grid();
            this.Content = _container;
            
            // Host the Forms control in WPF
            _host = new WindowsFormsHost();
            _host.Child = _mduiForm;
            _container.Children.Add(_host);
            
            // Forward events
            this.Loaded += (s, e) =>
            {
                _mduiForm.Show();
            };
            
            this.Closing += (s, e) =>
            {
                _mduiForm?.Dispose();
            };
        }
        
        /// <summary>
        /// Get MDEngine for direct interaction
        /// </summary>
        public MDEngine GetMDEngine()
        {
            return _mdEngine;
        }
        
        /// <summary>
        /// Get underlying Forms Form
        /// </summary>
        public System.Windows.Forms.Form GetFormsControl()
        {
            return _mduiForm;
        }
    }
}
