using System;
using System.Drawing;
using System.Windows.Forms;
using Nimbus.MDUI;

namespace Nimbus.Examples
{
    /// <summary>
    /// Complete Material Design UI Example Application
    /// Demonstrates all MDui components and features
    /// </summary>
    public class MDUIShowcase : Form
    {
        private MDEngine _engine;
        private MDPanel _mainPanel;
        private MDLinearLayout _contentLayout;

        public MDUIShowcase()
        {
            InitializeForm();
            InitializeUI();
        }

        private void InitializeForm()
        {
            this.Text = "Material Design UI Showcase - Complete Framework";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = MDColors.Background;
            this.Font = MDTypography.BodyMedium;
        }

        private void InitializeUI()
        {
            // Initialize MDEngine
            _engine = new MDEngine(this);
            _engine.SetTheme(MDTheme.Light());

            CreateHeader();
            CreateButtonDemo();
            CreateInputDemo();
            CreateControlsDemo();
            CreateLayoutDemo();
        }

        private void CreateHeader()
        {
            var headerPanel = new MDPanel
            {
                Bounds = new Rectangle(0, 0, this.ClientSize.Width, 80),
                BackgroundColor = MDColors.Primary,
                Elevation = ElevationLevel.Level2,
                CornerRadius = 0
            };

            var titleLabel = new MDLabel
            {
                Text = "Material Design UI Framework - Complete",
                TextFont = MDTypography.HeadlineMedium,
                ForeColor = Color.White,
                Bounds = new Rectangle(24, 20, 400, 40)
            };

            _engine.AddElement(headerPanel);
            _engine.AddElement(titleLabel);
        }

        private void CreateButtonDemo()
        {
            var sectionLabel = new MDLabel
            {
                Text = "Buttons",
                TextFont = MDTypography.TitleLarge,
                ForeColor = MDColors.Primary,
                Bounds = new Rectangle(24, 100, 200, 30)
            };

            // Contained Button
            var containedBtn = new MDButton
            {
                Text = "Contained",
                ButtonType = MDButtonType.Contained,
                Bounds = new Rectangle(24, 140, 150, 48)
            };
            containedBtn.Click += (s, e) => MessageBox.Show("Contained button clicked!");

            // Outlined Button
            var outlinedBtn = new MDButton
            {
                Text = "Outlined",
                ButtonType = MDButtonType.Outlined,
                Bounds = new Rectangle(184, 140, 150, 48)
            };
            outlinedBtn.Click += (s, e) => MessageBox.Show("Outlined button clicked!");

            // Text Button
            var textBtn = new MDButton
            {
                Text = "Text",
                ButtonType = MDButtonType.Text,
                Bounds = new Rectangle(344, 140, 150, 48)
            };
            textBtn.Click += (s, e) => MessageBox.Show("Text button clicked!");

            // Elevated Button
            var elevatedBtn = new MDButton
            {
                Text = "Elevated",
                ButtonType = MDButtonType.Elevated,
                Bounds = new Rectangle(504, 140, 150, 48)
            };
            elevatedBtn.Click += (s, e) => MessageBox.Show("Elevated button clicked!");

            // Tonal Button
            var tonalBtn = new MDButton
            {
                Text = "Tonal",
                ButtonType = MDButtonType.Tonal,
                Bounds = new Rectangle(664, 140, 150, 48)
            };
            tonalBtn.Click += (s, e) => MessageBox.Show("Tonal button clicked!");

            _engine.AddElement(sectionLabel);
            _engine.AddElement(containedBtn);
            _engine.AddElement(outlinedBtn);
            _engine.AddElement(textBtn);
            _engine.AddElement(elevatedBtn);
            _engine.AddElement(tonalBtn);
        }

        private void CreateInputDemo()
        {
            var sectionLabel = new MDLabel
            {
                Text = "Input Fields",
                TextFont = MDTypography.TitleLarge,
                ForeColor = MDColors.Primary,
                Bounds = new Rectangle(24, 220, 300, 30)
            };

            // Text Input
            var textInput = new MDTextBox
            {
                Placeholder = "Enter text...",
                Bounds = new Rectangle(24, 260, 300, 56)
            };

            // Email Input
            var emailInput = new MDTextBox
            {
                Placeholder = "Enter email...",
                Bounds = new Rectangle(344, 260, 300, 56)
            };

            // Password Input
            var passwordInput = new MDTextBox
            {
                Placeholder = "Enter password...",
                IsPassword = true,
                Bounds = new Rectangle(664, 260, 300, 56)
            };

            // Checkbox
            var checkbox1 = new MDCheckBox
            {
                Text = "Remember me",
                Bounds = new Rectangle(24, 330, 200, 40)
            };

            // Another checkbox
            var checkbox2 = new MDCheckBox
            {
                Text = "I agree to terms",
                Bounds = new Rectangle(24, 370, 200, 40)
            };

            _engine.AddElement(sectionLabel);
            _engine.AddElement(textInput);
            _engine.AddElement(emailInput);
            _engine.AddElement(passwordInput);
            _engine.AddElement(checkbox1);
            _engine.AddElement(checkbox2);
        }

        private void CreateControlsDemo()
        {
            var sectionLabel = new MDLabel
            {
                Text = "Controls",
                TextFont = MDTypography.TitleLarge,
                ForeColor = MDColors.Primary,
                Bounds = new Rectangle(24, 430, 300, 30)
            };

            // Slider
            var slider = new MDSlider
            {
                Value = 50,
                MinValue = 0,
                MaxValue = 100,
                Bounds = new Rectangle(24, 470, 300, 40)
            };

            // Progress Bar
            var progressBar = new MDProgressBar
            {
                Value = 65,
                MaxValue = 100,
                Bounds = new Rectangle(24, 520, 300, 4)
            };

            var progressLabel = new MDLabel
            {
                Text = $"Progress: 65%",
                TextFont = MDTypography.BodySmall,
                Bounds = new Rectangle(24, 530, 300, 20)
            };

            // Another Progress Bar (Indeterminate)
            var indeterminateBar = new MDProgressBar
            {
                Indeterminate = true,
                Bounds = new Rectangle(24, 560, 300, 4)
            };

            _engine.AddElement(sectionLabel);
            _engine.AddElement(slider);
            _engine.AddElement(progressBar);
            _engine.AddElement(progressLabel);
            _engine.AddElement(indeterminateBar);
        }

        private void CreateLayoutDemo()
        {
            var sectionLabel = new MDLabel
            {
                Text = "Layouts",
                TextFont = MDTypography.TitleLarge,
                ForeColor = MDColors.Primary,
                Bounds = new Rectangle(344, 430, 300, 30)
            };

            // Panel Demo
            var panel1 = new MDPanel
            {
                Bounds = new Rectangle(344, 470, 280, 100),
                Elevation = ElevationLevel.Level2,
                BackgroundColor = MDColors.Surface
            };

            var panelLabel1 = new MDLabel
            {
                Text = "Panel with Elevation",
                TextFont = MDTypography.TitleSmall,
                Bounds = new Rectangle(360, 490, 250, 30)
            };

            var panelText1 = new MDLabel
            {
                Text = "Material Design cards with elevation and customizable properties",
                TextFont = MDTypography.BodySmall,
                Bounds = new Rectangle(360, 520, 250, 40)
            };

            // Panel Demo 2
            var panel2 = new MDPanel
            {
                Bounds = new Rectangle(664, 470, 280, 100),
                Elevation = ElevationLevel.Level4,
                BorderColor = MDColors.Primary,
                BorderWidth = 2
            };

            var panelLabel2 = new MDLabel
            {
                Text = "Panel with Border",
                TextFont = MDTypography.TitleSmall,
                Bounds = new Rectangle(680, 490, 250, 30)
            };

            var panelText2 = new MDLabel
            {
                Text = "Customizable borders and styling",
                TextFont = MDTypography.BodySmall,
                Bounds = new Rectangle(680, 520, 250, 40)
            };

            _engine.AddElement(sectionLabel);
            _engine.AddElement(panel1);
            _engine.AddElement(panelLabel1);
            _engine.AddElement(panelText1);
            _engine.AddElement(panel2);
            _engine.AddElement(panelLabel2);
            _engine.AddElement(panelText2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _engine?.Render(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MDUIShowcase());
        }
    }
}
