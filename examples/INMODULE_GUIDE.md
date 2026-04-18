# InModule System - Nimbus Framework Documentation

## Overview
The **inModule** system allows you to create UI without WPF dependency. Instead of using WPF controls, custom C# classes render the interface directly.

## Why Use InModule?

✅ **No WPF Dependency** - Reduce framework bloat  
✅ **Custom Rendering** - Full control over UI elements  
✅ **Lightweight** - Minimal overhead  
✅ **Flexible** - Easy to extend with custom elements  
✅ **Performance** - Optimized for simple UI scenarios  

## How to Enable InModule

### XML Attribute
Add `inModule="true"` to your App declaration:

```xml
<App Name="My App"
     Width="800"
     Height="600"
     inModule="true">
     ...
</App>
```

### Without InModule (Default)
```xml
<App Name="My App"
     Width="800"
     Height="600">
     <!-- Uses WPF (inModule=false by default) -->
     ...
</App>
```

## Supported Elements

### Layout Containers
- **Grid** - Multi-cell layout with row/column definitions
- **StackPanel** - Linear layout (Horizontal or Vertical)

### Interactive Elements
- **Button** - Clickable button with text
- **Label** / **TextBlock** - Text display

## Element Properties

### Common Properties (All Elements)
```xml
Name="elementId"              <!-- Element identifier -->
Background="#FFFFFF"         <!-- Background color (hex or {Color.Name}) -->
Foreground="#000000"         <!-- Text color -->
Width="200"                  <!-- Width in pixels -->
Height="100"                <!-- Height in pixels -->
Margin="5"                   <!-- Margin in pixels -->
Padding="8"                  <!-- Padding in pixels -->
HorizontalAlignment="Center" <!-- Left, Center, Right, Stretch -->
VerticalAlignment="Center"   <!-- Top, Center, Bottom, Stretch -->
```

### Grid Properties
```xml
<Grid RowDefinitions="*,auto,100"
      ColumnDefinitions="200,*,100">
```

- `*` = Fill available space (proportional)
- `auto` = Fit to content
- `200` = Fixed pixel size

### StackPanel Properties
```xml
<StackPanel Orientation="Vertical"
            Spacing="10">
```

- `Orientation` = Vertical or Horizontal
- `Spacing` = Gap between children

### Button Properties
```xml
<Button Text="Click Me"
        OnClick="OnButtonClick">
```

### Label Properties
```xml
<Label Text="Hello World"
       FontSize="16">
```

## Color References

Use component colors via `{Color.ColorName}` syntax:

```xml
Background="{Color.BgMain}"
Foreground="{Color.TextPrimary}"
```

Define colors in Components section:
```xml
<Components>
    <Colors>
        <Color Name="BgMain" Value="#1E1E1E"/>
        <Color Name="TextPrimary" Value="#FFFFFF"/>
    </Colors>
</Components>
```

## Example: Simple Counter

```xml
<?xml version="1.0" encoding="utf-8"?>
<App Name="Counter App"
     Width="400"
     Height="300"
     inModule="true">

    <Logic>
        <Var Name="count" Value="0" Type="int"/>
        
        <Handler Name="OnIncrement">
            <Increment Var="count"/>
            <Set Control="countLabel" Property="Text" Value="Count: {count}"/>
        </Handler>
    </Logic>

    <UI>
        <StackPanel Orientation="Vertical">
            <Label Text="Simple Counter"
                   FontSize="24"
                   Foreground="#FF69B4"/>
            
            <Label Name="countLabel"
                   Text="Count: 0"
                   FontSize="18"/>
            
            <Button Text="Increment"
                   OnClick="OnIncrement"
                   Background="#4CAF50"/>
        </StackPanel>
    </UI>
</App>
```

## Example: Two-Column Layout

```xml
<UI>
    <Grid ColumnDefinitions="*,*"
          RowDefinitions="*,50">
        
        <!-- Column 1 -->
        <StackPanel>
            <Label Text="Left Column"/>
        </StackPanel>
        
        <!-- Column 2 -->
        <StackPanel>
            <Label Text="Right Column"/>
        </StackPanel>
        
        <!-- Footer (spans both columns) -->
        <Label Text="Footer"/>
    </Grid>
</UI>
```

## Advanced: Custom Components

For advanced use cases, extend the UIModule system:

```csharp
public class CustomUIPanel : ModuleUIElement
{
    public CustomUIPanel(string id) : base(id, "CustomPanel")
    {
        // Custom initialization
    }
    
    public override void Render()
    {
        // Custom rendering logic
    }
}
```

## Migration from WPF to InModule

1. **Set inModule="true"** in App
2. **Keep XML structure** - Same Layout and Logic sections
3. **Verify elements** - Use supported elements (Grid, StackPanel, Button, Label)
4. **Test rendering** - Check positioning and alignment
5. **Optimize styles** - Adjust colors and spacing as needed

## Performance Tips

✓ Use **StackPanel** for simple linear layouts  
✓ Use **Grid** for complex multi-column layouts  
✓ Minimize **nesting depth**  
✓ Use **color references** instead of inline hex values  
✓ Prefer **relative sizing** (*) over fixed pixels  

## Limitations

❌ **No animations** (yet - can be added)  
❌ **No complex styling** (borders, shadows, effects)  
❌ **No templates** (can be implemented)  
❌ **Limited control types** (extensible via plugins)  

## Examples

Check the `/examples` folder for:
- `inmodule-basic.xml` - Simple getting started
- `inmodule-layout.xml` - Complex layouts
- `inmodule-comparison.xml` - WPF vs inModule comparison

## Troubleshooting

**Q: UI not appearing?**  
A: Check that `inModule="true"` is set in App element

**Q: Colors not resolving?**  
A: Ensure color is defined in Components section

**Q: Layout looks wrong?**  
A: Verify RowDefinitions and ColumnDefinitions syntax

**Q: Custom element not working?**  
A: Check element name case sensitivity (Grid, StackPanel, Button, Label)

---

**For more help**, check the [Nimbus Framework Documentation](../README.md)
