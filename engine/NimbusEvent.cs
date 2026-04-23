using System;
using System.Collections.Generic;

namespace Nimbus.WPF
{
    // ══════════════════════════════════════════════════════════════════════════════════
    // NIMBUS EVENT SYSTEM - Professional Architecture
    // Input → Event System → UI (NO direct input to UI)
    // Supports: Bubbling, Capturing, Event Delegation, Custom Events
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>Event phases in the event lifecycle</summary>
    public enum EventPhase
    {
        Capturing,   // Phase 1: parent → target (if capturing enabled)
        AtTarget,    // Phase 2: target itself
        Bubbling     // Phase 3: target → parent (default propagation)
    }

    /// <summary>Base event class - all events inherit from this</summary>
    public abstract class NimbusEvent
    {
        /// <summary>Event type identifier (e.g., "click", "press", "longpress")</summary>
        public string Type { get; set; }

        /// <summary>Element that originally triggered the event</summary>
        public IUIModule Target { get; set; }

        /// <summary>Element currently processing the event</summary>
        public IUIModule CurrentTarget { get; set; }

        /// <summary>Current phase of event propagation</summary>
        public EventPhase Phase { get; set; }

        /// <summary>Timestamp when event occurred</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Has preventDefault() been called?</summary>
        public bool DefaultPrevented { get; private set; }

        /// <summary>Has stopPropagation() been called?</summary>
        public bool ImmediatePropagationStopped { get; private set; }

        /// <summary>Custom data attached to event</summary>
        public Dictionary<string, object> Data { get; set; }

        protected NimbusEvent(string type, IUIModule target)
        {
            Type = type;
            Target = target;
            CurrentTarget = target;
            Phase = EventPhase.AtTarget;
            Timestamp = DateTime.Now;
            DefaultPrevented = false;
            ImmediatePropagationStopped = false;
            Data = new Dictionary<string, object>();
        }

        /// <summary>Prevent default action</summary>
        public void PreventDefault()
        {
            DefaultPrevented = true;
        }

        /// <summary>Stop propagation to parent elements</summary>
        public void StopPropagation()
        {
            ImmediatePropagationStopped = true;
        }

        public override string ToString()
        {
            return string.Format("[{0}] target={1} @ {2:HH:mm:ss.fff}", Type, Target != null ? Target.Id : "null", Timestamp);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // MOUSE EVENTS
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>Mouse position data (X, Y relative to canvas)</summary>
    public class MouseData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double ClientX { get; set; }  // Relative to element
        public double ClientY { get; set; }
        public int ClickCount { get; set; }  // 1 = single, 2 = double, etc.
        public bool LeftButton { get; set; }
        public bool RightButton { get; set; }
        public bool MiddleButton { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }

        public override string ToString()
        {
            return string.Format("Mouse({0:F0},{1:F0}) click={2} buttons={3}/{4}/{5}", X, Y, ClickCount, LeftButton, RightButton, MiddleButton);
        }
    }

    /// <summary>Mouse DOWN event - finger/pointer pressed</summary>
    public class MouseDownEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }

        public MouseDownEvent(IUIModule target, MouseData mouseData) : base("mousedown", target)
        {
            Mouse = mouseData ?? new MouseData();
        }
    }

    /// <summary>Mouse UP event - finger/pointer released</summary>
    public class MouseUpEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }

        public MouseUpEvent(IUIModule target, MouseData mouseData) : base("mouseup", target)
        {
            Mouse = mouseData ?? new MouseData();
        }
    }

    /// <summary>CLICK event - successful click (press + release on same element)</summary>
    public class ClickEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }
        public double Duration { get; set; }  // ms held down
        public string ControlName { get; set; }  // Backward compatibility
        public string ControlType { get; set; }   // Backward compatibility

        public ClickEvent() : base("click", null)
        {
            Mouse = new MouseData();
            Duration = 0;
            ControlName = "";
            ControlType = "";
        }

        public ClickEvent(IUIModule target, MouseData mouseData, double durationMs = 0) : base("click", target)
        {
            Mouse = mouseData ?? new MouseData();
            Duration = durationMs;
            ControlName = "";
            ControlType = "";
        }
    }

    /// <summary>DOUBLE CLICK event</summary>
    public class DoubleClickEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }

        public DoubleClickEvent(IUIModule target, MouseData mouseData) : base("dblclick", target)
        {
            Mouse = mouseData ?? new MouseData();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // PRESS & HOLD EVENTS (Native-like)
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>PRESS event - key/button DOWN, NOT yet released</summary>
    public class PressEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }
        public double ElapsedMs { get; set; }  // How long being pressed

        public PressEvent(IUIModule target, MouseData mouseData) : base("press", target)
        {
            Mouse = mouseData ?? new MouseData();
            ElapsedMs = 0;
        }
    }

    /// <summary>RELEASE event - key/button released</summary>
    public class ReleaseEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }
        public double HeldDurationMs { get; set; }  // Total time held down

        public ReleaseEvent(IUIModule target, MouseData mouseData, double heldMs = 0) : base("release", target)
        {
            Mouse = mouseData ?? new MouseData();
            HeldDurationMs = heldMs;
        }
    }

    /// <summary>LONG PRESS event - held for X milliseconds (default 500ms)</summary>
    public class LongPressEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }
        public double ThresholdMs { get; set; }  // How long before it fires
        public double ElapsedMs { get; set; }

        public LongPressEvent(IUIModule target, MouseData mouseData, double thresholdMs = 500) : base("longpress", target)
        {
            Mouse = mouseData ?? new MouseData();
            ThresholdMs = thresholdMs;
            ElapsedMs = 0;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // GESTURE EVENTS (Touch/Swipe)
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>GESTURE event - swipe, pinch, rotate, etc.</summary>
    public class GestureEvent : NimbusEvent
    {
        public enum GestureType
        {
            Swipe,          // Left, Right, Up, Down
            Pinch,          // 2-finger pinch zoom
            Rotate,         // 2-finger rotation
            DoubleTap,      // Quick 2x tap
            LongPress       // Hold > 500ms (alternative to LongPressEvent)
        }

        public GestureType GestureKind { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double Velocity { get; set; }  // pixels/ms
        public double Scale { get; set; }     // pinch zoom factor
        public double Rotation { get; set; }  // degrees

        public GestureEvent(IUIModule target, GestureType kind) : base("gesture", target)
        {
            GestureKind = kind;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // CONTEXT MENU EVENT
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>CONTEXT REQUEST event - user wants context menu (right-click)</summary>
    public class ContextRequestEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }
        public NimbusContextMenuDef CustomMenu { get; set; }  // Custom menu definition

        public ContextRequestEvent(IUIModule target, MouseData mouseData) : base("contextrequest", target)
        {
            Mouse = mouseData ?? new MouseData();
            CustomMenu = null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // FOCUS EVENTS
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>FOCUS event - element received focus</summary>
    public class FocusEvent : NimbusEvent
    {
        public IUIModule RelatedTarget { get; set; }  // Previous focused element

        public FocusEvent(IUIModule target, IUIModule relatedTarget = null) : base("focus", target)
        {
            RelatedTarget = relatedTarget;
        }
    }

    /// <summary>BLUR event - element lost focus</summary>
    public class BlurEvent : NimbusEvent
    {
        public IUIModule RelatedTarget { get; set; }  // New focused element

        public BlurEvent(IUIModule target, IUIModule relatedTarget = null) : base("blur", target)
        {
            RelatedTarget = relatedTarget;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // HOVER EVENTS
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>MOUSEENTER event - cursor entered element</summary>
    public class MouseEnterEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }

        public MouseEnterEvent(IUIModule target, MouseData mouseData) : base("mouseenter", target)
        {
            Mouse = mouseData ?? new MouseData();
        }
    }

    /// <summary>MOUSELEAVE event - cursor left element</summary>
    public class MouseLeaveEvent : NimbusEvent
    {
        public MouseData Mouse { get; set; }

        public MouseLeaveEvent(IUIModule target, MouseData mouseData) : base("mouseleave", target)
        {
            Mouse = mouseData ?? new MouseData();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════
    // EVENT LISTENER DELEGATE
    // ══════════════════════════════════════════════════════════════════════════════════

    /// <summary>Callback for event listeners</summary>
    public delegate void EventListener(NimbusEvent evt);
}
