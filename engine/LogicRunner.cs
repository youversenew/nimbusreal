using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Xml;
using System.Text;
using System.Windows.Input;       // Cursors, MouseEventArgs uchun
using System.Windows.Threading;   // DispatcherTimer uchun
using System.Windows.Shapes;      // Rectangle, Path uchun (agar kerak bo'lsa)
namespace Nimbus.WPF
{
    /// <summary>
    /// Logic Runner - Complete implementation for Nimbus Framework
    /// </summary>
    public partial class LogicRunner
    {
        private WpfEngine _engine;
        
        public LogicRunner(WpfEngine engine)
        {
            _engine = engine;
        }
        
        /// <summary>
        /// Execute logic node
        /// </summary>
        public void Execute(XmlNode logicNode, object sender)
        {
            Execute(logicNode, sender, null);
        }

        /// <summary>
        /// Execute logic node with event context
        /// </summary>
        public void Execute(XmlNode logicNode, object sender, NimbusEvent evt)
        {
            if (logicNode == null) return;
            
            // Store event in engine state if provided
            if (evt != null)
            {
                _engine.SetVariable("__event__", evt);
                _engine.SetVariable("__eventType__", evt.Type);
            }
            
            foreach (XmlNode child in logicNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                ExecuteCommand(child, sender);
            }

            // Clean up event context
            if (evt != null)
            {
                _engine.SetVariable("__event__", null);
                _engine.SetVariable("__eventType__", null);
            }
        }
        
        private void ExecuteCommand(XmlNode child, object sender)
{
    string command = child.Name.ToLower();
    if (command == "else" || command == "then")
        return;
    switch (command)
    {
        // Basic commands
        case "set":
            ExecuteSet(child);
            break;
        //end
        // ExecuteCommand switch ichiga YANGI caslar:

case "zoomin":
    ExecuteZoomIn(child);
    break;

case "zoomout":
    ExecuteZoomOut(child);
    break;

case "importmanualc":
case "importcode":
case "loadmanualc":
    ExecuteImportManualC(child);
    break;
        case "timer":
            ExecuteTimer(child);
            break;
        //bilmadim
        case "get":
            ExecuteGet(child);
            break;
        //end
        case "if":
            ExecuteIf(child, sender);
            break;
        //end
        case "switch":
            ExecuteSwitch(child, sender);
            break;
        //bilmadim
        case "show":
            ExecuteShow(child);
            break;

        //end
        case "hide":
            ExecuteHide(child);
            break;
        //end
        case "toggle":
            ExecuteToggle(child);
            break;
        //end
        case "enable":
            ExecuteEnable(child);
            break;
        //end
        case "disable":
            ExecuteDisable(child);
            break;

        //end
        case "focus":
            ExecuteFocus(child);
            break;
        //end
        case "alert":
        case "messagebox":
            ExecuteAlert(child);
            break;
        //end
        case "confirm":
            ExecuteConfirm(child, sender);
            break;
        //end
        case "close":
            ExecuteClose(child);
            break;
        //end
        case "exit":
            ExecuteExit();
            break;
        //end

        // Math operations
        case "increment":
            ExecuteIncrement(child);
            break;
        //end
        case "decrement":
            ExecuteDecrement(child);
            break;
        //end
        case "multiply":
            ExecuteMultiply(child);
            break;
        //end
        case "divide":
            ExecuteDivide(child);
            break;
        //end
        case "calculate":
            ExecuteCalculate(child);
            break;
        //end

        // Text commands
        case "appendtext":
            ExecuteAppendText(child);
            break;
        //end
        case "cleartext":
            ExecuteClearText(child);
            break;
        //end

        // Item commands
        case "addmessage":
            ExecuteAddMessage(child);
            break;
        //end
        case "additem":
            ExecuteAddItem(child);
            break;
        //end
        case "removeitem":
            ExecuteRemoveItem(child);
            break;
        //end
        case "clearitems":
            ExecuteClearItems(child);
            break;
        //end
            
        // Effects
        case "shadow":
        case "dropshadow":
            ExecuteDropShadow(child);
            break;
        //end
        case "blur":
        case "blureffect":
            ExecuteBlurEffect(child);
            break;
        //end
        case "opacity":
            ExecuteOpacity(child);
            break;
        //end
        case "glow":
            ExecuteGlow(child);
            break;
        //end
        case "cleareffect":
        case "removeeffect":
            ExecuteClearEffect(child);
            break;
        case "animate":
            ExecuteAnimate(child);
            break;
        //end

        // Scroll
        case "scrolltobottom":
            ExecuteScrollToBottom(child);
            break;
        //end
        case "scrolltotop":
            ExecuteScrollToTop(child);
            break;
        //end

        // HTTP
        case "httprequest":
        case "http":
        case "fetch":
            ExecuteHttpRequest(child);
            break;
        //end

        // File
        case "savefile":
            ExecuteSaveFile(child);
            break;
        //end
        case "openfile":
            ExecuteOpenFile(child);
            break;
        //end

        // Clipboard
        case "copy":
            ExecuteCopy(child);
            break;
        //end
        case "paste":
            ExecutePaste(child);
            break;
        //end

        // Call handler
        case "call":
            ExecuteCall(child, sender);
            break;
        //end

        // Delay
        case "delay":
        case "wait":
            ExecuteDelay(child);
            break;
        //end

        // Debug
        case "print":
            ExecutePrint(child);
            break;
        //end
        case "debug":
            ExecuteDebug(child);
            break;
        //end
        // ExecuteCommand switch ichiga qo'shing:

case "modal":
case "showmodal":
    ExecuteModal(child, sender);
    break;

case "drawer":
case "showdrawer":
    ExecuteDrawer(child, sender);
    break;

case "toast":
case "showtoast":
case "notification":
    ExecuteToast(child);
    break;

case "bottomsheet":
case "showbottomsheet":
    ExecuteBottomSheet(child, sender);
    break;

case "contextmenu":
case "showcontextmenu":
case "menu":
    ExecuteContextMenu(child, sender);
    break;

case "closemodal":
case "closepopup":
    ExecuteClosePopup(child);
    break;
        // LogicRunner.cs da ExecuteCommand ichiga qo'shing:
        case "newwindow":
        case "openwindow":
            ExecuteNewWindow(child);
            break;
        //tuzatish kerak

// YANGI case lar:
        case "callmanualc":
        case "callcsharp":
        case "callcode":
            ExecuteCallManualC(child, sender);
            break;
        //end

        case "plugin":
        case "useplugin":
        case "callplugin":
            ExecutePlugin(child, sender);
            break;
        //end

        case "foreach":
        case "loop":
            ExecuteForEach(child, sender);
            break;
        //bilmadim

        case "while":
            ExecuteWhile(child, sender);
            break;
        //bilmagandekman

        case "try":
        case "trycatch":
            ExecuteTryCatch(child, sender);
            break;
        //bilmadim

        case "setproperty":
        case "setprop":
            ExecuteSetProperty(child);
            break;

        case "getproperty":
        case "getprop":
            ExecuteGetProperty(child);
            break;

        case "concat":
            ExecuteConcat(child);
            break;

        case "substring":
            ExecuteSubstring(child);
            break;

        case "replace":
            ExecuteReplace(child);
            break;

        case "log":
            ExecuteLog(child);
            break;
        //ishlamadiku

// LogicRunner.cs metodlari:


        // Custom / Plugin Commands
        default:
            bool handled = _engine.TryExecuteCustomCommand(child.Name, child, sender);
            if (!handled)
            {
                // Try case-insensitive
                handled = _engine.TryExecuteCustomCommand(command, child, sender);
            }
            if (!handled)
            {
                _engine.Log("WARN", "Unknown command: " + child.Name);
            }
            break;
    }
}
private void ExecuteZoomIn(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    string scaleStr = GetAttribute(node, "Scale", "1.2");
    string durationStr = GetAttribute(node, "Duration", "200");

    double scale = 1.2;
    double.TryParse(scaleStr, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out scale);

    int duration = 200;
    int.TryParse(durationStr, out duration);

    FrameworkElement control = _engine.GetControl(controlName);
    if (control == null) return;

    ScaleTransform scaleTransform = control.RenderTransform as ScaleTransform;
    if (scaleTransform == null)
    {
        scaleTransform = new ScaleTransform(1, 1);
        control.RenderTransform = scaleTransform;
        control.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    double currentScale = scaleTransform.ScaleX;

    DoubleAnimation anim = new DoubleAnimation(currentScale, scale, TimeSpan.FromMilliseconds(duration));
    anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
}
// LogicRunner.cs da ExecuteImportManualC ni ALMASHTIRING:

private void ExecuteImportManualC(XmlNode node)
{
    string source = GetAttribute(node, "Source", "");
    string id = GetAttribute(node, "Id", "");
    if (string.IsNullOrEmpty(id)) id = GetAttribute(node, "Name", "");

    if (string.IsNullOrEmpty(source))
    {
        _engine.Log("ERROR", "ImportManualC: Source attribute required");
        return;
    }

    string fullPath = source;
    if (!System.IO.Path.IsPathRooted(source) && !string.IsNullOrEmpty(_engine.CurrentXmlPath))
    {
        string dir = System.IO.Path.GetDirectoryName(_engine.CurrentXmlPath);
        fullPath = System.IO.Path.Combine(dir, source);
    }

    if (!System.IO.File.Exists(fullPath))
    {
        _engine.Log("ERROR", "ImportManualC: File not found: " + fullPath);
        return;
    }

    try
    {
        string content = System.IO.File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
        string code = "";
        string moduleId = id;

        if (fullPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            XmlNode manualCNode = doc.SelectSingleNode("//ManualC");
            if (manualCNode == null) manualCNode = doc.SelectSingleNode("//CSharp");
            if (manualCNode == null) manualCNode = doc.SelectSingleNode("//Code");

            if (manualCNode != null)
            {
                if (string.IsNullOrEmpty(moduleId))
                {
                    moduleId = GetAttribute(manualCNode, "id", "");
                    if (string.IsNullOrEmpty(moduleId)) moduleId = GetAttribute(manualCNode, "Id", "");
                    if (string.IsNullOrEmpty(moduleId)) moduleId = GetAttribute(manualCNode, "Name", "");
                    if (string.IsNullOrEmpty(moduleId)) moduleId = "ImportedModule";
                }

                // To'g'ridan-to'g'ri XmlNode ni compiler ga berish
                CSharpCompiler compiler = _engine.GetCompiler();
                if (compiler != null)
                {
                    // id attributini o'rnatish (agar yo'q bo'lsa)
                    if (manualCNode.Attributes["id"] == null)
                    {
                        XmlAttribute idAttr = manualCNode.OwnerDocument.CreateAttribute("id");
                        idAttr.Value = moduleId;
                        manualCNode.Attributes.Append(idAttr);
                    }

                    bool success = compiler.Compile(manualCNode);
                    if (success)
                    {
                        _engine.Log("IMPORT", "ManualC imported: " + moduleId + " from " + source);
                    }
                    else
                    {
                        _engine.Log("ERROR", "ManualC compilation failed: " + moduleId);
                    }
                }
            }
            else
            {
                _engine.Log("ERROR", "ImportManualC: No ManualC node in " + source);
            }
        }
        else if (fullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(moduleId))
            {
                moduleId = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            }

            // .cs fayl uchun XmlNode yaratish
            XmlDocument tempDoc = new XmlDocument();
            XmlElement tempNode = tempDoc.CreateElement("ManualC");
            tempNode.SetAttribute("id", moduleId);
            tempNode.InnerText = content;

            CSharpCompiler compiler = _engine.GetCompiler();
            if (compiler != null)
            {
                bool success = compiler.Compile(tempNode);
                if (success)
                {
                    _engine.Log("IMPORT", "C# file imported: " + moduleId + " from " + source);
                }
                else
                {
                    _engine.Log("ERROR", "C# compilation failed: " + moduleId);
                }
            }
        }
        else
        {
            _engine.Log("ERROR", "ImportManualC: Unsupported type: " + source);
        }
    }
    catch (Exception ex)
    {
        _engine.Log("ERROR", "ImportManualC failed: " + ex.Message);
    }
}

private void ExecuteZoomOut(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    string scaleStr = GetAttribute(node, "Scale", "1.0");
    string durationStr = GetAttribute(node, "Duration", "200");

    double scale = 1.0;
    double.TryParse(scaleStr, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out scale);

    int duration = 200;
    int.TryParse(durationStr, out duration);

    FrameworkElement control = _engine.GetControl(controlName);
    if (control == null) return;

    ScaleTransform scaleTransform = control.RenderTransform as ScaleTransform;
    if (scaleTransform == null)
    {
        scaleTransform = new ScaleTransform(1, 1);
        control.RenderTransform = scaleTransform;
        control.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    double currentScale = scaleTransform.ScaleX;

    DoubleAnimation anim = new DoubleAnimation(currentScale, scale, TimeSpan.FromMilliseconds(duration));
    anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
}

        
        #region Math Operations
private void ExecuteForEach(XmlNode node, object sender)
{
    string collection = GetAttribute(node, "Collection", "");
    string varName = GetAttribute(node, "Var", "item");
    string indexVar = GetAttribute(node, "IndexVar", "_index");
    string countStr = GetAttribute(node, "Count", "");

    // Count-based loop
    if (!string.IsNullOrEmpty(countStr))
    {
        countStr = ReplaceStateVariables(countStr);
        int count = 0;
        int.TryParse(countStr, out count);

        for (int i = 0; i < count; i++)
        {
            _engine.SetVariable(indexVar, i);
            _engine.SetVariable(varName, i);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    ExecuteCommand(child, sender);
                }
            }
        }
        return;
    }

    // Collection-based loop
    collection = ReplaceStateVariables(collection);

    if (string.IsNullOrEmpty(collection)) return;

    string[] items = collection.Split(',');
    for (int i = 0; i < items.Length; i++)
    {
        _engine.SetVariable(varName, items[i].Trim());
        _engine.SetVariable(indexVar, i);

        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                ExecuteCommand(child, sender);
            }
        }
    }
}
private void ExecuteWhile(XmlNode node, object sender)
{
    string condition = GetAttribute(node, "Condition", "");
    int maxIterations = 1000000000; // Infinite loop himoya
    int iteration = 0;

    while (EvaluateCondition(condition) && iteration < maxIterations)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                ExecuteCommand(child, sender);
            }
        }
        iteration++;
        // Condition ni qayta evaluate qilish uchun
        // (chunki state o'zgargan bo'lishi mumkin)
    }

    if (iteration >= maxIterations)
    {
        _engine.Log("WARN", "While loop reached max iterations: " + maxIterations);
    }
}
private void ExecuteTryCatch(XmlNode node, object sender)
{
    string errorVar = GetAttribute(node, "ErrorVar", "_error");

    XmlNode tryNode = node.SelectSingleNode("Try");
    XmlNode catchNode = node.SelectSingleNode("Catch");
    XmlNode finallyNode = node.SelectSingleNode("Finally");

    // Agar Try ichki node bo'lmasa, to'g'ridan-to'g'ri bolalar = try body
    if (tryNode == null)
    {
        tryNode = node;
    }

    try
    {
        foreach (XmlNode child in tryNode.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element &&
                child.Name != "Catch" && child.Name != "Finally")
            {
                ExecuteCommand(child, sender);
            }
        }
    }
    catch (Exception ex)
    {
        _engine.SetVariable(errorVar, ex.Message);
        _engine.Log("ERROR", "TryCatch caught: " + ex.Message);

        if (catchNode != null)
        {
            Execute(catchNode, sender);
        }
    }
    finally
    {
        if (finallyNode != null)
        {
            Execute(finallyNode, sender);
        }
    }
}
private void ExecuteConcat(XmlNode node)
{
    string var1 = GetAttribute(node, "A", "");
    string var2 = GetAttribute(node, "B", "");
    string separator = GetAttribute(node, "Separator", "");
    string toState = GetAttribute(node, "ToState", "");

    var1 = ReplaceStateVariables(var1);
    var2 = ReplaceStateVariables(var2);

    string result = var1 + separator + var2;

    if (!string.IsNullOrEmpty(toState))
    {
        _engine.SetVariable(toState, result);
    }
}

private void ExecuteSubstring(XmlNode node)
{
    string source = GetAttribute(node, "Source", "");
    string startStr = GetAttribute(node, "Start", "0");
    string lengthStr = GetAttribute(node, "Length", "");
    string toState = GetAttribute(node, "ToState", "");

    source = ReplaceStateVariables(source);

    int start = 0;
    int.TryParse(startStr, out start);

    string result;
    if (!string.IsNullOrEmpty(lengthStr))
    {
        int length = 0;
        int.TryParse(lengthStr, out length);
        result = source.Length >= start + length ? source.Substring(start, length) : source;
    }
    else
    {
        result = source.Length > start ? source.Substring(start) : "";
    }

    if (!string.IsNullOrEmpty(toState))
    {
        _engine.SetVariable(toState, result);
    }
}

private void ExecuteReplace(XmlNode node)
{
    string source = GetAttribute(node, "Source", "");
    string find = GetAttribute(node, "Find", "");
    string replaceWith = GetAttribute(node, "With", "");
    string toState = GetAttribute(node, "ToState", "");

    source = ReplaceStateVariables(source);
    find = ReplaceStateVariables(find);
    replaceWith = ReplaceStateVariables(replaceWith);

    string result = source.Replace(find, replaceWith);

    if (!string.IsNullOrEmpty(toState))
    {
        _engine.SetVariable(toState, result);
    }
}

private void ExecuteLog(XmlNode node)
{
    string message = GetAttribute(node, "Message", node.InnerText);
    string level = GetAttribute(node, "Level", "INFO");

    message = ReplaceStateVariables(message);
    message = ParseControlProperty(message);

    _engine.Log(level.ToUpper(), message);
}
private void ExecuteSetProperty(XmlNode node)
{
    string control = GetAttribute(node, "Control", "");
    string property = GetAttribute(node, "Property", "");
    string value = GetAttribute(node, "Value", "");

    value = ReplaceStateVariables(value);
    value = ParseControlProperty(value);

    if (!string.IsNullOrEmpty(control) && !string.IsNullOrEmpty(property))
    {
        _engine.SetControlProperty(control, property, value);
    }
}

private void ExecuteGetProperty(XmlNode node)
{
    string control = GetAttribute(node, "Control", "");
    string property = GetAttribute(node, "Property", "");
    string toState = GetAttribute(node, "ToState", "");

    if (!string.IsNullOrEmpty(control) && !string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(toState))
    {
        object val = _engine.GetControlProperty(control, property);
        _engine.SetVariable(toState, val != null ? val.ToString() : "");
    }
}
private void ExecutePlugin(XmlNode node, object sender)
{
    string pluginName = GetAttribute(node, "Name", "");
    string method = GetAttribute(node, "Method", "");
    string toState = GetAttribute(node, "ToState", "");
    string toControl = GetAttribute(node, "ToControl", "");
    string toProperty = GetAttribute(node, "ToProperty", "Text");

    if (string.IsNullOrEmpty(pluginName))
    {
        _engine.Log("ERROR", "Plugin: Name attribute required");
        return;
    }

    // Lazy load
    bool loaded = _engine.EnsureNimbusPlugin(pluginName);
    if (!loaded)
    {
        _engine.Log("ERROR", "Plugin not found: " + pluginName);
        return;
    }

    if (string.IsNullOrEmpty(method))
    {
        // Just loading plugin, no method call
        return;
    }

    // Collect params
    string paramsStr = GetAttribute(node, "Params", "");

    // Check child elements for Params
    if (string.IsNullOrEmpty(paramsStr))
    {
        XmlNode paramsNode = node.SelectSingleNode("Params");
        if (paramsNode != null)
        {
            paramsStr = paramsNode.InnerText.Trim();
        }
    }

    // Check ToState from child element
    if (string.IsNullOrEmpty(toState))
    {
        XmlNode toStateNode = node.SelectSingleNode("ToState");
        if (toStateNode != null)
        {
            toState = toStateNode.InnerText.Trim();
        }
    }

    paramsStr = ReplaceStateVariables(paramsStr);

    _engine.Log("PLUGIN", "Calling: " + pluginName + "." + method + "(" + paramsStr + ")");

    // Try function call with different key formats
    string result = null;

    // Try: pluginname.method
    string key1 = pluginName.ToLower() + "." + method.ToLower();
    result = _engine.TryExecuteCustomFunction(key1, paramsStr);

    // Try: method alone
    if (result == null)
    {
        result = _engine.TryExecuteCustomFunction(method.ToLower(), paramsStr);
    }

    // Try: plugin shortname.method (e.g., math.abs)
    if (result == null)
    {
        string shortName = pluginName.Replace("Plugin", "").ToLower();
        string key2 = shortName + "." + method.ToLower();
        result = _engine.TryExecuteCustomFunction(key2, paramsStr);
    }

    if (result != null)
    {
        _engine.Log("PLUGIN", "Result: " + result);

        if (!string.IsNullOrEmpty(toState))
        {
            _engine.SetVariable(toState, result);
        }
        if (!string.IsNullOrEmpty(toControl))
        {
            _engine.SetControlProperty(toControl, toProperty, result);
        }
    }
    else
    {
        _engine.Log("WARN", "Plugin function not found: " + pluginName + "." + method);

        // Try as command
        bool handled = _engine.TryExecuteCustomCommand(key1, node, sender);
        if (!handled)
        {
            _engine.Log("ERROR", "Plugin command also not found: " + key1);
        }
    }
}
        private void ExecuteCallManualC(XmlNode node, object sender)
{
    string id = GetAttribute(node, "id", "");
    if (string.IsNullOrEmpty(id)) id = GetAttribute(node, "Id", "");
    if (string.IsNullOrEmpty(id)) id = GetAttribute(node, "ID", "");

    string method = GetAttribute(node, "Method", "");
    if (string.IsNullOrEmpty(method)) method = GetAttribute(node, "method", "");

    string toState = GetAttribute(node, "ToState", "");
    if (string.IsNullOrEmpty(toState)) toState = GetAttribute(node, "toState", "");
    if (string.IsNullOrEmpty(toState)) toState = GetAttribute(node, "tostate", "");

    string toControl = GetAttribute(node, "ToControl", "");
    if (string.IsNullOrEmpty(toControl)) toControl = GetAttribute(node, "tocontrol", "");

    string toProperty = GetAttribute(node, "ToProperty", "");
    if (string.IsNullOrEmpty(toProperty)) toProperty = GetAttribute(node, "Property", "Text");

    if (string.IsNullOrEmpty(id))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[EXECUTION ERROR] CallManualC: 'id' attribute missing.");
        Console.ResetColor();
        return;
    }

    if (string.IsNullOrEmpty(toState))
    {
        XmlNode toStateNode = node.SelectSingleNode("ToState");
        if (toStateNode != null && toStateNode.InnerText != null)
        {
            toState = toStateNode.InnerText.Trim();
        }
    }

    List<object> paramList = new List<object>();
    if (node.ChildNodes != null)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element &&
                (child.Name == "Param" || child.Name == "Parameter" || child.Name == "Arg"))
            {
                string paramValue = GetAttribute(child, "Value", "");
                if (string.IsNullOrEmpty(paramValue) && child.InnerText != null)
                    paramValue = child.InnerText.Trim();

                paramValue = ReplaceStateVariables(paramValue);
                paramValue = ParseControlProperty(paramValue);
                paramList.Add(paramValue);
            }
        }
    }

    string paramsAttr = GetAttribute(node, "Params", "");
    if (!string.IsNullOrEmpty(paramsAttr) && paramList.Count == 0)
    {
        paramsAttr = ReplaceStateVariables(paramsAttr);
        paramsAttr = ParseControlProperty(paramsAttr);
        string[] separators = new string[] { "," };
        string[] parts = paramsAttr.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            int intVal;
            if (int.TryParse(trimmed, out intVal))
            {
                paramList.Add(intVal);
            }
            else
            {
                double dblVal;
                if (double.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out dblVal))
                {
                    paramList.Add(dblVal);
                }
                else
                {
                    paramList.Add(trimmed);
                }
            }
        }
    }

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(string.Format("\n[ACTION] ManualC -> ID: '{0}', Method: '{1}'", id, method));
    for (int i = 0; i < paramList.Count; i++)
    {
        string pVal = paramList[i] != null ? paramList[i].ToString() : "NULL";
        Console.WriteLine(string.Format("         Param[{0}] = '{1}'", i, pVal));
    }
    Console.ResetColor();

    try
    {
        CSharpCompiler compiler = _engine.GetCompiler();
        if (compiler == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] Compiler is null.");
            Console.ResetColor();
            return;
        }

        if (!compiler.HasModule(id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Format("[ERROR] Module '{0}' not found!", id));
            Console.ResetColor();
            return;
        }

        object result = compiler.ExecuteMethod(id, method, paramList.ToArray());

        string resultTypeName = result != null ? result.GetType().Name : "null";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(string.Format("[RESULT] Type: {0}", resultTypeName));
        Console.ResetColor();

        // STATE ga saqlash
        if (!string.IsNullOrEmpty(toState))
        {
            if (result != null)
                _engine.SetVariable(toState, result);
            else
                _engine.SetVariable(toState, "");
        }

        // UI CONTROL ga apply
        if (!string.IsNullOrEmpty(toControl))
        {
            bool handled = false;

            // 1) ImageSource (BitmapImage)
            if (!handled && result is System.Windows.Media.ImageSource)
            {
                _engine.GetMainWindow().Dispatcher.Invoke(new Action(delegate
                {
                    FrameworkElement ctrl = _engine.GetControl(toControl);
                    if (ctrl is System.Windows.Controls.Image)
                    {
                        ((System.Windows.Controls.Image)ctrl).Source =
                            (System.Windows.Media.ImageSource)result;
                    }
                    else if (ctrl != null)
                    {
                        System.Windows.Controls.Image childImg = FindChildImage(ctrl);
                        if (childImg != null)
                            childImg.Source = (System.Windows.Media.ImageSource)result;
                    }
                }));
                handled = true;
            }

            // 2) UIElement (dinamik yaratilgan control)
            if (!handled && result is UIElement)
            {
                _engine.GetMainWindow().Dispatcher.Invoke(new Action(delegate
                {
                    FrameworkElement container = _engine.GetControl(toControl);

                    UIElement newElement = (UIElement)result;

                    // Click + LongPress handler qo'shish
                    if (newElement is Border)
                    {
                        Border cellBorder = (Border)newElement;
                        AttachPhotoCellHandlers(cellBorder);
                    }

                    if (container is Panel)
                    {
                        ((Panel)container).Children.Add(newElement);
                    }
                    else if (container is Border)
                    {
                        ((Border)container).Child = newElement;
                    }
                    else if (container is ContentControl)
                    {
                        ((ContentControl)container).Content = newElement;
                    }
                }));
                handled = true;
            }

            // 3) String
            if (!handled)
            {
                string resultStr = result != null ? result.ToString() : "";
                _engine.SetControlProperty(toControl, toProperty, resultStr);
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format("[EXCEPTION] {0}.{1}: {2}", id, method, ex.Message));
        if (ex.InnerException != null)
            Console.WriteLine(string.Format("    Inner: {0}", ex.InnerException.Message));
        Console.ResetColor();
    }
}
private void AttachPhotoCellHandlers(Border cellBorder)
{
    if (cellBorder == null || cellBorder.Tag == null) return;

    DispatcherTimer longPressTimer = null;
    bool isLongPress = false;
    DateTime mouseDownTime = DateTime.MinValue;

    // Select overlay
    Border selectOverlay = new Border();
    selectOverlay.Background = new SolidColorBrush(Color.FromArgb(100, 0, 122, 255));
    selectOverlay.HorizontalAlignment = HorizontalAlignment.Stretch;
    selectOverlay.VerticalAlignment = VerticalAlignment.Stretch;
    selectOverlay.Visibility = Visibility.Collapsed;

    TextBlock checkMark = new TextBlock();
    checkMark.Text = "✓";
    checkMark.FontSize = 20;
    checkMark.FontWeight = FontWeights.Bold;
    checkMark.Foreground = Brushes.White;
    checkMark.HorizontalAlignment = HorizontalAlignment.Right;
    checkMark.VerticalAlignment = VerticalAlignment.Bottom;
    checkMark.Margin = new Thickness(0, 0, 6, 4);
    selectOverlay.Child = checkMark;

    UIElement existingChild = cellBorder.Child;
    Grid wrapGrid = new Grid();
    if (existingChild != null)
    {
        cellBorder.Child = null;
        wrapGrid.Children.Add(existingChild);
    }
    wrapGrid.Children.Add(selectOverlay);
    cellBorder.Child = wrapGrid;
    cellBorder.DataContext = false;

    // RenderTransform tayyorlash
    ScaleTransform scaleTransform = new ScaleTransform(1, 1);
    cellBorder.RenderTransform = scaleTransform;
    cellBorder.RenderTransformOrigin = new Point(0.5, 0.5);

    // LEFT MOUSE DOWN
    cellBorder.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
    {
        mouseDownTime = DateTime.Now;
        isLongPress = false;

        // Press scale effect
        DoubleAnimation scaleDown = new DoubleAnimation(1.0, 0.95, TimeSpan.FromMilliseconds(100));
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDown);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDown);

        // Long press timer
        longPressTimer = new DispatcherTimer();
        longPressTimer.Interval = TimeSpan.FromMilliseconds(500);
        longPressTimer.Tick += delegate
        {
            longPressTimer.Stop();
            longPressTimer = null;
            isLongPress = true;

            int idx = 0;
            if (cellBorder.Tag is int) idx = (int)cellBorder.Tag;
            else int.TryParse(cellBorder.Tag.ToString(), out idx);

            _engine.SetVariable("selectedIndex", idx);
            ShowPhotoContextMenu(cellBorder, idx, false);
        };
        longPressTimer.Start();
    };

    // LEFT MOUSE UP
    cellBorder.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
    {
        if (longPressTimer != null)
        {
            longPressTimer.Stop();
            longPressTimer = null;
        }

        // Scale back
        DoubleAnimation scaleUp = new DoubleAnimation(0.95, 1.0, TimeSpan.FromMilliseconds(150));
        scaleUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

        if (isLongPress) return;

        int photoIndex = 0;
        if (cellBorder.Tag is int) photoIndex = (int)cellBorder.Tag;
        else int.TryParse(cellBorder.Tag.ToString(), out photoIndex);

        object selectModeVal = _engine.GetVariable("selectMode");
        bool isSelectMode = false;
        if (selectModeVal != null)
        {
            string sv = selectModeVal.ToString().ToLower();
            isSelectMode = (sv == "true" || sv == "1");
        }

        if (isSelectMode)
        {
            bool currentlySelected = false;
            if (cellBorder.DataContext is bool)
                currentlySelected = (bool)cellBorder.DataContext;

            currentlySelected = !currentlySelected;
            cellBorder.DataContext = currentlySelected;

            if (currentlySelected)
            {
                selectOverlay.Visibility = Visibility.Visible;
                DoubleAnimation bounceIn = new DoubleAnimation(1.0, 0.92, TimeSpan.FromMilliseconds(100));
                bounceIn.AutoReverse = true;
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, bounceIn);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, bounceIn);
                _engine.ExecuteHandler("OnPhotoSelected", cellBorder);
            }
            else
            {
                selectOverlay.Visibility = Visibility.Collapsed;
                _engine.ExecuteHandler("OnPhotoDeselected", cellBorder);
            }
        }
        else
        {
            _engine.SetVariable("selectedIndex", photoIndex);
            _engine.ExecuteHandler("ShowViewer", cellBorder);
        }
    };

    // RIGHT MOUSE (Context Menu)
    cellBorder.MouseRightButtonUp += delegate(object s, MouseButtonEventArgs e)
    {
        int idx = 0;
        if (cellBorder.Tag is int) idx = (int)cellBorder.Tag;
        else int.TryParse(cellBorder.Tag.ToString(), out idx);

        _engine.SetVariable("selectedIndex", idx);
        ShowPhotoContextMenu(cellBorder, idx, true);
        e.Handled = true;
    };

    // MOUSE LEAVE
    cellBorder.MouseLeave += delegate
    {
        if (longPressTimer != null)
        {
            longPressTimer.Stop();
            longPressTimer = null;
        }
        DoubleAnimation scaleReset = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleReset);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleReset);
    };
}
private void ShowPhotoContextMenu(FrameworkElement sender, int photoIndex, bool isRightClick)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    // Get image info
    object pathObj = null;
    object nameObj = null;
    
    CSharpCompiler compiler = _engine.GetCompiler();
    if (compiler != null && compiler.HasModule("GalleryHelper"))
    {
        pathObj = compiler.ExecuteMethod("GalleryHelper", "GetImagePath", new object[] { photoIndex });
        nameObj = compiler.ExecuteMethod("GalleryHelper", "GetFileName", new object[] { photoIndex });
    }

    string imagePath = pathObj != null ? pathObj.ToString() : "";
    string imageName = nameObj != null ? nameObj.ToString() : "Photo";

    _engine.SetVariable("imagePath", imagePath);
    _engine.SetVariable("imageName", imageName);

    List<ContextMenuItem> items = new List<ContextMenuItem>();
    items.Add(new ContextMenuItem { Text = "Copy", Icon = "📋", Handler = "CopyPhotoAction" });
    items.Add(new ContextMenuItem { Text = "Share", Icon = "📤", Handler = "SharePhoto" });
    items.Add(new ContextMenuItem { Text = "Show in Explorer", Icon = "📂", Handler = "ShowInExplorer" });
    items.Add(new ContextMenuItem { Text = "Add to Favorites", Icon = "❤️", Handler = "AddToFavAction" });
    items.Add(new ContextMenuItem { IsSeparator = true });
    items.Add(new ContextMenuItem { Text = "Delete", Icon = "🗑️", Handler = "DeletePhotoFromGrid", Danger = true });

    if (isRightClick)
    {
        // Windows style - mouse position da
        ShowWindowsContextMenu(items, sender);
    }
    else
    {
        // iOS style - bottom action sheet
        ShowIOSActionSheet(imageName, items, sender);
    }
}
private void ShowWindowsContextMenu(List<ContextMenuItem> items, object sender)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // Invisible overlay
        Grid overlay = new Grid();
        overlay.Background = Brushes.Transparent;
        Panel.SetZIndex(overlay, 60000);

        // Menu
        Border menu = new Border();
        menu.MinWidth = 200;
        menu.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
        menu.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        menu.BorderThickness = new Thickness(1);
        menu.CornerRadius = new CornerRadius(8);
        menu.Padding = new Thickness(4);
        menu.HorizontalAlignment = HorizontalAlignment.Left;
        menu.VerticalAlignment = VerticalAlignment.Top;
        Panel.SetZIndex(menu, 60001);

        menu.Effect = new DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = 16,
            ShadowDepth = 4,
            Opacity = 0.4,
            Direction = 270
        };

        StackPanel itemsPanel = new StackPanel();

        foreach (ContextMenuItem item in items)
        {
            if (item.IsSeparator)
            {
                Border sep = new Border();
                sep.Height = 1;
                sep.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                sep.Margin = new Thickness(8, 4, 8, 4);
                itemsPanel.Children.Add(sep);
                continue;
            }

            Border itemBorder = new Border();
            itemBorder.Padding = new Thickness(12, 8, 12, 8);
            itemBorder.CornerRadius = new CornerRadius(4);
            itemBorder.Cursor = Cursors.Hand;
            itemBorder.Background = Brushes.Transparent;

            Grid itemGrid = new Grid();
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (!string.IsNullOrEmpty(item.Icon))
            {
                TextBlock iconTb = new TextBlock();
                iconTb.Text = item.Icon;
                iconTb.FontSize = 14;
                iconTb.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(iconTb, 0);
                itemGrid.Children.Add(iconTb);
            }

            TextBlock textTb = new TextBlock();
            textTb.Text = item.Text;
            textTb.FontSize = 13;
            textTb.VerticalAlignment = VerticalAlignment.Center;
            textTb.Foreground = item.Danger ? new SolidColorBrush(Color.FromRgb(255, 69, 58)) : Brushes.White;
            Grid.SetColumn(textTb, 1);
            itemGrid.Children.Add(textTb);

            itemBorder.Child = itemGrid;

            itemBorder.MouseEnter += delegate
            {
                itemBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
            };
            itemBorder.MouseLeave += delegate
            {
                itemBorder.Background = Brushes.Transparent;
            };

            string handlerName = item.Handler;
            itemBorder.MouseLeftButtonUp += delegate
            {
                if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                if (rootGrid.Children.Contains(menu)) rootGrid.Children.Remove(menu);
                if (!string.IsNullOrEmpty(handlerName))
                    _engine.ExecuteHandler(handlerName, sender);
            };

            itemsPanel.Children.Add(itemBorder);
        }

        menu.Child = itemsPanel;

        // Position at mouse
        Point mousePos = Mouse.GetPosition(rootGrid);
        double left = mousePos.X;
        double top = mousePos.Y;

        // Boundary check
        menu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double menuWidth = menu.DesiredSize.Width > 0 ? menu.DesiredSize.Width : 200;
        double menuHeight = menu.DesiredSize.Height > 0 ? menu.DesiredSize.Height : 200;

        if (left + menuWidth > rootGrid.ActualWidth - 10)
            left = rootGrid.ActualWidth - menuWidth - 10;
        if (top + menuHeight > rootGrid.ActualHeight - 10)
            top = mousePos.Y - menuHeight;
        if (left < 10) left = 10;
        if (top < 10) top = 10;

        menu.Margin = new Thickness(left, top, 0, 0);

        overlay.MouseLeftButtonUp += delegate
        {
            if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
            if (rootGrid.Children.Contains(menu)) rootGrid.Children.Remove(menu);
        };
        overlay.MouseRightButtonUp += delegate
        {
            if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
            if (rootGrid.Children.Contains(menu)) rootGrid.Children.Remove(menu);
        };

        rootGrid.Children.Add(overlay);
        rootGrid.Children.Add(menu);

        // Fade in
        menu.Opacity = 0;
        DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
        menu.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }));
}
private void ShowIOSActionSheet(string title, List<ContextMenuItem> items, object sender)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // Overlay
        Grid overlay = new Grid();
        overlay.Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
        overlay.Opacity = 0;
        Panel.SetZIndex(overlay, 60000);

        // Container
        Border container = new Border();
        container.HorizontalAlignment = HorizontalAlignment.Center;
        container.VerticalAlignment = VerticalAlignment.Bottom;
        container.Margin = new Thickness(10, 0, 10, 10);
        container.MaxWidth = 400;
        Panel.SetZIndex(container, 60001);

        StackPanel outerPanel = new StackPanel();

        // Actions group
        Border actionsGroup = new Border();
        actionsGroup.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        actionsGroup.CornerRadius = new CornerRadius(14);
        actionsGroup.Margin = new Thickness(0, 0, 0, 8);

        StackPanel actionsPanel = new StackPanel();

        // Title (if provided)
        if (!string.IsNullOrEmpty(title))
        {
            Border titleBorder = new Border();
            titleBorder.Padding = new Thickness(16, 12, 16, 8);
            TextBlock titleTb = new TextBlock();
            titleTb.Text = title;
            titleTb.FontSize = 13;
            titleTb.Foreground = new SolidColorBrush(Color.FromRgb(142, 142, 147));
            titleTb.TextAlignment = TextAlignment.Center;
            titleTb.TextTrimming = TextTrimming.CharacterEllipsis;
            titleBorder.Child = titleTb;
            actionsPanel.Children.Add(titleBorder);

            Border titleSep = new Border();
            titleSep.Height = 0.5;
            titleSep.Background = new SolidColorBrush(Color.FromRgb(68, 68, 70));
            actionsPanel.Children.Add(titleSep);
        }

        for (int i = 0; i < items.Count; i++)
        {
            ContextMenuItem item = items[i];

            if (item.IsSeparator)
            {
                Border sep = new Border();
                sep.Height = 6;
                sep.Background = new SolidColorBrush(Color.FromRgb(28, 28, 30));
                actionsPanel.Children.Add(sep);
                continue;
            }

            Border itemBorder = new Border();
            itemBorder.Padding = new Thickness(0, 14, 0, 14);
            itemBorder.Cursor = Cursors.Hand;
            itemBorder.Background = Brushes.Transparent;

            Grid itemGrid = new Grid();
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock textTb = new TextBlock();
            textTb.Text = item.Text;
            textTb.FontSize = 20;
            textTb.HorizontalAlignment = HorizontalAlignment.Center;
            textTb.Foreground = item.Danger ?
                new SolidColorBrush(Color.FromRgb(255, 59, 48)) :
                new SolidColorBrush(Color.FromRgb(10, 132, 255));
            Grid.SetColumn(textTb, 0);
            Grid.SetColumnSpan(textTb, 2);
            itemGrid.Children.Add(textTb);

            itemBorder.Child = itemGrid;

            itemBorder.MouseEnter += delegate
            {
                itemBorder.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            };
            itemBorder.MouseLeave += delegate
            {
                itemBorder.Background = Brushes.Transparent;
            };

            string handlerName = item.Handler;
            itemBorder.MouseLeftButtonUp += delegate
            {
                // Animate out
                TranslateTransform closeTrans = new TranslateTransform(0, 0);
                container.RenderTransform = closeTrans;
                DoubleAnimation slideOut = new DoubleAnimation(0, 400, TimeSpan.FromMilliseconds(250));
                slideOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                slideOut.Completed += delegate
                {
                    if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                    if (rootGrid.Children.Contains(container)) rootGrid.Children.Remove(container);
                    if (!string.IsNullOrEmpty(handlerName))
                        _engine.ExecuteHandler(handlerName, sender);
                };
                closeTrans.BeginAnimation(TranslateTransform.YProperty, slideOut);

                DoubleAnimation overlayFadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                overlay.BeginAnimation(UIElement.OpacityProperty, overlayFadeOut);
            };

            actionsPanel.Children.Add(itemBorder);

            // Separator
            if (i < items.Count - 1 && !items[i + 1].IsSeparator)
            {
                Border sep = new Border();
                sep.Height = 0.5;
                sep.Background = new SolidColorBrush(Color.FromRgb(68, 68, 70));
                actionsPanel.Children.Add(sep);
            }
        }

        actionsGroup.Child = actionsPanel;
        outerPanel.Children.Add(actionsGroup);

        // Cancel button
        Border cancelBtn = new Border();
        cancelBtn.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        cancelBtn.CornerRadius = new CornerRadius(14);
        cancelBtn.Padding = new Thickness(0, 16, 0, 16);
        cancelBtn.Cursor = Cursors.Hand;

        TextBlock cancelText = new TextBlock();
        cancelText.Text = "Cancel";
        cancelText.FontSize = 20;
        cancelText.FontWeight = FontWeights.SemiBold;
        cancelText.Foreground = new SolidColorBrush(Color.FromRgb(10, 132, 255));
        cancelText.HorizontalAlignment = HorizontalAlignment.Center;
        cancelBtn.Child = cancelText;

        cancelBtn.MouseEnter += delegate
        {
            cancelBtn.Background = new SolidColorBrush(Color.FromRgb(58, 58, 60));
        };
        cancelBtn.MouseLeave += delegate
        {
            cancelBtn.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        };
        cancelBtn.MouseLeftButtonUp += delegate
        {
            TranslateTransform closeTrans = new TranslateTransform(0, 0);
            container.RenderTransform = closeTrans;
            DoubleAnimation slideOut = new DoubleAnimation(0, 400, TimeSpan.FromMilliseconds(250));
            slideOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            slideOut.Completed += delegate
            {
                if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                if (rootGrid.Children.Contains(container)) rootGrid.Children.Remove(container);
            };
            closeTrans.BeginAnimation(TranslateTransform.YProperty, slideOut);

            DoubleAnimation overlayFadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            overlay.BeginAnimation(UIElement.OpacityProperty, overlayFadeOut);
        };

        outerPanel.Children.Add(cancelBtn);
        container.Child = outerPanel;

        // Overlay click
        overlay.MouseLeftButtonUp += delegate
        {
            TranslateTransform closeTrans = new TranslateTransform(0, 0);
            container.RenderTransform = closeTrans;
            DoubleAnimation slideOut = new DoubleAnimation(0, 400, TimeSpan.FromMilliseconds(250));
            slideOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            slideOut.Completed += delegate
            {
                if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                if (rootGrid.Children.Contains(container)) rootGrid.Children.Remove(container);
            };
            closeTrans.BeginAnimation(TranslateTransform.YProperty, slideOut);

            DoubleAnimation overlayFadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            overlay.BeginAnimation(UIElement.OpacityProperty, overlayFadeOut);
        };

        rootGrid.Children.Add(overlay);
        rootGrid.Children.Add(container);

        // Animate in
        TranslateTransform trans = new TranslateTransform(0, 400);
        container.RenderTransform = trans;
        DoubleAnimation slideUp = new DoubleAnimation(400, 0, TimeSpan.FromMilliseconds(300));
        slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        trans.BeginAnimation(TranslateTransform.YProperty, slideUp);

        DoubleAnimation overlayFadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        overlay.BeginAnimation(UIElement.OpacityProperty, overlayFadeIn);
    }));
}
private System.Windows.Controls.Image FindChildImage(DependencyObject parent)
{
    if (parent == null) return null;
    if (parent is System.Windows.Controls.Image)
        return (System.Windows.Controls.Image)parent;

    int count = VisualTreeHelper.GetChildrenCount(parent);
    for (int i = 0; i < count; i++)
    {
        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        System.Windows.Controls.Image result = FindChildImage(child);
        if (result != null) return result;
    }

    if (parent is ContentControl && ((ContentControl)parent).Content is DependencyObject)
        return FindChildImage((DependencyObject)((ContentControl)parent).Content);
    if (parent is Border && ((Border)parent).Child != null)
        return FindChildImage(((Border)parent).Child);
    if (parent is Decorator && ((Decorator)parent).Child != null)
        return FindChildImage(((Decorator)parent).Child);

    return null;
}

private object ConvertParamValue(string value, string type)
{
    switch (type.ToLower())
    {
        case "int":
        case "integer":
            int intVal;
            if (int.TryParse(value, out intVal)) return intVal;
            return 0;

        case "double":
        case "float":
        case "number":
            double dblVal;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out dblVal))
                return dblVal;
            return 0.0;

        case "bool":
        case "boolean":
            return value.ToLower() == "true" || value == "1";

        case "auto":
        default:
            // Avtomatik aniqlash
            int autoInt;
            if (int.TryParse(value, out autoInt)) return autoInt;

            double autoDbl;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out autoDbl))
                return autoDbl;

            if (value.ToLower() == "true") return true;
            if (value.ToLower() == "false") return false;

            return value;
    }
}
        private void ExecuteNewWindow(XmlNode node)
{
    string source = GetAttribute(node, "Source", "");
    string name = GetAttribute(node, "Name", "");
    
    if (string.IsNullOrEmpty(source)) return;
    
    string fullPath = "";
    if (!string.IsNullOrEmpty(_engine.CurrentXmlPath))
    {
        fullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_engine.CurrentXmlPath), source);
    }
    else
    {
        fullPath = System.IO.Path.GetFullPath(source);
    }

    if (System.IO.File.Exists(fullPath))
    {
        // Yangi engine va thread yaratish
        System.Threading.Thread thread = new System.Threading.Thread(delegate()
        {
            try
            {
                WpfEngine newEngine = new WpfEngine();
                // State ni ulashish (ixtiyoriy, hozircha izolyatsiya qilingan)
                // newEngine.IsDevMode = _engine.IsDevMode;
                newEngine.Run(fullPath, false);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening window: " + ex.Message);
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        
        _engine.Log("WINDOW", "Opened new window: " + source);
    }
    else
    {
        _engine.Log("ERROR", "Window file not found: " + fullPath);
    }
}

        private void ExecuteIncrement(XmlNode node)
{
    string state = GetAttribute(node, "State", "");
    string variable = GetAttribute(node, "Variable", "");
    if (string.IsNullOrEmpty(variable))
        variable = GetAttribute(node, "Var", state);

    // "By" va "Value" ikkalasini ham qo'llab-quvvatlash
    string value = GetAttribute(node, "By", "");
    if (string.IsNullOrEmpty(value))
        value = GetAttribute(node, "Value", "1");

    value = ParseControlProperty(value);

    if (!string.IsNullOrEmpty(variable))
    {
        _engine.IncrementVariable(variable, value);
    }
}
        
        private void ExecuteDecrement(XmlNode node)
{
    string state = GetAttribute(node, "State", "");
    string variable = GetAttribute(node, "Variable", "");
    if (string.IsNullOrEmpty(variable))
        variable = GetAttribute(node, "Var", state);
    
    // FIX: "By" ham qo'llab-quvvatlash
    string value = GetAttribute(node, "By", "");
    if (string.IsNullOrEmpty(value))
        value = GetAttribute(node, "Value", "1");
    
    value = ParseControlProperty(value);
    
    if (!string.IsNullOrEmpty(variable))
    {
        _engine.DecrementVariable(variable, value);
    }
}
        
        private void ExecuteMultiply(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseControlProperty(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.MultiplyVariable(variable, value);
            }
        }
        
        private void ExecuteDivide(XmlNode node)
        {
            string state = GetAttribute(node, "State", "");
            string variable = GetAttribute(node, "Variable", GetAttribute(node, "Var", state));
            string value = GetAttribute(node, "Value", "1");
            
            value = ParseControlProperty(value);
            
            if (!string.IsNullOrEmpty(variable))
            {
                _engine.DivideVariable(variable, value);
            }
        }
        
        #endregion
        
        #region Basic Commands
        
        private void ExecuteSet(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    if (string.IsNullOrEmpty(controlName)) controlName = GetAttribute(node, "Target", "");

    string property = GetAttribute(node, "Property", "");
    string value = GetAttribute(node, "Value", "");
    string varName = GetAttribute(node, "Var", "");
    if (string.IsNullOrEmpty(varName)) varName = GetAttribute(node, "State", "");

    value = ReplaceStateVariables(value);

    // Update variable if specified
    if (!string.IsNullOrEmpty(varName))
    {
        _engine.SetVariable(varName, value);
    }

    // Update control if specified
    if (!string.IsNullOrEmpty(controlName) && !string.IsNullOrEmpty(property))
    {
        _engine.SetControlProperty(controlName, property, value);
    }
}

private void ExecuteGet(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    string property = GetAttribute(node, "Property", "");
    string toState = GetAttribute(node, "ToState", "");

    if (!string.IsNullOrEmpty(controlName) && !string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(toState))
    {
        object val = _engine.GetControlProperty(controlName, property);
        if (val != null)
        {
            _engine.SetVariable(toState, val.ToString());
        }
    }
}
        
        private void ExecuteIf(XmlNode node, object sender)
{
    string condition = GetAttribute(node, "Condition", "");
    bool result = EvaluateCondition(condition);

    if (result)
    {
        XmlNode thenNode = node.SelectSingleNode("Then");
        if (thenNode != null)
        {
            Execute(thenNode, sender);
        }
        else
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.Name != "Else" && child.Name != "Then")
                {
                    ExecuteCommand(child, sender);
                }
            }
        }
    }
    else
    {
        XmlNode elseNode = node.SelectSingleNode("Else");
        if (elseNode != null)
        {
            Execute(elseNode, sender);
        }
    }
}
        
        private void ExecuteSwitch(XmlNode node, object sender)
        {
            string expression = GetAttribute(node, "Expression", "");
            string value = "";
            
            if (expression.Contains("."))
            {
                string[] parts = expression.Split('.');
                object propValue = _engine.GetControlProperty(parts[0], parts[1]);
                value = propValue != null ? propValue.ToString() : "";
            }
            else if (_engine.State.ContainsKey(expression))
            {
                object stateValue = _engine.State[expression];
                value = stateValue != null ? stateValue.ToString() : "";
            }
            
            bool matched = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                if (child.Name == "Case")
                {
                    string caseValue = GetAttribute(child, "Value", "");
                    if (caseValue == value)
                    {
                        Execute(child, sender);
                        matched = true;
                        break;
                    }
                }
            }
            
            if (!matched)
            {
                XmlNode defaultNode = node.SelectSingleNode("Default");
                if (defaultNode != null)
                {
                    Execute(defaultNode, sender);
                }
            }
        }
        
        private void ExecuteShow(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
    int duration = 200;
    int.TryParse(GetAttribute(node, "Duration", "200"), out duration);
    string effect = GetAttribute(node, "Effect", "fade"); // fade, zoomIn, slideUp, slideDown

    FrameworkElement control = _engine.GetControl(controlName);
    if (control == null) return;

    control.Visibility = Visibility.Visible;

    // Overlay uchun yuqori ZIndex
    if (control is Border)
    {
        Border brd = (Border)control;
        if (brd.Background is SolidColorBrush)
        {
            SolidColorBrush brush = (SolidColorBrush)brd.Background;
            if (brush.Color.R < 30 && brush.Color.G < 30 && brush.Color.B < 30 && brush.Color.A > 200)
            {
                Panel.SetZIndex(control, 10000);
            }
        }
    }

    if (animate)
    {
        control.BeginAnimation(UIElement.OpacityProperty, null);

        switch (effect.ToLower())
        {
            case "zoomin":
            case "zoom":
                control.Opacity = 0;
                ScaleTransform scaleIn = new ScaleTransform(0.8, 0.8);
                control.RenderTransform = scaleIn;
                control.RenderTransformOrigin = new Point(0.5, 0.5);

                DoubleAnimation fadeZoom = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration));
                fadeZoom.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                control.BeginAnimation(UIElement.OpacityProperty, fadeZoom);

                DoubleAnimation scaleAnim = new DoubleAnimation(0.8, 1.0, TimeSpan.FromMilliseconds(duration));
                scaleAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                scaleIn.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scaleIn.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                break;

            case "slideup":
                control.Opacity = 0;
                TranslateTransform transUp = new TranslateTransform(0, 50);
                control.RenderTransform = transUp;

                DoubleAnimation fadeUp = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration));
                control.BeginAnimation(UIElement.OpacityProperty, fadeUp);

                DoubleAnimation slideUp = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(duration));
                slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                transUp.BeginAnimation(TranslateTransform.YProperty, slideUp);
                break;

            case "slidedown":
                control.Opacity = 0;
                TranslateTransform transDown = new TranslateTransform(0, -50);
                control.RenderTransform = transDown;

                DoubleAnimation fadeDown = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration));
                control.BeginAnimation(UIElement.OpacityProperty, fadeDown);

                DoubleAnimation slideDown = new DoubleAnimation(-50, 0, TimeSpan.FromMilliseconds(duration));
                slideDown.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                transDown.BeginAnimation(TranslateTransform.YProperty, slideDown);
                break;

            default: // fade
                control.Opacity = 0;
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(duration));
                fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                control.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                break;
        }
    }
    else
    {
        control.BeginAnimation(UIElement.OpacityProperty, null);
        control.Opacity = 1;
    }
}

private void ExecuteHide(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
    int duration = 200;
    int.TryParse(GetAttribute(node, "Duration", "200"), out duration);
    string effect = GetAttribute(node, "Effect", "fade");

    FrameworkElement control = _engine.GetControl(controlName);
    if (control == null) return;

    if (animate)
    {
        switch (effect.ToLower())
        {
            case "zoomout":
            case "zoom":
                ScaleTransform scaleOut = control.RenderTransform as ScaleTransform;
                if (scaleOut == null)
                {
                    scaleOut = new ScaleTransform(1, 1);
                    control.RenderTransform = scaleOut;
                    control.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                DoubleAnimation fadeZoomOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(duration));
                fadeZoomOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                fadeZoomOut.Completed += delegate
                {
                    control.Visibility = Visibility.Collapsed;
                    Panel.SetZIndex(control, 0);
                    control.BeginAnimation(UIElement.OpacityProperty, null);
                    control.Opacity = 1;
                    scaleOut.ScaleX = 1;
                    scaleOut.ScaleY = 1;
                };
                control.BeginAnimation(UIElement.OpacityProperty, fadeZoomOut);

                DoubleAnimation scaleOutAnim = new DoubleAnimation(1.0, 0.8, TimeSpan.FromMilliseconds(duration));
                scaleOutAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                scaleOut.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOutAnim);
                scaleOut.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOutAnim);
                break;

            default: // fade
                DoubleAnimation fadeOut = new DoubleAnimation(control.Opacity, 0, TimeSpan.FromMilliseconds(duration));
                fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                fadeOut.Completed += delegate
                {
                    control.Visibility = Visibility.Collapsed;
                    Panel.SetZIndex(control, 0);
                    control.BeginAnimation(UIElement.OpacityProperty, null);
                    control.Opacity = 1;
                };
                control.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                break;
        }
    }
    else
    {
        control.Visibility = Visibility.Collapsed;
        Panel.SetZIndex(control, 0);
        control.BeginAnimation(UIElement.OpacityProperty, null);
        control.Opacity = 1;
    }
}
private void ShowNativeContextMenu(string title, List<ContextMenuItem> items, object sender, int photoIndex)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // Overlay (transparent, click to close)
        Grid overlay = new Grid();
        overlay.Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
        Panel.SetZIndex(overlay, 60000);

        // Menu container
        Border menuContainer = new Border();
        menuContainer.HorizontalAlignment = HorizontalAlignment.Center;
        menuContainer.VerticalAlignment = VerticalAlignment.Bottom;
        menuContainer.Margin = new Thickness(10, 0, 10, 20);
        menuContainer.MaxWidth = 380;
        Panel.SetZIndex(menuContainer, 60001);

        StackPanel outerPanel = new StackPanel();

        // Actions group
        Border actionsGroup = new Border();
        actionsGroup.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        actionsGroup.CornerRadius = new CornerRadius(14);
        actionsGroup.Margin = new Thickness(0, 0, 0, 8);

        StackPanel actionsPanel = new StackPanel();

        for (int i = 0; i < items.Count; i++)
        {
            ContextMenuItem item = items[i];

            if (item.IsSeparator)
            {
                Border sep = new Border();
                sep.Height = 0.5;
                sep.Background = new SolidColorBrush(Color.FromRgb(68, 68, 70));
                sep.Margin = new Thickness(0);
                actionsPanel.Children.Add(sep);
                continue;
            }

            Border itemBorder = new Border();
            itemBorder.Padding = new Thickness(16, 14, 16, 14);
            itemBorder.Cursor = Cursors.Hand;
            itemBorder.Background = Brushes.Transparent;

            Grid itemGrid = new Grid();
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock textBlock = new TextBlock();
            textBlock.Text = item.Text;
            textBlock.FontSize = 17;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            if (item.Danger)
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 59, 48));
            else
                textBlock.Foreground = Brushes.White;

            Grid.SetColumn(textBlock, 0);
            itemGrid.Children.Add(textBlock);

            if (!string.IsNullOrEmpty(item.Icon))
            {
                TextBlock iconBlock = new TextBlock();
                iconBlock.Text = item.Icon;
                iconBlock.FontSize = 20;
                iconBlock.VerticalAlignment = VerticalAlignment.Center;
                iconBlock.Margin = new Thickness(12, 0, 0, 0);
                Grid.SetColumn(iconBlock, 1);
                itemGrid.Children.Add(iconBlock);
            }

            itemBorder.Child = itemGrid;

            // Hover
            itemBorder.MouseEnter += delegate
            {
                itemBorder.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
            };
            itemBorder.MouseLeave += delegate
            {
                itemBorder.Background = Brushes.Transparent;
            };

            // Click
            string handlerName = item.Handler;
            itemBorder.MouseLeftButtonUp += delegate
            {
                // Close menu
                if (rootGrid.Children.Contains(overlay))
                    rootGrid.Children.Remove(overlay);
                if (rootGrid.Children.Contains(menuContainer))
                    rootGrid.Children.Remove(menuContainer);

                // Execute handler
                if (!string.IsNullOrEmpty(handlerName))
                {
                    _engine.ExecuteHandler(handlerName, sender);
                }
            };

            actionsPanel.Children.Add(itemBorder);

            // Separator between items (not after last)
            if (i < items.Count - 1 && !items[i + 1].IsSeparator)
            {
                Border autoSep = new Border();
                autoSep.Height = 0.5;
                autoSep.Background = new SolidColorBrush(Color.FromRgb(68, 68, 70));
                autoSep.Margin = new Thickness(16, 0, 0, 0);
                actionsPanel.Children.Add(autoSep);
            }
        }

        actionsGroup.Child = actionsPanel;
        outerPanel.Children.Add(actionsGroup);

        // Cancel button
        Border cancelBtn = new Border();
        cancelBtn.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        cancelBtn.CornerRadius = new CornerRadius(14);
        cancelBtn.Padding = new Thickness(16, 16, 16, 16);
        cancelBtn.Cursor = Cursors.Hand;

        TextBlock cancelText = new TextBlock();
        cancelText.Text = "Cancel";
        cancelText.FontSize = 20;
        cancelText.FontWeight = FontWeights.SemiBold;
        cancelText.Foreground = new SolidColorBrush(Color.FromRgb(10, 132, 255));
        cancelText.HorizontalAlignment = HorizontalAlignment.Center;
        cancelBtn.Child = cancelText;

        cancelBtn.MouseEnter += delegate
        {
            cancelBtn.Background = new SolidColorBrush(Color.FromRgb(58, 58, 60));
        };
        cancelBtn.MouseLeave += delegate
        {
            cancelBtn.Background = new SolidColorBrush(Color.FromRgb(44, 44, 46));
        };
        cancelBtn.MouseLeftButtonUp += delegate
        {
            if (rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);
            if (rootGrid.Children.Contains(menuContainer))
                rootGrid.Children.Remove(menuContainer);
        };

        outerPanel.Children.Add(cancelBtn);
        menuContainer.Child = outerPanel;

        // Overlay click to close
        overlay.MouseLeftButtonUp += delegate
        {
            if (rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);
            if (rootGrid.Children.Contains(menuContainer))
                rootGrid.Children.Remove(menuContainer);
        };

        rootGrid.Children.Add(overlay);
        rootGrid.Children.Add(menuContainer);

        // Slide up animation
        TranslateTransform trans = new TranslateTransform(0, 400);
        menuContainer.RenderTransform = trans;
        DoubleAnimation slideUp = new DoubleAnimation(400, 0, TimeSpan.FromMilliseconds(300));
        slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        trans.BeginAnimation(TranslateTransform.YProperty, slideUp);

        DoubleAnimation overlayFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        overlay.BeginAnimation(UIElement.OpacityProperty, overlayFade);
    }));
}
        
        private void ExecuteToggle(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            if (control == null) return;
            
            control.Visibility = control.Visibility == Visibility.Visible ? 
                Visibility.Collapsed : Visibility.Visible;
        }
        
        private void ExecuteEnable(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                control.IsEnabled = true;
            }
        }
        
        private void ExecuteDisable(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                control.IsEnabled = false;
            }
        }
        
        private void ExecuteFocus(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                control.Focus();
            }
        }
        
        private void ExecuteAlert(XmlNode node)
{
    string message = GetAttribute(node, "Message", node.InnerText);
    string title = GetAttribute(node, "Title", "Alert");
    string icon = GetAttribute(node, "Icon", "Information");
    string custom = GetAttribute(node, "Custom", "false");
    string type = GetAttribute(node, "Type", "alert"); // alert, modal, toast, drawer, bottomsheet
    string position = GetAttribute(node, "Position", "center");
    string animation = GetAttribute(node, "Animation", "fade");
    string width = GetAttribute(node, "Width", "400");
    string height = GetAttribute(node, "Height", "Auto");
    string bg = GetAttribute(node, "Background", "#1E1E1E");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string borderColor = GetAttribute(node, "BorderColor", "#3D3D3D");
    string cornerRadius = GetAttribute(node, "CornerRadius", "12");
    string buttonText = GetAttribute(node, "ButtonText", "OK");
    string buttonBg = GetAttribute(node, "ButtonBackground", "#8774E1");
    string showClose = GetAttribute(node, "ShowClose", "true");
    string overlay = GetAttribute(node, "Overlay", "true");
    string overlayColor = GetAttribute(node, "OverlayColor", "#000000");
    string overlayOpacity = GetAttribute(node, "OverlayOpacity", "0.5");
    string autoClose = GetAttribute(node, "AutoClose", "0");
    string draggable = GetAttribute(node, "Draggable", "false");

    message = ReplaceStateVariables(message);
    message = ParseControlProperty(message);
    title = ReplaceStateVariables(title);

    // Custom popup
    if (custom.ToLower() == "true" || type.ToLower() != "alert")
    {
        ShowCustomPopup(new PopupOptions
        {
            Type = ParsePopupType(type),
            Title = title,
            Message = message,
            Icon = icon,
            Position = ParsePopupPosition(position),
            Animation = ParsePopupAnimation(animation),
            Width = WpfEngine.ParseDouble(width, 400),
            Height = height == "Auto" ? double.NaN : WpfEngine.ParseDouble(height, 200),
            Background = bg,
            Foreground = fg,
            BorderColor = borderColor,
            CornerRadius = WpfEngine.ParseDouble(cornerRadius, 12),
            ButtonText = buttonText,
            ButtonBackground = buttonBg,
            ShowCloseButton = showClose.ToLower() == "true",
            ShowOverlay = overlay.ToLower() == "true",
            OverlayColor = overlayColor,
            OverlayOpacity = WpfEngine.ParseDouble(overlayOpacity, 0.5),
            AutoCloseMs = WpfEngine.ParseInt(autoClose, 0),
            Draggable = draggable.ToLower() == "true"
        });
        return;
    }

    // Default WPF MessageBox
    MessageBoxImage msgIcon = MessageBoxImage.Information;
    switch (icon.ToLower())
    {
        case "error": msgIcon = MessageBoxImage.Error; break;
        case "warning": msgIcon = MessageBoxImage.Warning; break;
        case "question": msgIcon = MessageBoxImage.Question; break;
    }

    MessageBox.Show(message, title, MessageBoxButton.OK, msgIcon);
}
        
        private void ExecuteConfirm(XmlNode node, object sender)
{
    string message = GetAttribute(node, "Message", "Are you sure?");
    string title = GetAttribute(node, "Title", "Confirm");
    string custom = GetAttribute(node, "Custom", "false");
    string type = GetAttribute(node, "Type", "confirm");
    string position = GetAttribute(node, "Position", "center");
    string animation = GetAttribute(node, "Animation", "scale");
    string bg = GetAttribute(node, "Background", "#1E1E1E");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string yesText = GetAttribute(node, "YesText", "Yes");
    string noText = GetAttribute(node, "NoText", "No");
    string yesBg = GetAttribute(node, "YesBackground", "#8774E1");
    string noBg = GetAttribute(node, "NoBackground", "#3D3D3D");
    string icon = GetAttribute(node, "Icon", "❓");
    string draggable = GetAttribute(node, "Draggable", "false");

    message = ReplaceStateVariables(message);

    XmlNode yesNode = node.SelectSingleNode("Yes");
    XmlNode noNode = node.SelectSingleNode("No");

    if (custom.ToLower() == "true")
    {
        ShowCustomConfirm(new ConfirmOptions
        {
            Title = title,
            Message = message,
            Icon = icon,
            Position = ParsePopupPosition(position),
            Animation = ParsePopupAnimation(animation),
            Background = bg,
            Foreground = fg,
            YesText = yesText,
            NoText = noText,
            YesBackground = yesBg,
            NoBackground = noBg,
            Draggable = draggable.ToLower() == "true",
            OnYes = delegate { if (yesNode != null) Execute(yesNode, sender); },
            OnNo = delegate { if (noNode != null) Execute(noNode, sender); }
        });
        return;
    }

    // Default
    MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes && yesNode != null)
    {
        Execute(yesNode, sender);
    }
    else if (result == MessageBoxResult.No && noNode != null)
    {
        Execute(noNode, sender);
    }
}
private void ShowCustomPopup(PopupOptions options)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(delegate
    {
        Grid overlay = null;
        if (options.ShowOverlay)
        {
            overlay = new Grid();
            overlay.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(options.OverlayColor));
            overlay.Opacity = options.OverlayOpacity;
            Panel.SetZIndex(overlay, 50000);
        }

        Border popup = new Border();
        popup.Width = options.Width;
        if (!double.IsNaN(options.Height)) popup.Height = options.Height;
        popup.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.Background));
        popup.BorderBrush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.BorderColor));
        popup.BorderThickness = new Thickness(1);
        popup.CornerRadius = new CornerRadius(options.CornerRadius);
        popup.HorizontalAlignment = HorizontalAlignment.Center;
        popup.VerticalAlignment = VerticalAlignment.Center;
        Panel.SetZIndex(popup, 50001);

        popup.Effect = new DropShadowEffect
        {
            Color = Colors.Black, BlurRadius = 30, ShadowDepth = 0, Opacity = 0.5
        };

        StackPanel content = new StackPanel();
        content.Margin = new Thickness(24);

        if (!string.IsNullOrEmpty(options.Title) || options.ShowCloseButton)
        {
            Grid header = new Grid();
            header.Margin = new Thickness(0, 0, 0, 16);

            TextBlock titleText = new TextBlock();
            titleText.Text = options.Title;
            titleText.FontSize = 18;
            titleText.FontWeight = FontWeights.SemiBold;
            titleText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(options.Foreground));
            header.Children.Add(titleText);

            if (options.ShowCloseButton)
            {
                Border closeBtn = new Border();
                closeBtn.Width = 32; closeBtn.Height = 32;
                closeBtn.CornerRadius = new CornerRadius(16);
                closeBtn.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
                closeBtn.Cursor = Cursors.Hand;

                TextBlock closeX = new TextBlock();
                closeX.Text = "✕"; closeX.FontSize = 14;
                closeX.Foreground = Brushes.White;
                closeX.HorizontalAlignment = HorizontalAlignment.Center;
                closeX.VerticalAlignment = VerticalAlignment.Center;
                closeBtn.Child = closeX;
                closeBtn.MouseLeftButtonUp += delegate { ClosePopup(mainWindow, overlay, popup); };
                header.Children.Add(closeBtn);
            }
            content.Children.Add(header);
        }

        if (!string.IsNullOrEmpty(options.Icon))
        {
            TextBlock iconText = new TextBlock();
            iconText.Text = GetIconForType(options.Icon);
            iconText.FontSize = 48;
            iconText.HorizontalAlignment = HorizontalAlignment.Center;
            iconText.Margin = new Thickness(0, 0, 0, 16);
            content.Children.Add(iconText);
        }

        TextBlock messageText = new TextBlock();
        messageText.Text = options.Message.Replace("\\n", "\n");
        messageText.FontSize = 15;
        messageText.Foreground = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.Foreground));
        messageText.TextWrapping = TextWrapping.Wrap;
        messageText.TextAlignment = TextAlignment.Center;
        messageText.Margin = new Thickness(0, 0, 0, 24);
        content.Children.Add(messageText);

        Border button = new Border();
        button.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.ButtonBackground));
        button.CornerRadius = new CornerRadius(8);
        button.Padding = new Thickness(32, 12, 32, 12);
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.Cursor = Cursors.Hand;

        TextBlock buttonText = new TextBlock();
        buttonText.Text = options.ButtonText;
        buttonText.FontSize = 14;
        buttonText.FontWeight = FontWeights.SemiBold;
        buttonText.Foreground = Brushes.White;
        button.Child = buttonText;
        button.MouseLeftButtonUp += delegate { ClosePopup(mainWindow, overlay, popup); };
        content.Children.Add(button);

        popup.Child = content;
        if (options.Draggable) MakeDraggable(popup);

        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid != null)
        {
            if (overlay != null)
            {
                overlay.MouseLeftButtonUp += delegate { ClosePopup(mainWindow, overlay, popup); };
                rootGrid.Children.Add(overlay);
            }
            rootGrid.Children.Add(popup);
            ApplyPopupAnimation(popup, options.Animation, true);

            if (options.AutoCloseMs > 0)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(options.AutoCloseMs);
                timer.Tick += delegate { timer.Stop(); ClosePopup(mainWindow, overlay, popup); };
                timer.Start();
            }
        }
    });
}
// Popup yopish
private void ClosePopup(Window mainWindow, Grid overlay, Border popup)
{
    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        if (popup != null && rootGrid.Children.Contains(popup))
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            fadeOut.Completed += delegate
            {
                if (rootGrid.Children.Contains(popup))
                    rootGrid.Children.Remove(popup);
                if (overlay != null && rootGrid.Children.Contains(overlay))
                    rootGrid.Children.Remove(overlay);
            };
            popup.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            if (overlay != null && rootGrid.Children.Contains(overlay))
            {
                DoubleAnimation overlayFade = new DoubleAnimation(overlay.Opacity, 0, TimeSpan.FromMilliseconds(200));
                overlay.BeginAnimation(UIElement.OpacityProperty, overlayFade);
            }
        }
        else
        {
            if (overlay != null && rootGrid.Children.Contains(overlay))
                rootGrid.Children.Remove(overlay);
        }
    }));
}

// Root Grid ni topish
private Grid FindRootGrid(Window window)
{
    if (window.Content is Grid) return (Grid)window.Content;
    if (window.Content is Border && ((Border)window.Content).Child is Grid)
        return (Grid)((Border)window.Content).Child;
    return null;
}

// Drag qilish imkoniyati
private void MakeDraggable(Border element)
{
    bool isDragging = false;
    Point clickPosition = new Point();

    element.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
    {
        isDragging = true;
        clickPosition = e.GetPosition(element);
        element.CaptureMouse();
    };

    element.MouseMove += delegate(object s, MouseEventArgs e)
    {
        if (isDragging && element.Parent is UIElement)
        {
            Point currentPosition = e.GetPosition((UIElement)element.Parent);
            double left = currentPosition.X - clickPosition.X;
            double top = currentPosition.Y - clickPosition.Y;

            element.Margin = new Thickness(left, top, 0, 0);
            element.HorizontalAlignment = HorizontalAlignment.Left;
            element.VerticalAlignment = VerticalAlignment.Top;
        }
    };

    element.MouseLeftButtonUp += delegate
    {
        isDragging = false;
        element.ReleaseMouseCapture();
    };
}

// Animatsiya qo'llash
private void ApplyPopupAnimation(Border element, PopupAnimation animation, bool isOpening)
{
    double fromOpacity = isOpening ? 0 : 1;
    double toOpacity = isOpening ? 1 : 0;

    DoubleAnimation fadeAnim = new DoubleAnimation();
    fadeAnim.From = fromOpacity;
    fadeAnim.To = toOpacity;
    fadeAnim.Duration = TimeSpan.FromMilliseconds(200);

    // RenderTransform tayyorlash
    if (element.RenderTransform == null || element.RenderTransform == Transform.Identity)
    {
        TransformGroup group = new TransformGroup();
        group.Children.Add(new ScaleTransform());
        group.Children.Add(new TranslateTransform());
        element.RenderTransform = group;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    TransformGroup transformGroup = element.RenderTransform as TransformGroup;
    ScaleTransform scale = transformGroup.Children[0] as ScaleTransform;
    TranslateTransform translate = transformGroup.Children[1] as TranslateTransform;

    switch (animation)
    {
        case PopupAnimation.Scale:
            if (scale != null)
            {
                DoubleAnimation scaleAnim = new DoubleAnimation();
                scaleAnim.From = isOpening ? 0.8 : 1;
                scaleAnim.To = isOpening ? 1 : 0.8;
                scaleAnim.Duration = TimeSpan.FromMilliseconds(200);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            }
            break;

        case PopupAnimation.SlideUp:
            if (translate != null)
            {
                DoubleAnimation slideAnim = new DoubleAnimation();
                slideAnim.From = isOpening ? 50 : 0;
                slideAnim.To = isOpening ? 0 : 50;
                slideAnim.Duration = TimeSpan.FromMilliseconds(200);
                translate.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            }
            break;
            
        case PopupAnimation.SlideDown:
            if (translate != null)
            {
                DoubleAnimation slideAnim = new DoubleAnimation();
                slideAnim.From = isOpening ? -50 : 0;
                slideAnim.To = isOpening ? 0 : -50;
                slideAnim.Duration = TimeSpan.FromMilliseconds(200);
                translate.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            }
            break;
    }

    element.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
}

// Icon olish
private string GetIconForType(string iconType)
{
    if (string.IsNullOrEmpty(iconType)) return "";
    switch (iconType.ToLower())
    {
        case "info": case "information": return "ℹ️";
        case "warn": case "warning": return "⚠️";
        case "error": return "❌";
        case "success": return "✅";
        case "question": return "❓";
        default: return iconType; // O'zini qaytarish (emoji bo'lsa)
    }
}

// Button yaratish
private Border CreateButton(string text, string bgColor, string fgColor)
{
    Border btn = new Border();
    btn.CornerRadius = new CornerRadius(6);
    btn.Padding = new Thickness(16, 8, 16, 8);
    btn.Cursor = Cursors.Hand;
    
    try { btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)); }
    catch { btn.Background = Brushes.Gray; }

    TextBlock txt = new TextBlock();
    txt.Text = text;
    txt.FontSize = 14;
    txt.FontWeight = FontWeights.SemiBold;
    try { txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor)); }
    catch { txt.Foreground = Brushes.White; }
    
    btn.Child = txt;
    return btn;
}
private void ShowModal(ModalOptions options)
{
    // Hozircha oddiy CustomPopup orqali simulyatsiya qilamiz
    // Yoki to'liq implementatsiya yozish kerak
    PopupOptions pop = new PopupOptions();
    pop.Title = options.Title;
    pop.Message = "(Modal Content)"; // ContentNode ni render qilish qiyinroq
    pop.Width = options.Width;
    pop.Height = options.Height;
    pop.Background = options.Background;
    pop.ShowCloseButton = options.ShowCloseButton;
    pop.ShowOverlay = options.ShowOverlay;
    pop.Animation = options.Animation;
    
    ShowCustomPopup(pop);
}

private void ShowDrawer(DrawerOptions options)
{
    // Drawer logikasi (soddalashtirilgan)
    Window win = _engine.GetMainWindow();
    if (win == null) return;
    
    win.Dispatcher.Invoke(new Action(delegate {
        Grid root = FindRootGrid(win);
        if (root == null) return;
        
        Border drawer = new Border();
        drawer.Width = options.Width;
        drawer.HorizontalAlignment = HorizontalAlignment.Right; // Default right
        drawer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(options.Background));
        
        // Close on click out
        drawer.MouseLeftButtonDown += delegate { /* Handle click */ };
        
        root.Children.Add(drawer);
    }));
}

private void ShowToast(ToastOptions options)
{
    Window win = _engine.GetMainWindow();
    if (win == null) return;

    win.Dispatcher.Invoke(new Action(delegate
    {
        Grid root = FindRootGrid(win);
        if (root == null) return;

        string bgColor = "#323232";
        string accentColor = "#FFFFFF";
        string iconText = options.Icon;

        switch (options.Type.ToLower())
        {
            case "success":
                bgColor = "#1B5E20";
                accentColor = "#4CAF50";
                if (string.IsNullOrEmpty(iconText) || iconText == "ℹ") iconText = "✓";
                break;
            case "error":
                bgColor = "#B71C1C";
                accentColor = "#FF5252";
                if (string.IsNullOrEmpty(iconText) || iconText == "ℹ") iconText = "✕";
                break;
            case "warning":
                bgColor = "#E65100";
                accentColor = "#FFB74D";
                if (string.IsNullOrEmpty(iconText) || iconText == "ℹ") iconText = "⚠";
                break;
            default:
                bgColor = "#1A237E";
                accentColor = "#42A5F5";
                if (string.IsNullOrEmpty(iconText)) iconText = "ℹ";
                break;
        }

        Border toast = new Border();
        toast.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor));
        toast.CornerRadius = new CornerRadius(12);
        toast.Padding = new Thickness(16, 12, 16, 12);
        toast.MinWidth = 200;
        toast.MaxWidth = 400;
        toast.Opacity = 0;
        toast.IsHitTestVisible = false;
        Panel.SetZIndex(toast, 99999);

        toast.Effect = new DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = 20,
            ShadowDepth = 4,
            Opacity = 0.4,
            Direction = 270
        };

        // Position
        switch (options.Position)
        {
            case PopupPosition.Top:
                toast.HorizontalAlignment = HorizontalAlignment.Center;
                toast.VerticalAlignment = VerticalAlignment.Top;
                toast.Margin = new Thickness(0, 20, 0, 0);
                break;
            case PopupPosition.Bottom:
                toast.HorizontalAlignment = HorizontalAlignment.Center;
                toast.VerticalAlignment = VerticalAlignment.Bottom;
                toast.Margin = new Thickness(0, 0, 0, 80);
                break;
            case PopupPosition.TopLeft:
                toast.HorizontalAlignment = HorizontalAlignment.Left;
                toast.VerticalAlignment = VerticalAlignment.Top;
                toast.Margin = new Thickness(20, 20, 0, 0);
                break;
            case PopupPosition.TopRight:
                toast.HorizontalAlignment = HorizontalAlignment.Right;
                toast.VerticalAlignment = VerticalAlignment.Top;
                toast.Margin = new Thickness(0, 20, 20, 0);
                break;
            case PopupPosition.BottomLeft:
                toast.HorizontalAlignment = HorizontalAlignment.Left;
                toast.VerticalAlignment = VerticalAlignment.Bottom;
                toast.Margin = new Thickness(20, 0, 0, 80);
                break;
            case PopupPosition.BottomRight:
                toast.HorizontalAlignment = HorizontalAlignment.Right;
                toast.VerticalAlignment = VerticalAlignment.Bottom;
                toast.Margin = new Thickness(0, 0, 20, 80);
                break;
            default:
                toast.HorizontalAlignment = HorizontalAlignment.Center;
                toast.VerticalAlignment = VerticalAlignment.Top;
                toast.Margin = new Thickness(0, 60, 0, 0);
                break;
        }

        StackPanel content = new StackPanel();
        content.Orientation = Orientation.Horizontal;

        Border accentLine = new Border();
        accentLine.Width = 3;
        accentLine.Height = 20;
        accentLine.CornerRadius = new CornerRadius(2);
        accentLine.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor));
        accentLine.Margin = new Thickness(0, 0, 10, 0);
        accentLine.VerticalAlignment = VerticalAlignment.Center;
        content.Children.Add(accentLine);

        if (!string.IsNullOrEmpty(iconText))
        {
            TextBlock iconBlock = new TextBlock();
            iconBlock.Text = iconText;
            iconBlock.FontSize = 16;
            iconBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor));
            iconBlock.VerticalAlignment = VerticalAlignment.Center;
            iconBlock.Margin = new Thickness(0, 0, 10, 0);
            content.Children.Add(iconBlock);
        }

        TextBlock msgBlock = new TextBlock();
        msgBlock.Text = options.Message;
        msgBlock.Foreground = Brushes.White;
        msgBlock.FontSize = 14;
        msgBlock.FontFamily = new FontFamily("Segoe UI");
        msgBlock.VerticalAlignment = VerticalAlignment.Center;
        msgBlock.TextWrapping = TextWrapping.Wrap;
        msgBlock.MaxWidth = 300;
        content.Children.Add(msgBlock);

        toast.Child = content;
        root.Children.Add(toast);

        // Animate in
        TranslateTransform trans = new TranslateTransform();
        toast.RenderTransform = trans;

        double slideFrom = -30;
        if (options.Position == PopupPosition.Bottom ||
            options.Position == PopupPosition.BottomLeft ||
            options.Position == PopupPosition.BottomRight)
            slideFrom = 30;

        DoubleAnimation slideAnim = new DoubleAnimation(slideFrom, 0, TimeSpan.FromMilliseconds(300));
        slideAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        trans.BeginAnimation(TranslateTransform.YProperty, slideAnim);

        DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        // Auto close
        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(options.DurationMs > 0 ? options.DurationMs : 3000);
        timer.Tick += delegate
        {
            timer.Stop();
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            fadeOut.Completed += delegate
            {
                if (root.Children.Contains(toast))
                    root.Children.Remove(toast);
            };
            toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            DoubleAnimation slideOut = new DoubleAnimation(0, slideFrom, TimeSpan.FromMilliseconds(300));
            slideOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            trans.BeginAnimation(TranslateTransform.YProperty, slideOut);
        };
        timer.Start();
    }));
}

private void ShowBottomSheet(BottomSheetOptions options)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // 1. Overlay
        Grid overlay = new Grid();
        if (options.ShowOverlay)
        {
            overlay.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
            Panel.SetZIndex(overlay, 50000);
        }

        // 2. Bottom Sheet Container
        Border sheet = new Border();
        sheet.VerticalAlignment = VerticalAlignment.Bottom;
        sheet.HorizontalAlignment = HorizontalAlignment.Stretch;
        Panel.SetZIndex(sheet, 50001);

        // Height
        if (options.Height.ToLower() != "auto")
        {
            sheet.Height = WpfEngine.ParseDouble(options.Height, 300);
        }

        // MaxHeight
        if (options.MaxHeight.Contains("%"))
        {
            double pct = WpfEngine.ParseDouble(options.MaxHeight.Replace("%", ""), 80) / 100.0;
            sheet.MaxHeight = mainWindow.ActualHeight * pct;
        }
        else
        {
            sheet.MaxHeight = WpfEngine.ParseDouble(options.MaxHeight, 500);
        }

        // Style
        try { sheet.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(options.Background)); }
        catch { sheet.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)); }

        // Corner Radius (faqat tepa burchaklar)
        try
        {
            string[] parts = options.CornerRadius.Split(',');
            if (parts.Length == 4)
                sheet.CornerRadius = new CornerRadius(
                    double.Parse(parts[0]), double.Parse(parts[1]),
                    double.Parse(parts[2]), double.Parse(parts[3]));
            else
                sheet.CornerRadius = new CornerRadius(16, 16, 0, 0);
        }
        catch { sheet.CornerRadius = new CornerRadius(16, 16, 0, 0); }

        // Shadow
        sheet.Effect = new DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = 30,
            ShadowDepth = 0,
            Opacity = 0.5,
            Direction = 90
        };

        // Content Container
        Grid contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // 3. Handle
        if (options.ShowHandle)
        {
            Border handleContainer = new Border();
            handleContainer.Height = 24;
            handleContainer.Background = Brushes.Transparent;
            handleContainer.Cursor = Cursors.SizeNS;

            Border handle = new Border();
            handle.Width = 40;
            handle.Height = 4;
            handle.CornerRadius = new CornerRadius(2);
            handle.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
            handle.HorizontalAlignment = HorizontalAlignment.Center;
            handle.VerticalAlignment = VerticalAlignment.Center;

            handleContainer.Child = handle;

            // Swipe to close
            if (options.SwipeToClose)
            {
                bool isDragging = false;
                Point startPoint = new Point(0, 0);

                handleContainer.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
                {
                    isDragging = true;
                    startPoint = e.GetPosition(mainWindow);
                    handleContainer.CaptureMouse();
                };

                handleContainer.MouseMove += delegate(object s, MouseEventArgs e)
                {
                    if (isDragging)
                    {
                        double delta = e.GetPosition(mainWindow).Y - startPoint.Y;
                        if (delta > 0)
                        {
                            sheet.RenderTransform = new TranslateTransform(0, delta);
                        }
                    }
                };

                handleContainer.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
                {
                    isDragging = false;
                    handleContainer.ReleaseMouseCapture();
                    double delta = e.GetPosition(mainWindow).Y - startPoint.Y;

                    if (delta > 100)
                    {
                        // Animate close
                        TranslateTransform closeTrans = new TranslateTransform(0, delta);
                        sheet.RenderTransform = closeTrans;
                        DoubleAnimation closeAnim = new DoubleAnimation(delta, 500, TimeSpan.FromMilliseconds(200));
                        closeAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                        closeAnim.Completed += delegate
                        {
                            if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                            if (rootGrid.Children.Contains(sheet)) rootGrid.Children.Remove(sheet);
                        };
                        closeTrans.BeginAnimation(TranslateTransform.YProperty, closeAnim);
                    }
                    else
                    {
                        // Bounce back
                        TranslateTransform bounceTrans = sheet.RenderTransform as TranslateTransform;
                        if (bounceTrans == null) bounceTrans = new TranslateTransform(0, delta);
                        sheet.RenderTransform = bounceTrans;
                        DoubleAnimation bounceAnim = new DoubleAnimation(delta, 0, TimeSpan.FromMilliseconds(200));
                        bounceAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                        bounceTrans.BeginAnimation(TranslateTransform.YProperty, bounceAnim);
                    }
                };
            }

            Grid.SetRow(handleContainer, 0);
            contentGrid.Children.Add(handleContainer);
        }

        // 4. Content
        if (options.ContentNode != null)
        {
            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(20, 0, 20, 20);

            TextBlock placeholder = new TextBlock();
            placeholder.Text = options.ContentNode.InnerText.Trim();
            placeholder.Foreground = Brushes.White;
            placeholder.TextWrapping = TextWrapping.Wrap;

            panel.Children.Add(placeholder);
            scroll.Content = panel;

            Grid.SetRow(scroll, 1);
            contentGrid.Children.Add(scroll);
        }

        sheet.Child = contentGrid;

        // UI ga qo'shish
        if (options.ShowOverlay)
        {
            overlay.MouseLeftButtonUp += delegate
            {
                // Animate close
                TranslateTransform closeTrans = new TranslateTransform(0, 0);
                sheet.RenderTransform = closeTrans;
                DoubleAnimation closeAnim = new DoubleAnimation(0, 500, TimeSpan.FromMilliseconds(300));
                closeAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                closeAnim.Completed += delegate
                {
                    if (rootGrid.Children.Contains(overlay)) rootGrid.Children.Remove(overlay);
                    if (rootGrid.Children.Contains(sheet)) rootGrid.Children.Remove(sheet);
                };
                closeTrans.BeginAnimation(TranslateTransform.YProperty, closeAnim);

                DoubleAnimation overlayFade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                overlay.BeginAnimation(UIElement.OpacityProperty, overlayFade);
            };
            rootGrid.Children.Add(overlay);
        }
        rootGrid.Children.Add(sheet);

        // Slide Up animatsiya
        TranslateTransform trans = new TranslateTransform(0, 500);
        sheet.RenderTransform = trans;
        DoubleAnimation anim = new DoubleAnimation(500, 0, TimeSpan.FromMilliseconds(options.DurationMs));
        anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        trans.BeginAnimation(TranslateTransform.YProperty, anim);
    }));
}

private void ShowContextMenu(ContextMenuOptions options)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // 1. Invisible Overlay (Click outside to close)
        Grid overlay = new Grid();
        overlay.Background = Brushes.Transparent;
        overlay.Name = "MenuOverlay";

        // 2. Menu Container
        Border menu = new Border();
        menu.MinWidth = 180;
        menu.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.Background));
        menu.BorderBrush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.BorderColor));
        menu.BorderThickness = new Thickness(1);
        menu.CornerRadius = new CornerRadius(options.CornerRadius);
        menu.Padding = new Thickness(0, 4, 0, 4);
        menu.HorizontalAlignment = HorizontalAlignment.Left;
        menu.VerticalAlignment = VerticalAlignment.Top;

        if (options.ShowShadow)
        {
            menu.Effect = new DropShadowEffect { 
                Color = Colors.Black, BlurRadius = 10, ShadowDepth = 2, Opacity = 0.3 
            };
        }

        // 3. Items
        StackPanel itemsPanel = new StackPanel();
        
        foreach (var item in options.Items)
        {
            if (item.IsSeparator)
            {
                Border sep = new Border();
                sep.Height = 1;
                sep.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
                sep.Margin = new Thickness(0, 4, 0, 4);
                itemsPanel.Children.Add(sep);
                continue;
            }

            Grid itemGrid = new Grid();
            itemGrid.Background = Brushes.Transparent;
            itemGrid.Cursor = Cursors.Hand;
            itemGrid.Height = 32;
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) }); // Icon
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Text
            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Shortcut

            // Icon
            if (!string.IsNullOrEmpty(item.Icon))
            {
                TextBlock icon = new TextBlock();
                icon.Text = item.Icon;
                icon.HorizontalAlignment = HorizontalAlignment.Center;
                icon.VerticalAlignment = VerticalAlignment.Center;
                icon.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(options.Foreground));
                Grid.SetColumn(icon, 0);
                itemGrid.Children.Add(icon);
            }

            // Text
            TextBlock text = new TextBlock();
            text.Text = item.Text;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.FontSize = 13;
            
            if (item.Danger) text.Foreground = Brushes.Red;
            else if (item.Disabled) text.Foreground = Brushes.Gray;
            else text.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(options.Foreground));
            
            Grid.SetColumn(text, 1);
            itemGrid.Children.Add(text);

            // Shortcut
            if (!string.IsNullOrEmpty(item.Shortcut))
            {
                TextBlock shortCut = new TextBlock();
                shortCut.Text = item.Shortcut;
                shortCut.FontSize = 11;
                shortCut.Foreground = Brushes.Gray;
                shortCut.VerticalAlignment = VerticalAlignment.Center;
                shortCut.Margin = new Thickness(0, 0, 12, 0);
                Grid.SetColumn(shortCut, 2);
                itemGrid.Children.Add(shortCut);
            }

            // Hover Effect
            itemGrid.MouseEnter += delegate { 
                if (!item.Disabled) itemGrid.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)); 
            };
            itemGrid.MouseLeave += delegate { itemGrid.Background = Brushes.Transparent; };

            // Click
            if (!item.Disabled)
            {
                itemGrid.MouseLeftButtonUp += delegate {
                    ClosePopup(mainWindow, overlay, menu);
                    if (!string.IsNullOrEmpty(item.Handler))
                    {
                        _engine.ExecuteHandler(item.Handler, options.Sender);
                    }
                };
            }

            itemsPanel.Children.Add(itemGrid);
        }

        menu.Child = itemsPanel;

        // 4. Position Calculation
        Point mousePos = Mouse.GetPosition(rootGrid);
        double left = mousePos.X;
        double top = mousePos.Y;

        // Oynadan chiqib ketmasligi uchun tekshirish
        if (left + 180 > rootGrid.ActualWidth) left = rootGrid.ActualWidth - 190;
        if (top + itemsPanel.Children.Count * 32 > rootGrid.ActualHeight) 
            top = mousePos.Y - (itemsPanel.Children.Count * 32);

        menu.Margin = new Thickness(left, top, 0, 0);

        // Close on overlay click
        overlay.MouseLeftButtonUp += delegate { ClosePopup(mainWindow, overlay, menu); };

        rootGrid.Children.Add(overlay);
        rootGrid.Children.Add(menu);

        // Fade Animation
        DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
        menu.BeginAnimation(UIElement.OpacityProperty, fade);
    }));
}
private void ExecuteClosePopup(XmlNode node)
{
    string targetName = GetAttribute(node, "Name", "");
    string targetControl = GetAttribute(node, "Control", "");

    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(new Action(delegate
    {
        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid == null) return;

        // Agar specific control berilgan bo'lsa
        if (!string.IsNullOrEmpty(targetControl))
        {
            FrameworkElement ctrl = _engine.GetControl(targetControl);
            if (ctrl != null)
            {
                ctrl.Visibility = Visibility.Collapsed;
                Panel.SetZIndex(ctrl, 0);
            }
            return;
        }

        // Oxiridan boshlab popup layerlarni topib o'chirish
        // Faqat yuqori ZIndex li elementlarni o'chirish (50000+)
        List<UIElement> toRemove = new List<UIElement>();

        for (int i = rootGrid.Children.Count - 1; i >= 0; i--)
        {
            UIElement element = rootGrid.Children[i];
            int zIndex = Panel.GetZIndex(element);

            if (zIndex >= 50000)
            {
                toRemove.Add(element);
            }
        }

        // Agar yuqori ZIndex li elementlar topilmasa, oxirgi qo'shilganlarni o'chirish
        if (toRemove.Count == 0)
        {
            for (int i = rootGrid.Children.Count - 1; i > 0; i--)
            {
                UIElement element = rootGrid.Children[i];
                FrameworkElement fe = element as FrameworkElement;

                // Asosiy content ga tegmaslik (birinchi child)
                if (fe != null)
                {
                    string name = fe.Name;
                    if (name == "popupOverlay" || name == "ConfirmOverlay" || name == "MenuOverlay")
                    {
                        toRemove.Add(element);
                    }
                    else if (i > 0 && element is Border && Panel.GetZIndex(element) > 0)
                    {
                        toRemove.Add(element);
                    }
                }
            }
        }

        foreach (UIElement el in toRemove)
        {
            if (rootGrid.Children.Contains(el))
            {
                rootGrid.Children.Remove(el);
            }
        }
    }));
}
private void ExecuteModal(XmlNode node, object sender)
{
    string name = GetAttribute(node, "Name", "modal_" + Guid.NewGuid().ToString("N").Substring(0, 6));
    string title = GetAttribute(node, "Title", "");
    string width = GetAttribute(node, "Width", "500");
    string height = GetAttribute(node, "Height", "Auto");
    string position = GetAttribute(node, "Position", "center");
    string animation = GetAttribute(node, "Animation", "scale");
    string bg = GetAttribute(node, "Background", "#17212B");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string borderColor = GetAttribute(node, "BorderColor", "#3D4D5F");
    string cornerRadius = GetAttribute(node, "CornerRadius", "16");
    string showClose = GetAttribute(node, "ShowClose", "true");
    string overlay = GetAttribute(node, "Overlay", "true");
    string overlayClose = GetAttribute(node, "OverlayClose", "true");
    string draggable = GetAttribute(node, "Draggable", "true");
    string resizable = GetAttribute(node, "Resizable", "false");

    title = ReplaceStateVariables(title);

    // Content XML dan olish
    XmlNode contentNode = node.SelectSingleNode("Content");
    if (contentNode == null) contentNode = node;

    ShowModal(new ModalOptions
    {
        Name = name,
        Title = title,
        Width = WpfEngine.ParseDouble(width, 500),
        Height = height == "Auto" ? double.NaN : WpfEngine.ParseDouble(height, 300),
        Position = ParsePopupPosition(position),
        Animation = ParsePopupAnimation(animation),
        Background = bg,
        Foreground = fg,
        BorderColor = borderColor,
        CornerRadius = WpfEngine.ParseDouble(cornerRadius, 16),
        ShowCloseButton = showClose.ToLower() == "true",
        ShowOverlay = overlay.ToLower() == "true",
        CloseOnOverlayClick = overlayClose.ToLower() == "true",
        Draggable = draggable.ToLower() == "true",
        Resizable = resizable.ToLower() == "true",
        ContentNode = contentNode,
        Sender = sender
    });
}
private void ExecuteDrawer(XmlNode node, object sender)
{
    string name = GetAttribute(node, "Name", "drawer");
    string side = GetAttribute(node, "Side", "right"); // left, right, top, bottom
    string width = GetAttribute(node, "Width", "320");
    string height = GetAttribute(node, "Height", "100%");
    string bg = GetAttribute(node, "Background", "#17212B");
    string animation = GetAttribute(node, "Animation", "slide");
    string duration = GetAttribute(node, "Duration", "300");
    string overlay = GetAttribute(node, "Overlay", "true");
    string overlayClose = GetAttribute(node, "OverlayClose", "true");

    XmlNode contentNode = node.SelectSingleNode("Content");
    if (contentNode == null) contentNode = node;

    ShowDrawer(new DrawerOptions
    {
        Name = name,
        Side = ParseDrawerSide(side),
        Width = WpfEngine.ParseDouble(width, 320),
        Height = height,
        Background = bg,
        Animation = ParsePopupAnimation(animation),
        DurationMs = WpfEngine.ParseInt(duration, 300),
        ShowOverlay = overlay.ToLower() == "true",
        CloseOnOverlayClick = overlayClose.ToLower() == "true",
        ContentNode = contentNode,
        Sender = sender
    });
}
private PopupType ParsePopupType(string type)
{
    switch (type.ToLower())
    {
        case "confirm": return PopupType.Confirm;
        case "modal": return PopupType.Modal;
        case "drawer": return PopupType.Drawer;
        case "toast": return PopupType.Toast;
        case "bottomsheet": return PopupType.BottomSheet;
        case "contextmenu": return PopupType.ContextMenu;
        case "tooltip": return PopupType.Tooltip;
        default: return PopupType.Alert;
    }
}

private PopupPosition ParsePopupPosition(string position)
{
    switch (position.ToLower())
    {
        case "top": return PopupPosition.Top;
        case "bottom": return PopupPosition.Bottom;
        case "left": return PopupPosition.Left;
        case "right": return PopupPosition.Right;
        case "topleft": return PopupPosition.TopLeft;
        case "topright": return PopupPosition.TopRight;
        case "bottomleft": return PopupPosition.BottomLeft;
        case "bottomright": return PopupPosition.BottomRight;
        case "mouse": return PopupPosition.Mouse;
        default: return PopupPosition.Center;
    }
}

private PopupAnimation ParsePopupAnimation(string animation)
{
    switch (animation.ToLower())
    {
        case "none": return PopupAnimation.None;
        case "slideup": return PopupAnimation.SlideUp;
        case "slidedown": return PopupAnimation.SlideDown;
        case "slideleft": return PopupAnimation.SlideLeft;
        case "slideright": return PopupAnimation.SlideRight;
        case "scale": return PopupAnimation.Scale;
        case "bounce": return PopupAnimation.Bounce;
        default: return PopupAnimation.Fade;
    }
}

private DrawerSide ParseDrawerSide(string side)
{
    switch (side.ToLower())
    {
        case "left": return DrawerSide.Left;
        case "top": return DrawerSide.Top;
        case "bottom": return DrawerSide.Bottom;
        default: return DrawerSide.Right;
    }
}
private void ExecuteToast(XmlNode node)
{
    string message = GetAttribute(node, "Message", node.InnerText);
    string type = GetAttribute(node, "Type", "info"); // info, success, warning, error
    string position = GetAttribute(node, "Position", "topright");
    string duration = GetAttribute(node, "Duration", "3000");
    string icon = GetAttribute(node, "Icon", "");
    string animation = GetAttribute(node, "Animation", "slideLeft");

    message = ReplaceStateVariables(message);
    message = ParseControlProperty(message);

    // Auto icon based on type
    if (string.IsNullOrEmpty(icon))
    {
        switch (type.ToLower())
        {
            case "success": icon = "✓"; break;
            case "error": icon = "✕"; break;
            case "warning": icon = "⚠"; break;
            default: icon = "ℹ"; break;
        }
    }

    ShowToast(new ToastOptions
    {
        Message = message,
        Type = type,
        Icon = icon,
        Position = ParsePopupPosition(position),
        DurationMs = WpfEngine.ParseInt(duration, 3000),
        Animation = ParsePopupAnimation(animation)
    });
}

private void ShowCustomConfirm(ConfirmOptions options)
{
    Window mainWindow = _engine.GetMainWindow();
    if (mainWindow == null) return;

    mainWindow.Dispatcher.Invoke(delegate
    {
        Grid overlay = new Grid();
        overlay.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        Panel.SetZIndex(overlay, 50000);

        Border popup = new Border();
        popup.Width = 400;
        popup.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(options.Background));
        popup.BorderBrush = new SolidColorBrush(Color.FromRgb(61, 61, 61));
        popup.BorderThickness = new Thickness(1);
        popup.CornerRadius = new CornerRadius(16);
        popup.HorizontalAlignment = HorizontalAlignment.Center;
        popup.VerticalAlignment = VerticalAlignment.Center;
        Panel.SetZIndex(popup, 50001);
        popup.Effect = new DropShadowEffect
        {
            Color = Colors.Black, BlurRadius = 30, ShadowDepth = 0, Opacity = 0.5
        };

        StackPanel content = new StackPanel();
        content.Margin = new Thickness(24);

        if (!string.IsNullOrEmpty(options.Icon))
        {
            TextBlock iconText = new TextBlock();
            iconText.Text = options.Icon; iconText.FontSize = 48;
            iconText.HorizontalAlignment = HorizontalAlignment.Center;
            iconText.Margin = new Thickness(0, 0, 0, 16);
            content.Children.Add(iconText);
        }

        if (!string.IsNullOrEmpty(options.Title))
        {
            TextBlock titleText = new TextBlock();
            titleText.Text = options.Title; titleText.FontSize = 18;
            titleText.FontWeight = FontWeights.SemiBold;
            titleText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(options.Foreground));
            titleText.HorizontalAlignment = HorizontalAlignment.Center;
            titleText.Margin = new Thickness(0, 0, 0, 8);
            content.Children.Add(titleText);
        }

        TextBlock messageText = new TextBlock();
        messageText.Text = options.Message.Replace("\\n", "\n");
        messageText.FontSize = 15;
        messageText.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
        messageText.TextWrapping = TextWrapping.Wrap;
        messageText.TextAlignment = TextAlignment.Center;
        messageText.Margin = new Thickness(0, 0, 0, 24);
        content.Children.Add(messageText);

        StackPanel buttons = new StackPanel();
        buttons.Orientation = Orientation.Horizontal;
        buttons.HorizontalAlignment = HorizontalAlignment.Center;

        Border noBtn = CreateButton(options.NoText, options.NoBackground, "#FFFFFF");
        noBtn.Margin = new Thickness(0, 0, 8, 0);
        noBtn.MouseLeftButtonUp += delegate
        {
            ClosePopup(mainWindow, overlay, popup);
            if (options.OnNo != null) options.OnNo.Invoke();
        };
        buttons.Children.Add(noBtn);

        Border yesBtn = CreateButton(options.YesText, options.YesBackground, "#FFFFFF");
        yesBtn.MouseLeftButtonUp += delegate
        {
            ClosePopup(mainWindow, overlay, popup);
            if (options.OnYes != null) options.OnYes.Invoke();
        };
        buttons.Children.Add(yesBtn);

        content.Children.Add(buttons);
        popup.Child = content;
        if (options.Draggable) MakeDraggable(popup);

        Grid rootGrid = FindRootGrid(mainWindow);
        if (rootGrid != null)
        {
            rootGrid.Children.Add(overlay);
            rootGrid.Children.Add(popup);
            ApplyPopupAnimation(popup, options.Animation, true);
        }
    });
}
private void ExecuteBottomSheet(XmlNode node, object sender)
{
    string name = GetAttribute(node, "Name", "bottomsheet");
    string height = GetAttribute(node, "Height", "Auto");
    string maxHeight = GetAttribute(node, "MaxHeight", "80%");
    string bg = GetAttribute(node, "Background", "#17212B");
    string cornerRadius = GetAttribute(node, "CornerRadius", "20,20,0,0");
    string showHandle = GetAttribute(node, "ShowHandle", "true");
    string animation = GetAttribute(node, "Animation", "slideUp");
    string duration = GetAttribute(node, "Duration", "300");
    string overlay = GetAttribute(node, "Overlay", "true");
    string swipeToClose = GetAttribute(node, "SwipeToClose", "true");

    XmlNode contentNode = node.SelectSingleNode("Content");
    if (contentNode == null) contentNode = node;

    ShowBottomSheet(new BottomSheetOptions
    {
        Name = name,
        Height = height,
        MaxHeight = maxHeight,
        Background = bg,
        CornerRadius = cornerRadius,
        ShowHandle = showHandle.ToLower() == "true",
        Animation = ParsePopupAnimation(animation),
        DurationMs = WpfEngine.ParseInt(duration, 300),
        ShowOverlay = overlay.ToLower() == "true",
        SwipeToClose = swipeToClose.ToLower() == "true",
        ContentNode = contentNode,
        Sender = sender
    });
}
private void ExecuteContextMenu(XmlNode node, object sender)
{
    string position = GetAttribute(node, "Position", "mouse");
    string style = GetAttribute(node, "Style", "auto"); // auto, ios, windows
    string bg = GetAttribute(node, "Background", "#2C2C2E");
    string fg = GetAttribute(node, "Foreground", "#FFFFFF");
    string borderColor = GetAttribute(node, "BorderColor", "#3D4D5F");
    string cornerRadius = GetAttribute(node, "CornerRadius", "8");
    string animation = GetAttribute(node, "Animation", "fade");
    string shadow = GetAttribute(node, "Shadow", "true");

    List<ContextMenuItem> items = new List<ContextMenuItem>();

    foreach (XmlNode child in node.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;

        if (child.Name == "Item" || child.Name == "MenuItem")
        {
            items.Add(new ContextMenuItem
            {
                Text = GetAttribute(child, "Text", GetAttribute(child, "Header", "")),
                Icon = GetAttribute(child, "Icon", ""),
                Shortcut = GetAttribute(child, "Shortcut", ""),
                Handler = GetAttribute(child, "onClick", GetAttribute(child, "Handler", "")),
                Disabled = GetAttribute(child, "Disabled", "false").ToLower() == "true",
                Danger = GetAttribute(child, "Danger", "false").ToLower() == "true"
            });
        }
        else if (child.Name == "Separator" || child.Name == "Divider")
        {
            items.Add(new ContextMenuItem { IsSeparator = true });
        }
    }

    // iOS style (bottom action sheet) yoki position=bottom bo'lsa
    if (style.ToLower() == "ios" || position.ToLower() == "bottom")
    {
        ShowNativeContextMenu("", items, sender, 0);
        return;
    }

    // Default Windows style context menu
    ShowContextMenu(new ContextMenuOptions
    {
        Position = ParsePopupPosition(position),
        Background = bg,
        Foreground = fg,
        BorderColor = borderColor,
        CornerRadius = WpfEngine.ParseDouble(cornerRadius, 8),
        Animation = ParsePopupAnimation(animation),
        ShowShadow = shadow.ToLower() == "true",
        Items = items,
        Sender = sender
    });
}
        
        private void ExecuteClose(XmlNode node)
        {
            string windowName = GetAttribute(node, "Window", "");
            
            if (!string.IsNullOrEmpty(windowName) && _engine.Windows.ContainsKey(windowName))
            {
                _engine.Windows[windowName].Close();
            }
            else
            {
                foreach (KeyValuePair<string, Window> kvp in _engine.Windows)
                {
                    kvp.Value.Close();
                    break;
                }
            }
        }
        
        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }
        
        #endregion
        
        #region Text Commands
        
        private void ExecuteAppendText(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            string text = GetAttribute(node, "Text", "");
            
            text = ReplaceStateVariables(text);
            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                if (control is TextBox)
                {
                    TextBox textBox = (TextBox)control;
                    textBox.Text = textBox.Text + text;
                }
                else if (control is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)control;
                    textBlock.Text = textBlock.Text + text;
                }
            }
        }
        
        private void ExecuteClearText(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                else if (control is TextBlock)
                {
                    ((TextBlock)control).Text = "";
                }
            }
        }
        
        private void ExecuteCalculate(XmlNode node)
        {
            string expression = GetAttribute(node, "Expression", "");
            string toState = GetAttribute(node, "ToState", "");
            string toControl = GetAttribute(node, "ToControl", "");
            string toProperty = GetAttribute(node, "ToProperty", "Text");
            
            try
            {
                expression = ReplaceStateVariables(expression);
                double result = EvaluateExpression(expression);
                
                if (!string.IsNullOrEmpty(toState))
{
    _engine.SetVariable(toState, result);
}
                
                if (!string.IsNullOrEmpty(toControl))
                {
                    _engine.SetControlProperty(toControl, toProperty, result.ToString());
                }
            }
            catch { }
        }
        
        private double EvaluateExpression(string expression)
{
    expression = expression.Trim();
    
    // Handle parentheses first (basic)
    while (expression.Contains("("))
    {
        int start = expression.LastIndexOf('(');
        int end = expression.IndexOf(')', start);
        if (end > start)
        {
            string inner = expression.Substring(start + 1, end - start - 1);
            double innerResult = EvaluateExpression(inner);
            expression = expression.Substring(0, start) + innerResult.ToString(System.Globalization.CultureInfo.InvariantCulture) + expression.Substring(end + 1);
        }
        else break;
    }
    
    // Addition and subtraction (left to right, lowest precedence)
    int addIdx = FindOperatorIndex(expression, '+');
    int subIdx = FindOperatorIndex(expression, '-');
    
    // Take the RIGHTMOST + or - (for left-to-right evaluation of same precedence)
    if (addIdx > 0 || subIdx > 0)
    {
        int opIdx = Math.Max(addIdx, subIdx);
        char op = expression[opIdx];
        
        string left = expression.Substring(0, opIdx);
        string right = expression.Substring(opIdx + 1);
        
        double leftVal = EvaluateExpression(left);
        double rightVal = EvaluateExpression(right);
        
        return op == '+' ? leftVal + rightVal : leftVal - rightVal;
    }
    
    // Multiplication and division (higher precedence)
    int mulIdx = expression.LastIndexOf('*');
    int divIdx = expression.LastIndexOf('/');
    
    if (mulIdx > 0 || divIdx > 0)
    {
        int opIdx = Math.Max(mulIdx, divIdx);
        char op = expression[opIdx];
        
        string left = expression.Substring(0, opIdx);
        string right = expression.Substring(opIdx + 1);
        
        double leftVal = EvaluateExpression(left);
        double rightVal = EvaluateExpression(right);
        
        if (op == '*') return leftVal * rightVal;
        if (op == '/' && rightVal != 0) return leftVal / rightVal;
        return 0;
    }
    
    // Base case: just a number
    double result;
    if (double.TryParse(expression.Trim(), System.Globalization.NumberStyles.Any, 
        System.Globalization.CultureInfo.InvariantCulture, out result))
    {
        return result;
    }
    
    return 0;
}

private int FindOperatorIndex(string expr, char op)
{
    // Find rightmost operator that's not inside nested expression
    int depth = 0;
    for (int i = expr.Length - 1; i >= 0; i--)
    {
        char c = expr[i];
        if (c == ')') depth++;
        else if (c == '(') depth--;
        else if (c == op && depth == 0 && i > 0)
        {
            // Make sure it's not a negative sign
            if (op == '-')
            {
                char prev = expr[i - 1];
                if (prev == '+' || prev == '-' || prev == '*' || prev == '/' || prev == '(')
                    continue;
            }
            return i;
        }
    }
    return -1;
}
        
        #endregion
        
        #region Effect Commands
        
        private void ExecuteDropShadow(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            string color = GetAttribute(node, "Color", "Black");
            double blurRadius = double.Parse(GetAttribute(node, "BlurRadius", "10"));
            double shadowDepth = double.Parse(GetAttribute(node, "ShadowDepth", "3"));
            double opacity = WpfEngine.ParseDouble(GetAttribute(node, "Opacity", "0.5"), 0.5);
            bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
            int duration = int.Parse(GetAttribute(node, "Duration", "300"));
            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control == null) return;
            
            DropShadowEffect shadow = new DropShadowEffect();
            try { shadow.Color = (Color)ColorConverter.ConvertFromString(color); }
            catch { shadow.Color = Colors.Black; }
            shadow.BlurRadius = animate ? 0 : blurRadius;
            shadow.ShadowDepth = shadowDepth;
            shadow.Opacity = opacity;
            
            control.Effect = shadow;
            
            if (animate)
            {
                DoubleAnimation blurAnim = new DoubleAnimation();
                blurAnim.From = 0;
                blurAnim.To = blurRadius;
                blurAnim.Duration = TimeSpan.FromMilliseconds(duration);
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
            }
        }
        
        private void ExecuteBlurEffect(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            double radius = double.Parse(GetAttribute(node, "Radius", "5"));
            bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
            int duration = WpfEngine.ParseInt(GetAttribute(node, "Duration", "300"), 300);            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control == null) return;
            
            BlurEffect blur = new BlurEffect();
            blur.Radius = animate ? 0 : radius;
            control.Effect = blur;
            
            if (animate)
            {
                DoubleAnimation radiusAnim = new DoubleAnimation();
                radiusAnim.From = 0;
                radiusAnim.To = radius;
                radiusAnim.Duration = TimeSpan.FromMilliseconds(duration);
                blur.BeginAnimation(BlurEffect.RadiusProperty, radiusAnim);
            }
        }
        
        private void ExecuteOpacity(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            double value = double.Parse(GetAttribute(node, "Value", "1"));
            bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
            int duration = int.Parse(GetAttribute(node, "Duration", "300"));
            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control == null) return;
            
            if (animate)
            {
                DoubleAnimation opacityAnim = new DoubleAnimation();
                opacityAnim.To = value;
                opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
                control.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            }
            else
            {
                control.Opacity = value;
            }
        }
        
        private void ExecuteGlow(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            string color = GetAttribute(node, "Color", "#00D4FF");
            double intensity = double.Parse(GetAttribute(node, "Intensity", "15"));
            double opacity = double.Parse(GetAttribute(node, "Opacity", "1"));
            bool animate = GetAttribute(node, "Animate", "false").ToLower() == "true";
            int duration = int.Parse(GetAttribute(node, "Duration", "300"));
            
            FrameworkElement control = _engine.GetControl(controlName);
            if (control == null) return;
            
            DropShadowEffect glow = new DropShadowEffect();
            try { glow.Color = (Color)ColorConverter.ConvertFromString(color); }
            catch { glow.Color = Color.FromRgb(0, 212, 255); }
            glow.BlurRadius = animate ? 0 : intensity;
            glow.ShadowDepth = 0;
            glow.Opacity = opacity;
            
            control.Effect = glow;
            
            if (animate)
            {
                DoubleAnimation glowAnim = new DoubleAnimation();
                glowAnim.From = 0;
                glowAnim.To = intensity;
                glowAnim.Duration = TimeSpan.FromMilliseconds(duration);
                glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
            }
        }
        
        private void ExecuteClearEffect(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            if (control != null)
            {
                control.Effect = null;
            }
        }
        
        private void ExecuteAnimate(XmlNode node)
{
    string controlName = GetAttribute(node, "Control", "");
    string property = GetAttribute(node, "Property", "Opacity");
    string fromStr = GetAttribute(node, "From", "");
    string toStr = GetAttribute(node, "To", "1");
    string durationStr = GetAttribute(node, "Duration", "300");
    string easing = GetAttribute(node, "Easing", "CubicEaseOut");
    string autoReverse = GetAttribute(node, "AutoReverse", "false");
    string repeatStr = GetAttribute(node, "Repeat", "1");

    int duration = 300;
    int.TryParse(durationStr, out duration);

    FrameworkElement control = _engine.GetControl(controlName);
    if (control == null) return;

    double toValue = 1;
    double.TryParse(toStr, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out toValue);

    double? fromValue = null;
    if (!string.IsNullOrEmpty(fromStr))
    {
        double fv;
        if (double.TryParse(fromStr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out fv))
        {
            fromValue = fv;
        }
    }

    IEasingFunction easingFunc = null;
    switch (easing.ToLower())
    {
        case "cubiceaseout":
            easingFunc = new CubicEase { EasingMode = EasingMode.EaseOut };
            break;
        case "cubiceasein":
            easingFunc = new CubicEase { EasingMode = EasingMode.EaseIn };
            break;
        case "cubiceaseinout":
            easingFunc = new CubicEase { EasingMode = EasingMode.EaseInOut };
            break;
        case "bounce":
        case "bounceease":
            easingFunc = new BounceEase { Bounces = 2, Bounciness = 2 };
            break;
        case "elastic":
        case "elasticease":
            easingFunc = new ElasticEase { Oscillations = 1, Springiness = 3 };
            break;
        case "back":
        case "backease":
            easingFunc = new BackEase { Amplitude = 0.3 };
            break;
    }

    DoubleAnimation animation = new DoubleAnimation();
    if (fromValue.HasValue) animation.From = fromValue.Value;
    animation.To = toValue;
    animation.Duration = TimeSpan.FromMilliseconds(duration);
    if (easingFunc != null) animation.EasingFunction = easingFunc;
    animation.AutoReverse = autoReverse.ToLower() == "true";

    int repeat = 1;
    int.TryParse(repeatStr, out repeat);
    if (repeat > 1) animation.RepeatBehavior = new RepeatBehavior(repeat);
    if (repeatStr.ToLower() == "forever") animation.RepeatBehavior = RepeatBehavior.Forever;

    switch (property.ToLower())
    {
        case "opacity":
            control.BeginAnimation(UIElement.OpacityProperty, animation);
            break;
        case "width":
            control.BeginAnimation(FrameworkElement.WidthProperty, animation);
            break;
        case "height":
            control.BeginAnimation(FrameworkElement.HeightProperty, animation);
            break;
        case "scale":
        case "scalex":
        case "scaley":
        case "zoom":
            // Scale transform
            ScaleTransform scale = control.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(1, 1);
                control.RenderTransform = scale;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            if (property.ToLower() == "scalex")
            {
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            }
            else if (property.ToLower() == "scaley")
            {
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
            else
            {
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
            break;
        case "rotate":
        case "rotation":
            RotateTransform rotate = control.RenderTransform as RotateTransform;
            if (rotate == null)
            {
                rotate = new RotateTransform(0);
                control.RenderTransform = rotate;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
            break;
        case "translatex":
        case "x":
            TranslateTransform transX = control.RenderTransform as TranslateTransform;
            if (transX == null)
            {
                transX = new TranslateTransform(0, 0);
                control.RenderTransform = transX;
            }
            transX.BeginAnimation(TranslateTransform.XProperty, animation);
            break;
        case "translatey":
        case "y":
            TranslateTransform transY = control.RenderTransform as TranslateTransform;
            if (transY == null)
            {
                transY = new TranslateTransform(0, 0);
                control.RenderTransform = transY;
            }
            transY.BeginAnimation(TranslateTransform.YProperty, animation);
            break;
    }
}
        
        #endregion
        
        #region Item Commands
        
        private void ExecuteAddMessage(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string text = GetAttribute(node, "Text", "");
            string senderType = GetAttribute(node, "Sender", "me");
            string time = GetAttribute(node, "Time", DateTime.Now.ToString("HH:mm"));
            
            text = ReplaceStateVariables(text);
            text = ParseControlProperty(text);
            
            if (string.IsNullOrWhiteSpace(text)) return;
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Panel messagesPanel = GetTargetPanel(container);
            if (messagesPanel == null) return;
            
            Border bubble = CreateChatBubble(text, time, senderType == "me");
            messagesPanel.Children.Add(bubble);
            
            if (container is ScrollViewer)
            {
                ((ScrollViewer)container).ScrollToEnd();
            }
        }
        
        private void ExecuteAddItem(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            string type = GetAttribute(node, "Type", "TextBlock");
            string text = GetAttribute(node, "Text", "");
            
            text = ReplaceStateVariables(text);
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Panel targetPanel = GetTargetPanel(container);
            if (targetPanel == null) return;
            
            FrameworkElement newElement = null;
            
            switch (type.ToLower())
            {
                case "textblock":
                    TextBlock tb = new TextBlock();
                    tb.Text = text;
                    tb.Foreground = Brushes.White;
                    tb.FontSize = 14;
                    tb.Margin = new Thickness(5);
                    newElement = tb;
                    break;
                    
                case "button":
                    Button btn = new Button();
                    btn.Content = text;
                    btn.Margin = new Thickness(5);
                    newElement = btn;
                    break;
                    
                case "card":
                case "border":
                    Border brd = new Border();
                    string bgColor = GetAttribute(node, "Background", "#2D2D30");
                    try { brd.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)); }
                    catch { brd.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); }
                    brd.CornerRadius = new CornerRadius(10);
                    brd.Padding = new Thickness(15);
                    brd.Margin = new Thickness(5);
                    
                    TextBlock innerText = new TextBlock();
                    innerText.Text = text;
                    innerText.Foreground = Brushes.White;
                    innerText.TextWrapping = TextWrapping.Wrap;
                    brd.Child = innerText;
                    newElement = brd;
                    break;
            }
            
            if (newElement != null)
            {
                targetPanel.Children.Add(newElement);
                
                if (container is ScrollViewer)
                {
                    ((ScrollViewer)container).ScrollToEnd();
                }
            }
        }
        
        private void ExecuteRemoveItem(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            int index = int.Parse(GetAttribute(node, "Index", "-1"));
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Panel targetPanel = GetTargetPanel(container);
            if (targetPanel == null) return;
            
            if (index == -1)
            {
                if (targetPanel.Children.Count > 0)
                {
                    targetPanel.Children.RemoveAt(targetPanel.Children.Count - 1);
                }
            }
            else if (index >= 0 && index < targetPanel.Children.Count)
            {
                targetPanel.Children.RemoveAt(index);
            }
        }
        
        private void ExecuteClearItems(XmlNode node)
        {
            string containerName = GetAttribute(node, "Container", "");
            
            FrameworkElement container = _engine.GetControl(containerName);
            if (container == null) return;
            
            Panel targetPanel = GetTargetPanel(container);
            if (targetPanel != null)
            {
                targetPanel.Children.Clear();
            }
        }
        
        private Panel GetTargetPanel(FrameworkElement container)
        {
            if (container is ScrollViewer)
            {
                ScrollViewer sv = (ScrollViewer)container;
                if (sv.Content is Panel)
                {
                    return (Panel)sv.Content;
                }
            }
            else if (container is Panel)
            {
                return (Panel)container;
            }
            return null;
        }
        
        private Border CreateChatBubble(string text, string time, bool isMe)
        {
            Border bubble = new Border();
            bubble.CornerRadius = isMe ? new CornerRadius(15, 15, 0, 15) : new CornerRadius(15, 15, 15, 0);
            bubble.Padding = new Thickness(12, 8, 12, 8);
            bubble.Margin = new Thickness(isMe ? 80 : 0, 5, isMe ? 0 : 80, 5);
            bubble.MaxWidth = 350;
            bubble.HorizontalAlignment = isMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            
            bubble.Background = isMe ? 
                new SolidColorBrush(Color.FromRgb(0, 120, 212)) : 
                new SolidColorBrush(Color.FromRgb(60, 60, 60));
            
            StackPanel content = new StackPanel();
            
            TextBlock msgText = new TextBlock();
            msgText.Text = text;
            msgText.FontSize = 14;
            msgText.Foreground = Brushes.White;
            msgText.TextWrapping = TextWrapping.Wrap;
            content.Children.Add(msgText);
            
            TextBlock timeText = new TextBlock();
            timeText.Text = time;
            timeText.FontSize = 10;
            timeText.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            timeText.HorizontalAlignment = HorizontalAlignment.Right;
            timeText.Margin = new Thickness(0, 5, 0, 0);
            content.Children.Add(timeText);
            
            bubble.Child = content;
            return bubble;
        }
        
        #endregion
        
        #region Scroll Commands
        
        private void ExecuteScrollToBottom(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            
            if (control is ScrollViewer)
            {
                ((ScrollViewer)control).ScrollToEnd();
            }
        }
        
        private void ExecuteScrollToTop(XmlNode node)
        {
            string controlName = GetAttribute(node, "Control", "");
            FrameworkElement control = _engine.GetControl(controlName);
            
            if (control is ScrollViewer)
            {
                ((ScrollViewer)control).ScrollToHome();
            }
        }
        
        #endregion
        
        #region HTTP Command
        
        private void ExecuteHttpRequest(XmlNode node)
{
    string method = GetAttribute(node, "Method", "GET").ToUpper();
    string url = GetAttribute(node, "Url", "");
    string body = GetAttribute(node, "Body", "");
    string toState = GetAttribute(node, "ToState", "");
    string toControl = GetAttribute(node, "ToControl", "");
    string toProperty = GetAttribute(node, "ToProperty", "Text");

    if (string.IsNullOrEmpty(url)) return;

    url = ReplaceStateVariables(url);
    body = ReplaceStateVariables(body);

    _engine.Log("HTTP", method + " " + url);

    try
    {
        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        request.Method = method;
        request.ContentType = "application/json";
        request.Timeout = 30000;
        request.UserAgent = "NimbusEngine/3.0";

        if ((method == "POST" || method == "PUT") && !string.IsNullOrEmpty(body))
        {
            byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(body);
            request.ContentLength = bodyBytes.Length;

            using (System.IO.Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            }
        }

        string responseText = "";
        using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
        using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
        {
            responseText = reader.ReadToEnd();
        }

        _engine.Log("HTTP", "Response: " + responseText.Length + " chars");

        if (!string.IsNullOrEmpty(toState))
        {
            _engine.SetVariable(toState, responseText);
        }

        if (!string.IsNullOrEmpty(toControl))
        {
            _engine.SetControlProperty(toControl, toProperty, responseText);
        }
    }
    catch (System.Net.WebException webEx)
    {
        string errorBody = "";
        if (webEx.Response != null)
        {
            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                {
                    errorBody = reader.ReadToEnd();
                }
            }
            catch { }
        }

        string errorMsg = webEx.Message;
        _engine.Log("ERROR", "HTTP failed: " + errorMsg);
        
        if (!string.IsNullOrEmpty(toState))
        {
            // Clear state on error to avoid false positives
            _engine.SetVariable(toState, "");
        }
    }
    catch (Exception ex)
    {
        _engine.Log("ERROR", "HTTP error: " + ex.Message);
        
        if (!string.IsNullOrEmpty(toState))
        {
            _engine.SetVariable(toState, "");
        }
    }
}
        
        #endregion
        
        #region File & Clipboard Commands
        private void ExecuteTimer(XmlNode node)
{
    string name = GetAttribute(node, "Name", "timer_" + Guid.NewGuid().ToString("N").Substring(0, 6));
    string intervalStr = GetAttribute(node, "Interval", "1000");
    string handler = GetAttribute(node, "Handler", "");
    string repeatStr = GetAttribute(node, "Repeat", "true");

    if (string.IsNullOrEmpty(handler)) return;

    int interval = 1000;
    int.TryParse(intervalStr, out interval);

    bool repeat = repeatStr.ToLower() != "false" && repeatStr != "0";

    _engine.CreateTimer(name, interval, handler, repeat);
}
        private void ExecuteSaveFile(XmlNode node)
        {
            string content = GetAttribute(node, "Content", "");
            string fromControl = GetAttribute(node, "FromControl", "");
            
            if (!string.IsNullOrEmpty(fromControl))
            {
                object value = _engine.GetControlProperty(fromControl, "Text");
                content = value != null ? value.ToString() : "";
            }
            
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, content);
            }
        }
        
        private void ExecuteOpenFile(XmlNode node)
        {
            string toControl = GetAttribute(node, "ToControl", "");
            string toState = GetAttribute(node, "ToState", "");
            string filter = GetAttribute(node, "Filter", "Text Files|*.txt|All Files|*.*");
            
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = filter;
            
            if (dialog.ShowDialog() == true)
            {
                string content = File.ReadAllText(dialog.FileName);
                
                if (!string.IsNullOrEmpty(toControl))
                {
                    _engine.SetControlProperty(toControl, "Text", content);
                }
                
                if (!string.IsNullOrEmpty(toState))
                {
                    _engine.SetVariable(toState, content);
                }
            }
        }
        
        private void ExecuteCopy(XmlNode node)
        {
            string text = GetAttribute(node, "Text", "");
            string fromControl = GetAttribute(node, "FromControl", "");
            
            if (!string.IsNullOrEmpty(fromControl))
            {
                object value = _engine.GetControlProperty(fromControl, "Text");
                text = value != null ? value.ToString() : "";
            }
            else
            {
                text = ReplaceStateVariables(text);
            }
            
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }
        
        private void ExecutePaste(XmlNode node)
        {
            string toControl = GetAttribute(node, "ToControl", "");
            string toState = GetAttribute(node, "ToState", "");
            
            string text = Clipboard.GetText();
            
            if (!string.IsNullOrEmpty(toControl))
            {
                _engine.SetControlProperty(toControl, "Text", text);
            }
            
            if (!string.IsNullOrEmpty(toState))
            {
                _engine.SetVariable(toState, text);
            }
        }
        
        #endregion
        
        #region Other Commands
        
        private void ExecuteCall(XmlNode node, object sender)
        {
            string handler = GetAttribute(node, "Handler", "");
            if (!string.IsNullOrEmpty(handler))
            {
                _engine.ExecuteHandler(handler, sender);
            }
        }
        
        private void ExecuteDelay(XmlNode node)
{
    int milliseconds = WpfEngine.ParseInt(GetAttribute(node, "Milliseconds", "100"), 100);
    
    // UI thread ni to'xtatmaydigan delay
    // Dispatcher Frame ishlatamiz
    DateTime endTime = DateTime.Now.AddMilliseconds(milliseconds);
    
    System.Windows.Threading.DispatcherFrame frame = new System.Windows.Threading.DispatcherFrame();
    
    new System.Threading.Thread(delegate()
    {
        System.Threading.Thread.Sleep(milliseconds);
        frame.Continue = false;
    }).Start();
    
    System.Windows.Threading.Dispatcher.PushFrame(frame);
}
        
        private void ExecutePrint(XmlNode node)
        {
            string text = GetAttribute(node, "Text", node.InnerText);
            text = ReplaceStateVariables(text);
            Console.WriteLine(text);
        }
        
        private void ExecuteDebug(XmlNode node)
        {
            string message = GetAttribute(node, "Message", node.InnerText);
            string level = GetAttribute(node, "Level", "Info");
            
            message = ReplaceStateVariables(message);
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine("[{0}] [{1}] {2}", timestamp, level, message);
        }
        
        #endregion
        
        #region Helper Methods
        
        protected bool EvaluateCondition(string condition)
{
    condition = ReplaceStateVariables(condition);

    // XML-safe operators ni convert qilish
    condition = condition.Replace("&amp;&amp;", "&&");
    condition = condition.Replace("&lt;", "<");
    condition = condition.Replace("&gt;", ">");
    condition = condition.Replace("&amp;", "&");
    condition = condition.Replace(" AND ", " && ");
    condition = condition.Replace(" OR ", " || ");
    condition = condition.Replace(" and ", " && ");
    condition = condition.Replace(" or ", " || ");
    condition = condition.Replace(" EQUALS ", " == ");
    condition = condition.Replace(" NOT ", " != ");
    condition = condition.Replace(" eq ", " == ");
    condition = condition.Replace(" ne ", " != ");
    condition = condition.Replace(" gt ", " > ");
    condition = condition.Replace(" lt ", " < ");
    condition = condition.Replace(" gte ", " >= ");
    condition = condition.Replace(" lte ", " <= ");

    // AND (&&) operator - ikki shart
    if (condition.Contains("&&"))
    {
        string[] andParts = condition.Split(new string[] { "&&" }, StringSplitOptions.None);
        bool allTrue = true;
        foreach (string part in andParts)
        {
            if (!EvaluateSingleCondition(part.Trim()))
            {
                allTrue = false;
                break;
            }
        }
        return allTrue;
    }

    // OR (||) operator
    if (condition.Contains("||"))
    {
        string[] orParts = condition.Split(new string[] { "||" }, StringSplitOptions.None);
        foreach (string part in orParts)
        {
            if (EvaluateSingleCondition(part.Trim()))
            {
                return true;
            }
        }
        return false;
    }

    // NOT (!) operator
    if (condition.TrimStart().StartsWith("!"))
    {
        string inner = condition.TrimStart().Substring(1).Trim();
        return !EvaluateSingleCondition(inner);
    }

    return EvaluateSingleCondition(condition);
}

private bool EvaluateSingleCondition(string condition)
{
    condition = condition.Trim();

    if (string.IsNullOrEmpty(condition)) return false;
    if (condition.ToLower() == "true") return true;
    if (condition.ToLower() == "false") return false;

    // == operator
    if (condition.Contains("=="))
    {
        string[] parts = condition.Split(new string[] { "==" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            string left = parts[0].Trim().Trim('"', '\'');
            string right = parts[1].Trim().Trim('"', '\'');
            return left == right;
        }
    }

    // != operator
    if (condition.Contains("!="))
    {
        string[] parts = condition.Split(new string[] { "!=" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            string left = parts[0].Trim().Trim('"', '\'');
            string right = parts[1].Trim().Trim('"', '\'');
            return left != right;
        }
    }

    // >= operator (must check before >)
    if (condition.Contains(">="))
    {
        string[] parts = condition.Split(new string[] { ">=" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            double left = WpfEngine.ParseDouble(parts[0].Trim(), 0);
            double right = WpfEngine.ParseDouble(parts[1].Trim(), 0);
            return left >= right;
        }
    }

    // <= operator (must check before <)
    if (condition.Contains("<="))
    {
        string[] parts = condition.Split(new string[] { "<=" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            double left = WpfEngine.ParseDouble(parts[0].Trim(), 0);
            double right = WpfEngine.ParseDouble(parts[1].Trim(), 0);
            return left <= right;
        }
    }

    // > operator
    if (condition.Contains(">") && !condition.Contains(">="))
    {
        string[] parts = condition.Split(new char[] { '>' }, 2);
        if (parts.Length == 2)
        {
            double left = WpfEngine.ParseDouble(parts[0].Trim(), 0);
            double right = WpfEngine.ParseDouble(parts[1].Trim(), 0);
            return left > right;
        }
    }

    // < operator
    if (condition.Contains("<") && !condition.Contains("<="))
    {
        string[] parts = condition.Split(new char[] { '<' }, 2);
        if (parts.Length == 2)
        {
            double left = WpfEngine.ParseDouble(parts[0].Trim(), 0);
            double right = WpfEngine.ParseDouble(parts[1].Trim(), 0);
            return left < right;
        }
    }

    // Bool check
    if (condition.ToLower() == "true" || condition == "1") return true;
    if (condition.ToLower() == "false" || condition == "0") return false;

    // Non-empty string = true
    return !string.IsNullOrEmpty(condition) && condition != "0";
}
        
        protected string ReplaceStateVariables(string text)
{
    if (string.IsNullOrEmpty(text)) return text;
    if (!text.Contains("{")) return text;

    StringBuilder result = new StringBuilder();
    int i = 0;

    while (i < text.Length)
    {
        if (text[i] == '{')
        {
            // Find closing brace
            int closeIndex = text.IndexOf('}', i + 1);
            if (closeIndex > i + 1)
            {
                string varName = text.Substring(i + 1, closeIndex - i - 1);

                // Only replace if varName is a simple identifier
                // Skip if it looks like JSON structure
                bool isSimpleVar = true;
                bool hasQuotes = false;
                bool hasBrackets = false;

                foreach (char c in varName)
                {
                    if (c == '"' || c == '\'') hasQuotes = true;
                    if (c == '[' || c == ']') hasBrackets = true;
                    
                    if (c == ':' || c == ',' || c == '\n')
                    {
                        isSimpleVar = false;
                        break;
                    }
                }

                // If it looks like JSON content (quoted key or array), don't replace
                // EXCEPT if it's explicitly a variable name like {myVar}
                if (isSimpleVar && !hasQuotes && !hasBrackets)
                {
                    // Check if it's a control.property reference
                    if (varName.Contains("."))
                    {
                        string[] parts = varName.Split('.');
                        if (parts.Length == 2)
                        {
                            object propVal = _engine.GetControlProperty(parts[0], parts[1]);
                            if (propVal != null)
                            {
                                result.Append(propVal.ToString());
                                i = closeIndex + 1;
                                continue;
                            }
                        }
                    }

                    // Check state variable
                    object val = _engine.GetVariable(varName);
                    if (val != null)
                    {
                        result.Append(val.ToString());
                        i = closeIndex + 1;
                        continue;
                    }
                }

                // Not a variable, keep as-is
                result.Append('{');
                i++;
            }
            else
            {
                result.Append('{');
                i++;
            }
        }
        else
        {
            result.Append(text[i]);
            i++;
        }
    }

    return result.ToString();
}
        protected string ParseControlProperty(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            if (text.StartsWith("{") && text.EndsWith("}") && text.Contains("."))
            {
                string inner = text.Substring(1, text.Length - 2);
                string[] parts = inner.Split('.');
                if (parts.Length == 2)
                {
                    object value = _engine.GetControlProperty(parts[0], parts[1]);
                    return value != null ? value.ToString() : "";
                }
            }
            return text;
        }
        
        protected string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
        
        #endregion
    }
}
