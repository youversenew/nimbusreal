using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ═══════════════════════════════════════════════════════════════════
    //  NimbusWrapPanel - Wrapping flow layout
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusWrapPanel : ModuleUIElement
    {
        public string Direction { get; set; }       // Row, Column
        public double HGap { get; set; }            // Horizontal gap
        public double VGap { get; set; }            // Vertical gap
        public string Alignment { get; set; }       // Start, Center, End, SpaceBetween

        public NimbusWrapPanel(string id) : base(id, "WrapPanel")
        {
            Direction = "Row";
            HGap = 8;
            VGap = 8;
            Alignment = "Start";
            Background = "Transparent";
        }

        public override void Render()
        {
            Console.WriteLine("[WrapPanel] Direction=" + Direction + " HGap=" + HGap + " VGap=" + VGap);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusGridLayout - CSS Grid-like layout
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusGridLayout : ModuleUIElement
    {
        public string TemplateColumns { get; set; } // "1fr 2fr 1fr" or "200px auto"
        public string TemplateRows { get; set; }    // "auto 1fr auto"
        public double ColGap { get; set; }
        public double RowGap { get; set; }
        public string AlignItems { get; set; }      // Start, Center, End, Stretch
        public string JustifyItems { get; set; }    // Start, Center, End, Stretch

        public NimbusGridLayout(string id) : base(id, "GridLayout")
        {
            TemplateColumns = "1fr 1fr";
            TemplateRows = "auto";
            ColGap = 8;
            RowGap = 8;
            AlignItems = "Stretch";
            JustifyItems = "Stretch";
            Background = "Transparent";
        }

        public override void Render()
        {
            Console.WriteLine("[GridLayout] Columns=" + TemplateColumns + " Rows=" + TemplateRows);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusContainer - Constrained width centered container
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusContainer : ModuleUIElement
    {
        public double MaxContainerWidth { get; set; }
        public bool CenterContent { get; set; }
        public string ContainerPadding { get; set; }

        public NimbusContainer(string id) : base(id, "Container")
        {
            MaxContainerWidth = 1200;
            CenterContent = true;
            ContainerPadding = "0,16";
            
            Width = "Auto";
            Background = "Transparent";
            HorizontalAlignment = "Center";
        }

        public override void Render()
        {
            Console.WriteLine("[Container] MaxWidth=" + MaxContainerWidth + " Center=" + CenterContent);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSpacer - Flexible space that fills available room
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSpacer : ModuleUIElement
    {
        public double FlexFactor { get; set; }

        public NimbusSpacer(string id) : base(id, "Spacer")
        {
            FlexFactor = 1;
            Width = "Auto";
            Height = "Auto";
            Background = "Transparent";
        }

        public override void Render()
        {
            Console.WriteLine("[Spacer] Flex=" + FlexFactor);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusSizedBox - Fixed size box for spacing
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusSizedBox : ModuleUIElement
    {
        public NimbusSizedBox(string id) : base(id, "SizedBox")
        {
            Background = "Transparent";
        }

        public static NimbusSizedBox FixedWidth(string id, double width)
        {
            NimbusSizedBox box = new NimbusSizedBox(id);
            box.Width = width.ToString();
            box.Height = "0";
            return box;
        }

        public static NimbusSizedBox FixedHeight(string id, double height)
        {
            NimbusSizedBox box = new NimbusSizedBox(id);
            box.Width = "0";
            box.Height = height.ToString();
            return box;
        }

        public override void Render()
        {
            Console.WriteLine("[SizedBox] W=" + Width + " H=" + Height);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusCenter - Centers its child
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusCenter : ModuleUIElement
    {
        public NimbusCenter(string id) : base(id, "Center")
        {
            HorizontalAlignment = "Center";
            VerticalAlignment = "Center";
            Background = "Transparent";
        }

        public override void Render()
        {
            Console.WriteLine("[Center] Children=" + Children.Count);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  NimbusAspectRatio - Maintains aspect ratio
    // ═══════════════════════════════════════════════════════════════════
    public class NimbusAspectRatio : ModuleUIElement
    {
        public double Ratio { get; set; }  // Width / Height (e.g., 16.0/9.0 = 1.778)

        public NimbusAspectRatio(string id) : base(id, "AspectRatio")
        {
            Ratio = 1.0;
            Background = "Transparent";
        }

        public override void Render()
        {
            Console.WriteLine("[AspectRatio] Ratio=" + Ratio);
        }
    }
}
