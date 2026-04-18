using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  Input Validation
    // ═══════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// NimbusInputValidation - Validation state for input fields
    /// </summary>
    public enum NimbusInputValidation
    {
        None,       // No validation state
        Valid,      // Input is valid (green)
        Invalid,    // Input is invalid (red)
        Warning     // Input has warning (yellow)
    }

    /// <summary>
    /// NimbusInputType - Extended input types
    /// </summary>
    public enum NimbusInputType
    {
        Text,
        Password,
        Email,
        Number,
        Phone,
        Url,
        Search,
        Date,
        Time,
        DateTime,
        Color,
        File,
        MultiLine
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusTextInput - Full-featured text input field
    //  Supports: Validation, Icons, Prefix/Suffix, Character count
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusTextInput : ModuleUIElement
    {
        // Content
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }          // Floating label text
        public string HelperText { get; set; }     // Helper text below input
        public string ErrorText { get; set; }      // Error message text
        public string Prefix { get; set; }         // Prefix text (e.g., "$")
        public string Suffix { get; set; }         // Suffix text (e.g., ".com")
        
        // Type & Behavior
        public NimbusInputType InputType { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public bool ShowCharCount { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsClearable { get; set; }      // Show clear "x" button
        public bool AutoFocus { get; set; }
        public string Pattern { get; set; }        // Regex validation pattern
        
        // Icons
        public string IconLeft { get; set; }
        public string IconRight { get; set; }
        
        // Validation
        public NimbusInputValidation ValidationState { get; set; }
        public bool ShowValidationIcon { get; set; }
        
        // Styling
        public string InputBackground { get; set; }
        public string FocusBorderColor { get; set; }
        public string ErrorBorderColor { get; set; }
        public string ValidBorderColor { get; set; }
        public string LabelColor { get; set; }
        public string PlaceholderColor { get; set; }
        public bool IsFocused { get; set; }
        public bool FloatingLabel { get; set; }    // Animate label to float above
        
        // Events
        public Action<string> OnTextChanged { get; set; }
        public Action OnFocus { get; set; }
        public Action OnBlur { get; set; }
        public Action OnSubmit { get; set; }
        public Action OnClear { get; set; }

        public NimbusTextInput(string id) : base(id, "NimbusTextInput")
        {
            Value = "";
            Placeholder = "Enter text...";
            Label = "";
            HelperText = "";
            ErrorText = "";
            Prefix = null;
            Suffix = null;
            
            InputType = NimbusInputType.Text;
            MaxLength = -1;
            MinLength = 0;
            ShowCharCount = false;
            IsReadOnly = false;
            IsClearable = false;
            AutoFocus = false;
            Pattern = null;
            
            IconLeft = null;
            IconRight = null;
            
            ValidationState = NimbusInputValidation.None;
            ShowValidationIcon = true;
            
            InputBackground = "#2D2D30";
            FocusBorderColor = "#6C63FF";
            ErrorBorderColor = "#CF6679";
            ValidBorderColor = "#4CAF50";
            LabelColor = "#9E9E9E";
            PlaceholderColor = "#666666";
            IsFocused = false;
            FloatingLabel = true;
            
            // Base styling
            Height = "48";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnTextChanged = null;
            OnFocus = null;
            OnBlur = null;
            OnSubmit = null;
            OnClear = null;
        }

        /// <summary>Get the effective border color based on state</summary>
        public string GetEffectiveBorderColor()
        {
            if (ValidationState == NimbusInputValidation.Invalid)
                return ErrorBorderColor;
            if (ValidationState == NimbusInputValidation.Valid)
                return ValidBorderColor;
            if (IsFocused)
                return FocusBorderColor;
            return BorderBrush;
        }

        /// <summary>Validate the current value</summary>
        public bool Validate()
        {
            if (MinLength > 0 && (Value == null || Value.Length < MinLength))
            {
                ValidationState = NimbusInputValidation.Invalid;
                ErrorText = "Minimum " + MinLength + " characters required";
                return false;
            }
            if (MaxLength > 0 && Value != null && Value.Length > MaxLength)
            {
                ValidationState = NimbusInputValidation.Invalid;
                ErrorText = "Maximum " + MaxLength + " characters allowed";
                return false;
            }
            ValidationState = NimbusInputValidation.Valid;
            ErrorText = "";
            return true;
        }

        public override void Render()
        {
            Console.WriteLine("[NimbusTextInput] Label=" + Label + " Type=" + InputType + " Validation=" + ValidationState);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusTextArea - Multi-line text input
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusTextArea : ModuleUIElement
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }
        public string HelperText { get; set; }
        public string ErrorText { get; set; }
        
        public int Rows { get; set; }             // Visible rows
        public int MaxRows { get; set; }          // Auto-grow limit
        public int MaxLength { get; set; }
        public bool ShowCharCount { get; set; }
        public bool IsReadOnly { get; set; }
        public bool AutoResize { get; set; }      // Auto-grow with content
        public bool IsResizable { get; set; }     // User can resize
        
        public NimbusInputValidation ValidationState { get; set; }
        public string FocusBorderColor { get; set; }
        public bool IsFocused { get; set; }
        
        public Action<string> OnTextChanged { get; set; }
        public Action OnFocus { get; set; }
        public Action OnBlur { get; set; }

        public NimbusTextArea(string id) : base(id, "NimbusTextArea")
        {
            Value = "";
            Placeholder = "Enter text...";
            Label = "";
            HelperText = "";
            ErrorText = "";
            
            Rows = 4;
            MaxRows = 10;
            MaxLength = -1;
            ShowCharCount = false;
            IsReadOnly = false;
            AutoResize = true;
            IsResizable = true;
            
            ValidationState = NimbusInputValidation.None;
            FocusBorderColor = "#6C63FF";
            IsFocused = false;
            
            Height = "120";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnTextChanged = null;
            OnFocus = null;
            OnBlur = null;
        }

        public override void Render()
        {
            Console.WriteLine("[NimbusTextArea] Rows=" + Rows + " Label=" + Label);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSearchInput - Search-specific input with results
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSearchInput : ModuleUIElement
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public bool ShowSearchIcon { get; set; }
        public bool ShowClearButton { get; set; }
        public bool IsExpanded { get; set; }       // Expandable search bar
        public bool ShowSuggestions { get; set; }   // Show dropdown suggestions
        public List<string> Suggestions { get; set; }
        public List<string> RecentSearches { get; set; }
        public int DebounceMs { get; set; }        // Debounce delay for search
        
        public Action<string> OnSearch { get; set; }
        public Action<string> OnTextChanged { get; set; }
        public Action<string> OnSuggestionSelected { get; set; }
        public Action OnClear { get; set; }

        public NimbusSearchInput(string id) : base(id, "SearchInput")
        {
            Value = "";
            Placeholder = "Search...";
            ShowSearchIcon = true;
            ShowClearButton = true;
            IsExpanded = true;
            ShowSuggestions = false;
            Suggestions = new List<string>();
            RecentSearches = new List<string>();
            DebounceMs = 300;
            
            Height = "40";
            Padding = "8,40,8,16";
            CornerRadius = 20;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            
            OnSearch = null;
            OnTextChanged = null;
            OnSuggestionSelected = null;
            OnClear = null;
        }

        public override void Render()
        {
            Console.WriteLine("[SearchInput] Value=" + Value + " Suggestions=" + Suggestions.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusPasswordInput - Password input with toggle visibility
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusPasswordInput : ModuleUIElement
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }
        public bool IsPasswordVisible { get; set; }
        public bool ShowToggleButton { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool ShowStrengthIndicator { get; set; }
        public int PasswordStrength { get; set; }   // 0-4 (None, Weak, Fair, Good, Strong)
        
        public NimbusInputValidation ValidationState { get; set; }
        public string ErrorText { get; set; }
        
        public Action<string> OnTextChanged { get; set; }
        public Action OnSubmit { get; set; }

        public NimbusPasswordInput(string id) : base(id, "PasswordInput")
        {
            Value = "";
            Placeholder = "Password";
            Label = "Password";
            IsPasswordVisible = false;
            ShowToggleButton = true;
            MinLength = 0;
            MaxLength = -1;
            ShowStrengthIndicator = false;
            PasswordStrength = 0;
            
            ValidationState = NimbusInputValidation.None;
            ErrorText = "";
            
            Height = "48";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnTextChanged = null;
            OnSubmit = null;
        }

        /// <summary>Calculate password strength (0-4)</summary>
        public int CalculateStrength()
        {
            if (string.IsNullOrEmpty(Value)) return 0;
            int score = 0;
            if (Value.Length >= 8) score++;
            if (Value.Length >= 12) score++;
            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
            foreach (char c in Value)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }
            if (hasUpper && hasLower) score++;
            if (hasDigit) score++;
            if (hasSpecial) score++;
            PasswordStrength = Math.Min(4, score);
            return PasswordStrength;
        }

        public override void Render()
        {
            Console.WriteLine("[PasswordInput] Strength=" + PasswordStrength + " Visible=" + IsPasswordVisible);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusNumberInput - Number input with stepper controls
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusNumberInput : ModuleUIElement
    {
        public double Value { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Step { get; set; }
        public int DecimalPlaces { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }
        public bool ShowStepper { get; set; }      // +/- buttons
        public string Unit { get; set; }           // Display unit (e.g., "px", "%")
        
        public NimbusInputValidation ValidationState { get; set; }
        
        public Action<double> OnValueChanged { get; set; }

        public NimbusNumberInput(string id) : base(id, "NumberInput")
        {
            Value = 0;
            Minimum = double.MinValue;
            Maximum = double.MaxValue;
            Step = 1;
            DecimalPlaces = 0;
            Placeholder = "0";
            Label = "";
            ShowStepper = true;
            Unit = null;
            
            ValidationState = NimbusInputValidation.None;
            
            Height = "48";
            Width = "120";
            Padding = "8,12";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnValueChanged = null;
        }

        /// <summary>Increment value by step</summary>
        public void Increment()
        {
            double newVal = Value + Step;
            if (newVal <= Maximum)
            {
                Value = Math.Round(newVal, DecimalPlaces);
                if (OnValueChanged != null) OnValueChanged(Value);
            }
        }

        /// <summary>Decrement value by step</summary>
        public void Decrement()
        {
            double newVal = Value - Step;
            if (newVal >= Minimum)
            {
                Value = Math.Round(newVal, DecimalPlaces);
                if (OnValueChanged != null) OnValueChanged(Value);
            }
        }

        public override void Render()
        {
            Console.WriteLine("[NumberInput] Value=" + Value + " Min=" + Minimum + " Max=" + Maximum);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusComboBox - Dropdown select with search
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusComboBox : ModuleUIElement
    {
        public string SelectedValue { get; set; }
        public int SelectedIndex { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }
        public List<NimbusComboBoxItem> Items { get; set; }
        public bool IsOpen { get; set; }
        public bool IsSearchable { get; set; }
        public bool AllowMultiple { get; set; }
        public List<int> SelectedIndices { get; set; }
        public string SearchText { get; set; }
        public string DropdownColor { get; set; }
        public int MaxDropdownHeight { get; set; }
        
        public NimbusInputValidation ValidationState { get; set; }
        public string ErrorText { get; set; }
        
        public Action<int> OnSelectionChanged { get; set; }
        public Action<string> OnSearchChanged { get; set; }

        public NimbusComboBox(string id) : base(id, "ComboBox")
        {
            SelectedValue = "";
            SelectedIndex = -1;
            Placeholder = "Select...";
            Label = "";
            Items = new List<NimbusComboBoxItem>();
            IsOpen = false;
            IsSearchable = false;
            AllowMultiple = false;
            SelectedIndices = new List<int>();
            SearchText = "";
            DropdownColor = "#2D2D30";
            MaxDropdownHeight = 200;
            
            ValidationState = NimbusInputValidation.None;
            ErrorText = "";
            
            Height = "48";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnSelectionChanged = null;
            OnSearchChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ComboBox] Items=" + Items.Count + " Selected=" + SelectedIndex);
        }
    }

    public class NimbusComboBoxItem
    {
        public string Value { get; set; }
        public string DisplayText { get; set; }
        public string Icon { get; set; }
        public string Group { get; set; }
        public bool IsEnabled { get; set; }

        public NimbusComboBoxItem()
        {
            Value = "";
            DisplayText = "";
            Icon = null;
            Group = null;
            IsEnabled = true;
        }

        public NimbusComboBoxItem(string value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
            Icon = null;
            Group = null;
            IsEnabled = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSwitch - Modern toggle switch (iOS/Android style)
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSwitch : ModuleUIElement
    {
        public bool IsOn { get; set; }
        public string Label { get; set; }
        public string OnLabel { get; set; }        // Text when on
        public string OffLabel { get; set; }       // Text when off
        public string ActiveColor { get; set; }
        public string InactiveColor { get; set; }
        public string ThumbColor { get; set; }
        public bool ShowLabel { get; set; }
        public string LabelPosition { get; set; }  // Left, Right
        
        public Action<bool> OnToggle { get; set; }

        public NimbusSwitch(string id) : base(id, "Switch")
        {
            IsOn = false;
            Label = "";
            OnLabel = "ON";
            OffLabel = "OFF";
            ActiveColor = "#6C63FF";
            InactiveColor = "#555555";
            ThumbColor = "#FFFFFF";
            ShowLabel = true;
            LabelPosition = "Right";
            
            Width = "48";
            Height = "24";
            CornerRadius = 12;
            
            OnToggle = null;
        }

        public override void Render()
        {
            Console.WriteLine("[Switch] IsOn=" + IsOn + " Label=" + Label);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusCheckBox - Modern checkbox with label
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusCheckBox : ModuleUIElement
    {
        public bool IsChecked { get; set; }
        public bool IsIndeterminate { get; set; }  // Third state (dash)
        public string Label { get; set; }
        public string CheckColor { get; set; }
        public string UncheckedColor { get; set; }
        public string CheckmarkColor { get; set; }
        public double CheckSize { get; set; }
        
        public Action<bool> OnChanged { get; set; }

        public NimbusCheckBox(string id) : base(id, "NimbusCheckBox")
        {
            IsChecked = false;
            IsIndeterminate = false;
            Label = "";
            CheckColor = "#6C63FF";
            UncheckedColor = "#555555";
            CheckmarkColor = "#FFFFFF";
            CheckSize = 20;
            
            Height = "24";
            CornerRadius = 4;
            
            OnChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[CheckBox] Checked=" + IsChecked + " Label=" + Label);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusRadioButton - Radio button for single selection
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusRadioButton : ModuleUIElement
    {
        public bool IsSelected { get; set; }
        public string Label { get; set; }
        public string GroupName { get; set; }      // Radio group name
        public string Value { get; set; }          // Value when selected
        public string ActiveColor { get; set; }
        public string InactiveColor { get; set; }
        public double RadioSize { get; set; }
        
        public Action<string> OnSelected { get; set; }

        public NimbusRadioButton(string id) : base(id, "RadioButton")
        {
            IsSelected = false;
            Label = "";
            GroupName = "default";
            Value = "";
            ActiveColor = "#6C63FF";
            InactiveColor = "#555555";
            RadioSize = 20;
            
            Height = "24";
            
            OnSelected = null;
        }

        public override void Render()
        {
            Console.WriteLine("[RadioButton] Selected=" + IsSelected + " Label=" + Label + " Group=" + GroupName);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusRadioGroup - Container for radio buttons
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusRadioGroup : ModuleUIElement
    {
        public string GroupName { get; set; }
        public int SelectedIndex { get; set; }
        public string SelectedValue { get; set; }
        public List<string> Labels { get; set; }
        public List<string> Values { get; set; }
        public string Orientation { get; set; }    // Vertical, Horizontal
        public double Spacing { get; set; }
        
        public Action<int, string> OnSelectionChanged { get; set; }

        public NimbusRadioGroup(string id) : base(id, "RadioGroup")
        {
            GroupName = "default";
            SelectedIndex = -1;
            SelectedValue = "";
            Labels = new List<string>();
            Values = new List<string>();
            Orientation = "Vertical";
            Spacing = 8;
            
            OnSelectionChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[RadioGroup] Name=" + GroupName + " Selected=" + SelectedIndex + " Items=" + Labels.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusRangeSlider - Dual-thumb range slider
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusRangeSlider : ModuleUIElement
    {
        public double LowValue { get; set; }
        public double HighValue { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Step { get; set; }
        public string Label { get; set; }
        public bool ShowValues { get; set; }
        public bool ShowTicks { get; set; }
        public string TrackColor { get; set; }
        public string ActiveTrackColor { get; set; }
        public string ThumbColor { get; set; }
        
        public Action<double, double> OnRangeChanged { get; set; }

        public NimbusRangeSlider(string id) : base(id, "RangeSlider")
        {
            LowValue = 20;
            HighValue = 80;
            Minimum = 0;
            Maximum = 100;
            Step = 1;
            Label = "";
            ShowValues = true;
            ShowTicks = false;
            TrackColor = "#555555";
            ActiveTrackColor = "#6C63FF";
            ThumbColor = "#FFFFFF";
            
            Height = "40";
            Width = "Auto";
            
            OnRangeChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[RangeSlider] Range=" + LowValue + "-" + HighValue + " (" + Minimum + " to " + Maximum + ")");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusColorPicker - Color picker input
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusColorPicker : ModuleUIElement
    {
        public string SelectedColor { get; set; }
        public string Label { get; set; }
        public bool ShowHexInput { get; set; }
        public bool ShowPresets { get; set; }
        public List<string> PresetColors { get; set; }
        public bool IsOpen { get; set; }
        public bool ShowAlpha { get; set; }
        
        public Action<string> OnColorChanged { get; set; }

        public NimbusColorPicker(string id) : base(id, "ColorPicker")
        {
            SelectedColor = "#6C63FF";
            Label = "Color";
            ShowHexInput = true;
            ShowPresets = true;
            PresetColors = new List<string> {
                "#F44336", "#E91E63", "#9C27B0", "#673AB7",
                "#3F51B5", "#2196F3", "#03A9F4", "#00BCD4",
                "#009688", "#4CAF50", "#8BC34A", "#CDDC39",
                "#FFEB3B", "#FFC107", "#FF9800", "#FF5722"
            };
            IsOpen = false;
            ShowAlpha = false;
            
            Width = "48";
            Height = "48";
            CornerRadius = 8;
            
            OnColorChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[ColorPicker] Color=" + SelectedColor);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusDatePicker - Date picker input
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusDatePicker : ModuleUIElement
    {
        public string SelectedDate { get; set; }   // ISO format YYYY-MM-DD
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public string DateFormat { get; set; }     // Display format
        public bool IsOpen { get; set; }
        public bool ShowTodayButton { get; set; }
        
        public NimbusInputValidation ValidationState { get; set; }
        public string ErrorText { get; set; }
        
        public Action<string> OnDateChanged { get; set; }

        public NimbusDatePicker(string id) : base(id, "DatePicker")
        {
            SelectedDate = "";
            Label = "Date";
            Placeholder = "Select date...";
            MinDate = null;
            MaxDate = null;
            DateFormat = "yyyy-MM-dd";
            IsOpen = false;
            ShowTodayButton = true;
            
            ValidationState = NimbusInputValidation.None;
            ErrorText = "";
            
            Height = "48";
            Padding = "12,16";
            CornerRadius = 8;
            Background = "#2D2D30";
            BorderBrush = "#555555";
            BorderThickness = 1;
            Foreground = "#E0E0E0";
            FontSize = 14;
            
            OnDateChanged = null;
        }

        public override void Render()
        {
            Console.WriteLine("[DatePicker] Date=" + SelectedDate + " Format=" + DateFormat);
        }
    }
}
