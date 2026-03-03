# Material Design UI Framework (MDui)

Complete WPF-like custom UI library built using **GDI+** in C#. A fully-featured Material Design implementation with beautiful components, animations, themes, and full parametrization.

## Features

### ✨ Core Components
- **MDButton** - 5 button types (Contained, Outlined, Text, Elevated, Tonal)
- **MDLabel** - Text rendering with auto-sizing
- **MDTextBox** - Text input with password support
- **MDCheckBox** - Checkbox with animation states
- **MDSlider** - Range slider with value display
- **MDProgressBar** - Linear progress indicator
- **MDPanel** - Container with elevation and borders
- **MDLinearLayout** - Vertical/Horizontal layout
- **MDGridLayout** - Grid-based layout system

### 🎨 Design System
- **Material Design 3 Color Palette** - 20+ predefined colors
- **Typography System** - 12 text styles (Display, Headline, Title, Body, Label)
- **Elevation Support** - 10 elevation levels with shadow rendering
- **Rounded Corners** - Customizable corner radius
- **Ripple Effects** - Interactive ripple animations
- **Theme System** - Light, Dark, Blue, and Green themes

### 🔧 Advanced Features
- **GDI+ Rendering** - Hardware-accelerated graphics
- **Event System** - Click, Hover, Focus, KeyDown, KeyUp, MouseMove events
- **Focus Management** - Keyboard navigation support
- **Animation Framework** - Property-based animations
- **Layout Helpers** - Auto-alignment and positioning
- **DPI Scaling** - Automatic DPI awareness
- **Double Buffering** - Flicker-free rendering

### 📦 Full Parametrization
Every component supports:
- Size and position customization
- Color theming (background, foreground, borders)
- Font customization (family, size, style)
- Elevation and shadow effects
- Padding and margin settings
- Enable/Disable states
- Custom tags and metadata

## Architecture

```
MDui System
├── MDElement (Base class)
│   ├── MDControl (Interactive)
│   │   ├── MDButton
│   │   ├── MDTextBox
│   │   ├── MDCheckBox
│   │   ├── MDSlider
│   │   └── ...
│   └── MDContainer (Layout)
│       ├── MDPanel
│       ├── MDLinearLayout
│       ├── MDGridLayout
│       └── ...
├── MDEngine (Rendering engine)
├── MDColors (Color palette)
├── MDTypography (Font styles)
├── MDTheme (Theme system)
└── MDLayoutHelper (Layout utilities)
```

## Quick Start

### Basic Usage

```csharp
using Nimbus.MDUI;
using System.Windows.Forms;

public class MainWindow : Form
{
    private MDEngine _engine;

    public MainWindow()
    {
        this.Size = new System.Drawing.Size(800, 600);
        _engine = new MDEngine(this);
        
        // Add components
        var button = new MDButton
        {
            Text = "Click me",
            ButtonType = MDButtonType.Contained,
            Bounds = new System.Drawing.Rectangle(50, 50, 200, 48)
        };
        button.Click += (s, e) => MessageBox.Show("Clicked!");
        
        _engine.AddElement(button);
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        _engine.Render(e.Graphics);
    }
}
```

### Button Examples

```csharp
// Contained Button
var button = new MDButton
{
    Text = "Save",
    ButtonType = MDButtonType.Contained,
    ContainedColor = MDColors.Primary,
    Bounds = new Rectangle(10, 10, 150, 48)
};

// Outlined Button
var outlineBtn = new MDButton
{
    Text = "Cancel",
    ButtonType = MDButtonType.Outlined,
    Bounds = new Rectangle(170, 10, 150, 48)
};

// Text Button
var textBtn = new MDButton
{
    Text = "Learn More",
    ButtonType = MDButtonType.Text,
    TextColor = MDColors.Primary,
    Bounds = new Rectangle(330, 10, 150, 48)
};
```

### Text Input Examples

```csharp
// Regular Text Input
var textInput = new MDTextBox
{
    Placeholder = "Enter your name",
    Bounds = new Rectangle(10, 70, 300, 56)
};

// Email Input
var emailInput = new MDTextBox
{
    Placeholder = "Enter email",
    Bounds = new Rectangle(10, 140, 300, 56)
};

// Password Input
var passwordInput = new MDTextBox
{
    Placeholder = "Enter password",
    IsPassword = true,
    Bounds = new Rectangle(10, 210, 300, 56)
};

// Get value
string inputValue = textInput.Text;
textInput.Text = "New value";
```

### Checkbox Example

```csharp
var checkbox = new MDCheckBox
{
    Text = "I agree to terms",
    Bounds = new Rectangle(10, 10, 250, 40)
};
checkbox.Click += (s, e) => 
{
    MessageBox.Show($"Checked: {checkbox.Checked}");
};
```

### Slider Example

```csharp
var slider = new MDSlider
{
    Value = 50,
    MinValue = 0,
    MaxValue = 100,
    ShowLabel = true,
    Bounds = new Rectangle(10, 10, 300, 40)
};
```

### Progress Bar Example

```csharp
// Determinate Progress
var progressBar = new MDProgressBar
{
    Value = 75,
    MaxValue = 100,
    Bounds = new Rectangle(10, 10, 300, 4)
};

// Indeterminate Progress (loading)
var loadingBar = new MDProgressBar
{
    Indeterminate = true,
    Bounds = new Rectangle(10, 30, 300, 4)
};
```

### Panels and Containers

```csharp
// Panel with elevation
var panel = new MDPanel
{
    Bounds = new Rectangle(10, 10, 400, 200),
    BackgroundColor = MDColors.Surface,
    Elevation = ElevationLevel.Level2,
    BorderColor = MDColors.SurfaceVariant,
    BorderWidth = 1,
    CornerRadius = 12
};
_engine.AddElement(panel);

// Add content to panel
var label = new MDLabel
{
    Text = "Panel Content",
    TextFont = MDTypography.TitleMedium,
    Bounds = new Rectangle(30, 30, 350, 30)
};
_engine.AddElement(label);
```

### Linear Layout

```csharp
var layout = new MDLinearLayout
{
    LayoutOrientation = MDLinearLayout.Orientation.Vertical,
    Spacing = 8,
    Bounds = new Rectangle(10, 10, 400, 400),
    Padding = new Padding(16)
};

layout.AddChild(new MDButton { Text = "Button 1", Bounds = new Rectangle(0, 0, 300, 48) });
layout.AddChild(new MDButton { Text = "Button 2", Bounds = new Rectangle(0, 0, 300, 48) });
layout.AddChild(new MDButton { Text = "Button 3", Bounds = new Rectangle(0, 0, 300, 48) });

_engine.AddElement(layout);
```

### Grid Layout

```csharp
var grid = new MDGridLayout
{
    Columns = 2,
    Rows = 2,
    CellSpacing = 16,
    Bounds = new Rectangle(10, 10, 500, 400)
};

for (int i = 0; i < 4; i++)
{
    var btn = new MDButton { Text = $"Item {i + 1}" };
    grid.AddChild(btn);
}

_engine.AddElement(grid);
```

## Theming

### Built-in Themes

```csharp
MDEngine engine = new MDEngine(this);

// Light Theme (default)
engine.SetTheme(MDTheme.Light());

// Dark Theme
engine.SetTheme(MDTheme.Dark());

// Blue Theme
engine.SetTheme(MDTheme.BlueTheme());

// Green Theme
engine.SetTheme(MDTheme.GreenTheme());

// Custom Theme
var customTheme = new MDTheme
{
    Primary = Color.FromArgb(255, 100, 50),
    PrimaryLight = Color.FromArgb(255, 150, 100),
    PrimaryDark = Color.FromArgb(200, 50, 20),
    Secondary = Color.FromArgb(100, 200, 255),
    Surface = Color.FromArgb(240, 240, 240),
    Background = Color.White
};
engine.SetTheme(customTheme);
```

### Color Customization

```csharp
// Change global colors
MDColors.Primary = Color.FromArgb(103, 58, 183);
MDColors.Secondary = Color.FromArgb(0, 188, 212);
MDColors.Success = Color.FromArgb(56, 142, 60);
MDColors.Error = Color.FromArgb(179, 38, 30);

// Component-specific colors
var button = new MDButton
{
    ContainedColor = Color.FromArgb(100, 200, 50),
    ContainedHoverColor = Color.FromArgb(150, 220, 100),
    TextColor = Color.White
};
```

## Animations

```csharp
// Animate button bounds
var button = new MDButton { Text = "Animate Me" };

// Animate width over 500ms
_engine.AnimateProperty(button, "Width", 300, 500);

// Animate height
_engine.AnimateProperty(button, "Height", 100, 500);
```

## Layout Helpers

```csharp
var element = new MDButton { Bounds = new Rectangle(0, 0, 100, 50) };

// Center horizontally
MDLayoutHelper.CenterHorizontal(element, containerWidth: 800);

// Center vertically
MDLayoutHelper.CenterVertical(element, containerHeight: 600);

// Center both
MDLayoutHelper.Center(element, containerWidth: 800, containerHeight: 600);

// Align to edges
MDLayoutHelper.AlignTop(element, margin: 16);
MDLayoutHelper.AlignBottom(element, containerHeight: 600, margin: 16);
MDLayoutHelper.AlignLeft(element, margin: 16);
MDLayoutHelper.AlignRight(element, containerWidth: 800, margin: 16);
```

## Event Handling

```csharp
var button = new MDButton { Text = "Click me" };

// Click events
button.Click += (sender, e) => 
{
    MessageBox.Show("Button clicked!");
};

button.DoubleClick += (sender, e) =>
{
    MessageBox.Show("Button double-clicked!");
};

// Hover events
button.HoverChanged += (sender, e) =>
{
    // Handle hover
};

// Focus events
button.FocusChanged += (sender, e) =>
{
    // Handle focus
};

// Text input
var textBox = new MDTextBox();
textBox.OnTextInput("example text");
```

## Advanced Features

### Focus Management

```csharp
var button = new MDButton();
_engine.SetFocus(button);

var focusedElement = _engine.GetFocused();
if (focusedElement != null)
{
    MessageBox.Show($"Focused: {focusedElement.Name}");
}
```

### Hit Testing

```csharp
var element = new MDButton { Bounds = new Rectangle(10, 10, 100, 50) };
bool isInside = element.HitTest(new Point(50, 30)); // true
```

### Custom Rendering

```csharp
_engine.PreRender += (sender, e) =>
{
    // Draw before components
};

_engine.PostRender += (sender, e) =>
{
    // Draw after components
};
```

## Component Reference

### MDButton
- **ButtonType**: Contained, Outlined, Text, Elevated, Tonal
- **Size**: Small, Medium, Large
- **Properties**: Text, TextFont, ContainedColor, TextColor, CornerRadius
- **Events**: Click, DoubleClick, HoverChanged, FocusChanged

### MDTextBox
- **Properties**: Text, Placeholder, IsPassword, TextFont, OutlineColor, OutlineFocusColor
- **Events**: TextChanged (via OnTextInput), EnabledChanged, VisibleChanged
- **Methods**: Clear(), SetText(string)

### MDCheckBox
- **Properties**: Checked, Text, TextFont
- **Events**: Click, HoverChanged, FocusChanged

### MDSlider
- **Properties**: Value, MinValue, MaxValue, ShowLabel
- **Range**: -∞ to +∞ (customizable)

### MDProgressBar
- **Properties**: Value, MaxValue, Indeterminate, CornerRadius
- **Usage**: Async progress indication, loading states

### MDPanel
- **Properties**: BackgroundColor, BorderColor, BorderWidth, Elevation, CornerRadius
- **Methods**: AddChild(), RemoveChild(), ClearChildren()

## Performance Tips

1. **Use double buffering** - Enabled by default
2. **Minimize elements** - Reuse containers instead of creating many individual elements
3. **Set appropriate sizes** - Don't animate large elements frequently
4. **Use VSync** - Enable for smooth rendering
5. **Batch updates** - Invalidate once per frame

## Compatibility

- **.NET Framework**: 4.6+
- **.NET Core**: 3.1+
- **.NET 5+**: Full support
- **Windows Forms**: Required

## Files

- **MDui.cs** - Core framework (15+ components, 2000+ lines)
- **MDEngine.cs** - Rendering engine, theme system, layout helpers
- **MDUIShowcase.cs** - Complete example application
- **README.md** - This documentation

## Example Application

Run the showcase to see all components:

```csharp
// Compile
csc MDui.cs MDEngine.cs MDUIShowcase.cs /target:winexe

// Run
MDUIShowcase.exe
```

The showcase demonstrates:
- ✅ All button types
- ✅ Text input with validation
- ✅ Checkboxes and sliders
- ✅ Progress bars
- ✅ Panels with elevation
- ✅ Event handling
- ✅ Theme switching

## Contributing

To extend the framework:

1. Create a new class inheriting from `MDElement` or `MDControl`
2. Implement the abstract `Draw()` method
3. Override event handlers as needed
4. Add to `MDEngine` for rendering

## License

Part of the Nimbus Framework
