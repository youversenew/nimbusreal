// Example: Mouse Events Handler for mouse-events-demo.xml
// Add this code to your Logic Runner or engine handler section

using Nimbus.WPF;

public class MouseEventsDemo
{
    private int leftClickCount = 0;
    private int rightClickCount = 0;

    // ════════════════════════════════════════════════════════════
    // LEFT CLICK HANDLERS
    // ════════════════════════════════════════════════════════════

    public void OnLeftClick(IUIModule module)
    {
        // Update UI element to show feedback
        var statusLabel = GetModuleById("leftClickStatus");
        if (statusLabel != null)
        {
            ((CustomUILabel)statusLabel).Text = "✅ Left click detected! Fired at: " + DateTime.Now.ToString("HH:mm:ss.fff");
        }
        
        LogInfo("Left Click Handler Executed");
    }

    public void OnDualLeftClick(IUIModule module)
    {
        leftClickCount++;
        
        var countLabel = GetModuleById("leftCount");
        if (countLabel != null)
        {
            ((CustomUILabel)countLabel).Text = leftClickCount.ToString();
        }
        
        LogInfo("Dual Left Click #" + leftClickCount);
    }

    // ════════════════════════════════════════════════════════════
    // RIGHT CLICK HANDLERS
    // ════════════════════════════════════════════════════════════

    public void OnRightClick(IUIModule module)
    {
        // Update UI element to show feedback
        var statusLabel = GetModuleById("rightClickStatus");
        if (statusLabel != null)
        {
            ((CustomUILabel)statusLabel).Text = "✅ Right click detected! Fired at: " + DateTime.Now.ToString("HH:mm:ss.fff");
        }
        
        LogInfo("Right Click Handler Executed");
    }

    public void OnDualRightClick(IUIModule module)
    {
        rightClickCount++;
        
        var countLabel = GetModuleById("rightCount");
        if (countLabel != null)
        {
            ((CustomUILabel)countLabel).Text = rightClickCount.ToString();
        }
        
        LogInfo("Dual Right Click #" + rightClickCount);
    }

    // ════════════════════════════════════════════════════════════
    // EVENT LISTENERS (Optional - Advanced Usage)
    // ════════════════════════════════════════════════════════════

    // Example: Attach event listeners programmatically to UI elements
    public void AttachEventListeners(IUIModule rootModule)
    {
        var leftButton = GetModuleById("leftClickButton");
        if (leftButton != null)
        {
            // Attach left click listener
            leftButton.AddEventListener("leftclick", (evt) =>
            {
                LogDebug("LeftClickEvent fired on " + leftButton.Id);
            });

            // Also listen for generic click
            leftButton.AddEventListener("click", (evt) =>
            {
                LogDebug("Click event fired on " + leftButton.Id);
            });
        }

        var rightButton = GetModuleById("rightClickButton");
        if (rightButton != null)
        {
            // Attach right click listener
            rightButton.AddEventListener("rightclick", (evt) =>
            {
                LogDebug("RightClickEvent fired on " + rightButton.Id);
                // You can access mouse data from the event
                if (evt is RightClickEvent)
                {
                    var rcEvt = (RightClickEvent)evt;
                    LogDebug("Mouse position: " + rcEvt.Mouse.X + ", " + rcEvt.Mouse.Y);
                }
            });
        }
    }

    // ════════════════════════════════════════════════════════════
    // XML EVENT ATTRIBUTES REFERENCE
    // ════════════════════════════════════════════════════════════

    /*
    
    XML SYNTAX:
    ===========

    1. LEFT CLICK (fires onClick attribute handler):
       <Button onClick="OnLeftClick" Text="Click Me"/>
       
    2. RIGHT CLICK (fires onRightClick attribute handler):
       <Button onRightClick="OnRightClick" Text="Right Click Me"/>
       
    3. BOTH EVENTS:
       <Button onClick="OnLeftClick" onRightClick="OnRightClick" Text="Click Either Way"/>

    EVENTS FIRED (in Event System):
    ===============================

    LEFT CLICK:
      - mousedown: fires on mouse button press
      - mouseup: fires on mouse button release
      - leftclick: fires when left mouse button completes
      - click: generic click event (backward compatibility)
      - onClick handler: executes the defined handler

    RIGHT CLICK:
      - rightclick: fires when right mouse button clicks
      - onRightClick handler: executes the defined handler
      - contextmenu: traditional context menu (text inputs)

    C# EVENT CLASSES:
    =================
    - MouseDownEvent: fires when button pressed
    - MouseUpEvent: fires when button released
    - LeftClickEvent: fires on left mouse button complete
    - RightClickEvent: fires on right mouse button
    - ClickEvent: generic click (backward compat)
    - ContextRequestEvent: context menu request

    */

    // Helper methods
    private IUIModule GetModuleById(string id)
    {
        // Implement this based on your UI structure
        // This should search the UI tree for an element with matching ID
        return null;
    }

    private void LogInfo(string message)
    {
        System.Console.WriteLine("[INFO] " + message);
    }

    private void LogDebug(string message)
    {
        System.Console.WriteLine("[DEBUG] " + message);
    }
}
