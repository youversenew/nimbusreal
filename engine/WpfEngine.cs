using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Security.Cryptography;
namespace Nimbus.WPF
{
    /// <summary>
    /// WPF Engine v3.0 - Core Engine for Nimbus Framework
    /// Plugin-based, Data Binding, Caching, Process Management
    /// One-time write - extend via plugins folder
    /// </summary>
    public class WpfEngine
    {
        #region Constants

        private const int MAX_LOG_ENTRIES = 2000;
        private const int MAX_CLICK_HISTORY = 500;
        private const int MAX_CACHE_ENTRIES = 1000;
        private const int FILE_WATCH_DEBOUNCE_MS = 500;
        private const int IPC_POLL_INTERVAL_MS = 100;
        private const int CLOCK_INTERVAL_MS = 1000;
        private const string ENGINE_VERSION = "3.0.0";
        private const string ENGINE_NAME = "Nimbus Engine";
        private const string PLUGIN_FOLDER = "plugins";

        #endregion

        #region Static Culture

        private static readonly CultureInfo INV = CultureInfo.InvariantCulture;

        #endregion

        #region Core Dictionaries (Thread-Safe)

        private readonly object _stateLock = new object();
        private readonly object _controlLock = new object();
        private readonly object _handlerLock = new object();
        private readonly object _cacheLock = new object();
        private readonly object _bindingLock = new object();
        private readonly object _pluginLock = new object();
        private readonly object _logLock = new object();
        private readonly object _clickLock = new object();
        private readonly object _timerLock = new object();
        private readonly object _processLock = new object();

        private Dictionary<string, object> _state;
        private Dictionary<string, FrameworkElement> _controls;
        private Dictionary<string, XmlNode> _eventHandlers;
        private Dictionary<string, Delegate> _compiledMethods;
        private Dictionary<string, object> _variables;
        private Dictionary<string, CacheEntry> _cache;
        private Dictionary<string, List<BindingInfo>> _bindings;
        private Dictionary<string, DispatcherTimer> _timers;
        private Dictionary<string, Process> _processes;
        private Dictionary<string, INimbusPlugin> _plugins;
        private Dictionary<string, Func<XmlNode, object, bool>> _customCommands;
        private Dictionary<string, Func<string, string>> _customFunctions;
        private List<DevLogEntry> _devLogs;
        private List<ClickEvent> _clickHistory;
        private Dictionary<string, bool> _debugSwitches;
        private Dictionary<string, Window> _windows;
        // YANGI: ManualC va Nimbus-based plugins
private Dictionary<string, Type> _nimbusBuiltinPlugins;
private Dictionary<string, bool> _nimbusPluginLoaded;
private CSharpCompiler _csharpCompiler;
private bool _pluginsInitialized;
        #endregion

        #region Public Properties

        public Dictionary<string, Window> Windows
        {
            get { return _windows; }
        }
        
        public ComponentSystem ComponentSystem
        {
            get { return _compSystem; }
        }

        public EventDispatcher EventSystem
        {
            get { return _eventDispatcher; }
        }
        
        // Boshqa public propertylar bilan birga:
        public Dictionary<string, FrameworkElement> Controls
        {
            get { lock (_controlLock) { return new Dictionary<string, FrameworkElement>(_controls); } }
        }

        public Dictionary<string, object> State
        {
            get { lock (_stateLock) { return _state; } }
        }

        public Dictionary<string, XmlNode> EventHandlers
        {
            get { lock (_handlerLock) { return _eventHandlers; } }
        }

        public Dictionary<string, Delegate> CompiledMethods
        {
            get { return _compiledMethods; }
        }

        public Dictionary<string, object> Variables
        {
            get { return _variables; }
        }

        public List<DevLogEntry> DevLogs
        {
            get { lock (_logLock) { return _devLogs; } }
        }

        public List<ClickEvent> ClickHistory
        {
            get { lock (_clickLock) { return _clickHistory; } }
        }

        public Dictionary<string, bool> DebugSwitches
        {
            get { return _debugSwitches; }
        }

        public OSInfo SystemInfo { get; private set; }
        public bool IsDevMode { get; set; }
        public string CurrentXmlPath { get; private set; }
        public string EngineVersion { get { return ENGINE_VERSION; } }

        #endregion

        #region Internal Access for Subsystems

private XmlParser _xmlParser;
private WpfUI _wpfUI;
private LogicRunner _logicRunner;
private ComponentSystem _compSystem;
private XamlRenderer _xamlRenderer;
private ModuleRenderer _moduleRenderer;
private EventDispatcher _eventDispatcher;  // ════════ EVENT SYSTEM ════════
private FileSystemWatcher _fileWatcher;
private XmlDocument _appDoc;
private Window _mainWindow;
private DispatcherTimer _clockTimer;
private DispatcherTimer _ipcTimer;
private DateTime _lastReload = DateTime.MinValue;

// IPC paths
private string _ipcLogFile;
private string _ipcCommandFile;
private string _ipcStateFile;
private bool _ipcRunning;

#endregion
        #region Events

        public event Action<string, string> OnLogMessage;
        public event Action<string> OnHotReload;
        public event Action<ClickEvent> OnControlClick;
        public event Action<string, object, object> OnStateChanged;
        public event Action<string> OnPluginLoaded;
        public event Action<string> OnPluginUnloaded;
        public event Action<string, string> OnBindingUpdated;
        public event Action<string> OnError;

        #endregion

        #region Native APIs

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_MICA_EFFECT = 1029;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        #endregion

        #region Constructor

        public WpfEngine()
{
    // Initialize all dictionaries
    _state = new Dictionary<string, object>();
    _controls = new Dictionary<string, FrameworkElement>();
    _eventHandlers = new Dictionary<string, XmlNode>();
    _compiledMethods = new Dictionary<string, Delegate>();
    _variables = new Dictionary<string, object>();
    _cache = new Dictionary<string, CacheEntry>();
    _bindings = new Dictionary<string, List<BindingInfo>>();
    _timers = new Dictionary<string, DispatcherTimer>();
    _processes = new Dictionary<string, Process>();
    _plugins = new Dictionary<string, INimbusPlugin>();
    _customCommands = new Dictionary<string, Func<XmlNode, object, bool>>();
    _customFunctions = new Dictionary<string, Func<string, string>>();
    _devLogs = new List<DevLogEntry>();
    _clickHistory = new List<ClickEvent>();
    _debugSwitches = new Dictionary<string, bool>();
    _windows = new Dictionary<string, Window>();

    // Initialize subsystems
    _xmlParser = new XmlParser(this);
    _wpfUI = new WpfUI(this);
    _logicRunner = new LogicRunner(this);
    _xamlRenderer = new XamlRenderer(this);
    _moduleRenderer = new ModuleRenderer(this);
    _eventDispatcher = new EventDispatcher(debugLogging: false);  // ════ EVENT SYSTEM ════
    
    // Initialize ComponentSystem
    _compSystem = new ComponentSystem(this);
    
    // OS detection
    SystemInfo = new OSInfo();
    IsDevMode = false;
    
    _nimbusBuiltinPlugins = new Dictionary<string, Type>();
    _nimbusPluginLoaded = new Dictionary<string, bool>();
    _csharpCompiler = new CSharpCompiler(this);
    _pluginsInitialized = false;

    // YANGI: Nimbus-based pluginlarni ro'yxatdan o'tkazish
    RegisterBuiltinPlugins();
    
    // Debug switches
    _debugSwitches["LogClicks"] = true;
    _debugSwitches["LogEvents"] = true;
    _debugSwitches["LogState"] = false;
    _debugSwitches["LogPerformance"] = false;
    _debugSwitches["LogBindings"] = false;
    _debugSwitches["LogPlugins"] = true;
    _debugSwitches["LogCache"] = false;
    _debugSwitches["VerboseMode"] = false;

    // System state variables
    InitializeSystemState();

    // IPC setup
    string tempDir = Path.Combine(Path.GetTempPath(), "nimbus_ipc");
    if (!Directory.Exists(tempDir))
    {
        try { Directory.CreateDirectory(tempDir); } catch { }
    }
    _ipcLogFile = Path.Combine(tempDir, "logs.json");
    _ipcCommandFile = Path.Combine(tempDir, "commands.txt");
    _ipcStateFile = Path.Combine(tempDir, "state.json");
}

        private void InitializeSystemState()
        {
            lock (_stateLock)
            {
                _state["_engineVersion"] = ENGINE_VERSION;
                _state["_engineName"] = ENGINE_NAME;
                _state["_osName"] = SystemInfo.Name;
                _state["_osBuild"] = SystemInfo.Build;
                _state["_osVersion"] = SystemInfo.Version.ToString();
                _state["_supportsBlur"] = SystemInfo.SupportsBlur;
                _state["_supportsAcrylic"] = SystemInfo.SupportsAcrylic;
                _state["_supportsMica"] = SystemInfo.SupportsMica;
                _state["_currentTime"] = DateTime.Now.ToString("HH:mm:ss");
                _state["_currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
                _state["_pid"] = Process.GetCurrentProcess().Id;
                _state["_machineName"] = Environment.MachineName;
                _state["_userName"] = Environment.UserName;
                _state["_workingDir"] = Environment.CurrentDirectory;
                _state["_processorCount"] = Environment.ProcessorCount;
                _state["_is64Bit"] = Environment.Is64BitOperatingSystem;
                _state["_clrVersion"] = Environment.Version.ToString();
                _state["_pluginCount"] = 0;
                _state["_controlCount"] = 0;
                _state["_handlerCount"] = 0;
                _state["_uptime"] = "0";
                // YANGI
        _state["_engineType"] = "Nimbus";
        _state["_renderEngine"] = "Nimbus Render Engine";
        _state["_builtinPluginCount"] = 0;
        _state["_externalPluginCount"] = 0;
        _state["_manualCCount"] = 0;
        _state["_startTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        #endregion

        #region Plugin System

        /// <summary>
        /// Load all plugins from plugins folder
        /// </summary>
        private void RegisterBuiltinPlugins()
{
    // Nimbus-based pluginlar (framework bilan birga keladi)
    // Faqat ishlatilganda yuklanadi (lazy loading)
    _nimbusBuiltinPlugins["MathPlugin"] = typeof(NimbusMathPlugin);
    _nimbusBuiltinPlugins["StringPlugin"] = typeof(NimbusStringPlugin);
    _nimbusBuiltinPlugins["FilePlugin"] = typeof(NimbusFilePlugin);
    _nimbusBuiltinPlugins["DatePlugin"] = typeof(NimbusDatePlugin);
    _nimbusBuiltinPlugins["JsonPlugin"] = typeof(NimbusJsonPlugin);
    _nimbusBuiltinPlugins["UIPlugin"] = typeof(NimbusUIPlugin);
    _nimbusBuiltinPlugins["NetPlugin"] = typeof(NimbusNetPlugin);
    _nimbusBuiltinPlugins["CryptoPlugin"] = typeof(NimbusCryptoPlugin);
    _nimbusBuiltinPlugins["ClipboardPlugin"] = typeof(NimbusClipboardPlugin);
    _nimbusBuiltinPlugins["DialogPlugin"] = typeof(NimbusDialogPlugin);

    foreach (string name in _nimbusBuiltinPlugins.Keys)
    {
        _nimbusPluginLoaded[name] = false;
    }

    Log("ENGINE", "Registered " + _nimbusBuiltinPlugins.Count + " builtin plugins (lazy)");
}
public bool EnsureNimbusPlugin(string pluginName)
{
    lock (_pluginLock)
    {
        // Allaqachon yuklangan
        if (_plugins.ContainsKey(pluginName))
            return true;

        // Builtin plugin bormi
        if (_nimbusBuiltinPlugins.ContainsKey(pluginName))
        {
            if (!_nimbusPluginLoaded[pluginName])
            {
                try
                {
                    Type pluginType = _nimbusBuiltinPlugins[pluginName];
                    INimbusPlugin instance = (INimbusPlugin)Activator.CreateInstance(pluginType);
                    _plugins[pluginName] = instance;
                    instance.OnLoad(this);
                    _nimbusPluginLoaded[pluginName] = true;
                    Log("PLUGIN", "Lazy loaded builtin: " + pluginName);
                    return true;
                }
                catch (Exception ex)
                {
                    Log("ERROR", "Builtin plugin load failed [" + pluginName + "]: " + ex.Message);
                    return false;
                }
            }
            return true;
        }

        return false;
    }
}
        public void LoadPlugins()
{
    if (_pluginsInitialized) return;
    _pluginsInitialized = true;

    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string pluginDir = Path.Combine(baseDir, PLUGIN_FOLDER);

    if (!string.IsNullOrEmpty(CurrentXmlPath))
    {
        string xmlDir = Path.GetDirectoryName(CurrentXmlPath);
        string localPluginDir = Path.Combine(xmlDir, PLUGIN_FOLDER);
        if (Directory.Exists(localPluginDir))
        {
            pluginDir = localPluginDir;
        }
    }

    // External plugins papkadan yuklash
    if (Directory.Exists(pluginDir))
    {
        string[] csFiles = Directory.GetFiles(pluginDir, "*.cs");
        foreach (string csFile in csFiles)
        {
            try
            {
                LoadPluginFromSource(csFile);
            }
            catch (Exception ex)
            {
                Log("ERROR", "External plugin load failed [" + Path.GetFileName(csFile) + "]: " + ex.Message);
            }
        }

        string[] dllFiles = Directory.GetFiles(pluginDir, "*.dll");
        foreach (string dllFile in dllFiles)
        {
            try
            {
                LoadPluginFromDll(dllFile);
            }
            catch (Exception ex)
            {
                Log("ERROR", "External DLL load failed [" + Path.GetFileName(dllFile) + "]: " + ex.Message);
            }
        }

        // YANGI: XML-based external pluginlarni yuklash
        string[] xmlFiles = Directory.GetFiles(pluginDir, "*.xml");
        foreach (string xmlFile in xmlFiles)
        {
            try
            {
                LoadPluginFromXml(xmlFile);
            }
            catch (Exception ex)
            {
                Log("ERROR", "XML plugin load failed [" + Path.GetFileName(xmlFile) + "]: " + ex.Message);
            }
        }
    }
    else
    {
        Log("PLUGIN", "No external plugins folder. Using builtin plugins only.");
    }

    int externalCount = 0;
    lock (_pluginLock)
    {
        foreach (KeyValuePair<string, INimbusPlugin> kvp in _plugins)
        {
            if (!_nimbusBuiltinPlugins.ContainsKey(kvp.Key))
                externalCount++;
        }
    }

    lock (_stateLock)
    {
        _state["_pluginCount"] = _plugins.Count;
        _state["_builtinPluginCount"] = _nimbusBuiltinPlugins.Count;
        _state["_externalPluginCount"] = externalCount;
    }

    Log("PLUGIN", "External: " + externalCount + ", Builtin: " + _nimbusBuiltinPlugins.Count + " (lazy)");
}
private void LoadPluginFromXml(string xmlFilePath)
{
    string fileName = Path.GetFileNameWithoutExtension(xmlFilePath);
    Log("PLUGIN", "Loading XML plugin: " + fileName);

    XmlDocument doc = new XmlDocument();
    doc.Load(xmlFilePath);

    XmlNode root = doc.DocumentElement;
    if (root == null || root.Name != "NimbusPlugin") return;

    string pluginName = _xmlParser.GetAttribute(root, "Name", fileName);
    string pluginVersion = _xmlParser.GetAttribute(root, "Version", "1.0");
    string pluginDesc = _xmlParser.GetAttribute(root, "Description", "");

    XmlPluginAdapter adapter = new XmlPluginAdapter();
    adapter.PluginName = pluginName;
    adapter.PluginVersion = pluginVersion;
    adapter.PluginDescription = pluginDesc;

    // Handlerlarni parse qilish
    XmlNode handlersNode = root.SelectSingleNode("Handlers");
    if (handlersNode != null)
    {
        foreach (XmlNode handler in handlersNode.ChildNodes)
        {
            if (handler.NodeType != XmlNodeType.Element) continue;
            if (handler.Name == "Handler")
            {
                string handlerName = _xmlParser.GetAttribute(handler, "Name", "");
                if (!string.IsNullOrEmpty(handlerName))
                {
                    adapter.Handlers[handlerName] = handler;
                }
            }
        }
    }

    // Commandlarni parse qilish
    XmlNode commandsNode = root.SelectSingleNode("Commands");
    if (commandsNode != null)
    {
        foreach (XmlNode cmdNode in commandsNode.ChildNodes)
        {
            if (cmdNode.NodeType != XmlNodeType.Element) continue;
            if (cmdNode.Name == "Command")
            {
                string cmdName = _xmlParser.GetAttribute(cmdNode, "Name", "");
                string cmdHandler = _xmlParser.GetAttribute(cmdNode, "Handler", "");
                if (!string.IsNullOrEmpty(cmdName) && !string.IsNullOrEmpty(cmdHandler))
                {
                    adapter.CommandMap[cmdName] = cmdHandler;
                }
            }
        }
    }

    RegisterPlugin(adapter);
    Log("PLUGIN", "XML Plugin loaded: " + pluginName + " v" + pluginVersion);
}

        /// <summary>
        /// Compile and load a .cs plugin file
        /// </summary>
// WpfEngine.cs ga qo'shing (public class WpfEngine ichiga):

public void CompileManualCCode(string moduleId, string code)
{
    if (_csharpCompiler == null)
    {
        Log("ERROR", "Compiler not initialized");
        return;
    }

    try
    {
        // XmlNode yaratib, mavjud Compile metodiga berish
        XmlDocument tempDoc = new XmlDocument();
        XmlElement manualCNode = tempDoc.CreateElement("ManualC");
        manualCNode.SetAttribute("id", moduleId);
        
        // HTML entities encode qilish
        string encodedCode = code.Replace("&", "&amp;")
                                  .Replace("<", "&lt;")
                                  .Replace(">", "&gt;");
        manualCNode.InnerXml = "<![CDATA[" + code + "]]>";
        
        // Yoki oddiy InnerText
        // manualCNode.InnerText = code;
        
        bool success = _csharpCompiler.Compile(manualCNode);
        
        if (success)
        {
            Log("COMPILE", "Module compiled successfully: " + moduleId);
            lock (_stateLock)
            {
                int count = 0;
                if (_state.ContainsKey("_manualCCount"))
                {
                    int.TryParse(_state["_manualCCount"].ToString(), out count);
                }
                _state["_manualCCount"] = count + 1;
            }
        }
        else
        {
            Log("ERROR", "Module compilation failed: " + moduleId);
        }
    }
    catch (Exception ex)
    {
        Log("ERROR", "CompileManualCCode error: " + ex.Message);
    }
}
        private void LoadPluginFromSource(string csFilePath)
{
    string fileName = Path.GetFileNameWithoutExtension(csFilePath);
    string code = File.ReadAllText(csFilePath, Encoding.UTF8);

    Log("PLUGIN", "Compiling plugin: " + fileName);

    CSharpCodeProvider provider = new CSharpCodeProvider();
    CompilerParameters parameters = new CompilerParameters();
    parameters.GenerateInMemory = true;
    parameters.GenerateExecutable = false;
    parameters.TreatWarningsAsErrors = false;

    // Add standard references
    parameters.ReferencedAssemblies.Add("System.dll");
    parameters.ReferencedAssemblies.Add("System.Core.dll");
    parameters.ReferencedAssemblies.Add("System.Xml.dll");
    parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

    // Add WPF references (using RuntimeEnvironment to find real path)
    string runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
    string wpfPath = Path.Combine(runtimeDir, "WPF");

    // Try multiple paths for WPF DLLs
    string[] wpfDlls = { "WindowsBase.dll", "PresentationCore.dll", "PresentationFramework.dll" };
    
    foreach (string dll in wpfDlls)
    {
        string p1 = Path.Combine(wpfPath, dll);
        string p2 = Path.Combine(runtimeDir, dll);
        
        if (File.Exists(p1)) parameters.ReferencedAssemblies.Add(p1);
        else if (File.Exists(p2)) parameters.ReferencedAssemblies.Add(p2);
    }

    string xamlPath = Path.Combine(runtimeDir, "System.Xaml.dll");
    if (File.Exists(xamlPath)) parameters.ReferencedAssemblies.Add(xamlPath);

    // CRITICAL FIX: Reference the running assembly (Nimbus.exe itself)
    // This allows plugins to use "using Nimbus.WPF;"
    string selfPath = Assembly.GetExecutingAssembly().Location;
    if (!string.IsNullOrEmpty(selfPath) && File.Exists(selfPath))
    {
        parameters.ReferencedAssemblies.Add(selfPath);
    }

    CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

    if (results.Errors.HasErrors)
    {
        StringBuilder errors = new StringBuilder();
        errors.AppendLine("Plugin compilation errors [" + fileName + "]:");
        foreach (CompilerError error in results.Errors)
        {
            if (!error.IsWarning)
            {
                errors.AppendLine("  Line " + error.Line + ": " + error.ErrorText);
            }
        }
        throw new Exception(errors.ToString());
    }

    Assembly assembly = results.CompiledAssembly;
    RegisterPluginsFromAssembly(assembly, fileName);
}

        /// <summary>
        /// Load a pre-compiled .dll plugin
        /// </summary>
        private void LoadPluginFromDll(string dllPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(dllPath);
            Log("PLUGIN", "Loading DLL plugin: " + fileName);

            Assembly assembly = Assembly.LoadFrom(dllPath);
            RegisterPluginsFromAssembly(assembly, fileName);
        }

        /// <summary>
        /// Find and register all INimbusPlugin implementations in assembly
        /// </summary>
        private void RegisterPluginsFromAssembly(Assembly assembly, string sourceName)
        {
            Type pluginInterface = typeof(INimbusPlugin);
            int registered = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (pluginInterface.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    try
                    {
                        INimbusPlugin plugin = (INimbusPlugin)Activator.CreateInstance(type);
                        RegisterPlugin(plugin);
                        registered++;
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Failed to create plugin instance [" + type.Name + "]: " + ex.Message);
                    }
                }
            }

            if (registered == 0)
            {
                // Try to find static Register method as fallback
                foreach (Type type in assembly.GetTypes())
                {
                    MethodInfo registerMethod = type.GetMethod("Register",
                        BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { typeof(WpfEngine) }, null);

                    if (registerMethod != null)
                    {
                        try
                        {
                            registerMethod.Invoke(null, new object[] { this });
                            Log("PLUGIN", "Static plugin registered: " + type.Name);
                            registered++;
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR", "Static Register failed [" + type.Name + "]: " + ex.Message);
                        }
                    }
                }
            }

            if (registered == 0)
            {
                Log("WARN", "No plugin classes found in: " + sourceName);
            }
        }

        /// <summary>
        /// Register a single plugin instance
        /// </summary>
        public void RegisterPlugin(INimbusPlugin plugin)
        {
            lock (_pluginLock)
            {
                string name = plugin.Name;

                if (_plugins.ContainsKey(name))
                {
                    // Unload existing
                    try { _plugins[name].OnUnload(this); } catch { }
                    _plugins.Remove(name);
                    Log("PLUGIN", "Replaced existing plugin: " + name);
                }

                _plugins[name] = plugin;

                try
                {
                    plugin.OnLoad(this);
                }
                catch (Exception ex)
                {
                    Log("ERROR", "Plugin OnLoad failed [" + name + "]: " + ex.Message);
                }

                Log("PLUGIN", "Registered: " + name + " v" + plugin.Version);

                if (OnPluginLoaded != null)
                {
                    OnPluginLoaded.Invoke(name);
                }
            }
        }

        /// <summary>
        /// Unload a plugin by name
        /// </summary>
        public void UnloadPlugin(string name)
        {
            lock (_pluginLock)
            {
                if (_plugins.ContainsKey(name))
                {
                    try { _plugins[name].OnUnload(this); } catch { }
                    _plugins.Remove(name);

                    Log("PLUGIN", "Unloaded: " + name);

                    if (OnPluginUnloaded != null)
                    {
                        OnPluginUnloaded.Invoke(name);
                    }
                }
            }
        }

        /// <summary>
        /// Register a custom command from plugin
        /// </summary>
        public void RegisterCommand(string commandName, Func<XmlNode, object, bool> handler)
        {
            lock (_pluginLock)
            {
                _customCommands[commandName.ToLower()] = handler;
                Log("PLUGIN", "Command registered: " + commandName);
            }
        }

        /// <summary>
        /// Register a custom function from plugin (for use in expressions)
        /// </summary>
        public void RegisterFunction(string functionName, Func<string, string> handler)
        {
            lock (_pluginLock)
            {
                _customFunctions[functionName.ToLower()] = handler;
                Log("PLUGIN", "Function registered: " + functionName);
            }
        }

        /// <summary>
        /// Try execute a custom command
        /// Returns true if command was found and executed
        /// </summary>
        public bool TryExecuteCustomCommand(string commandName, XmlNode node, object sender)
        {
            lock (_pluginLock)
            {
                string key = commandName.ToLower();
                if (_customCommands.ContainsKey(key))
                {
                    try
                    {
                        _customCommands[key].Invoke(node, sender);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Custom command error [" + commandName + "]: " + ex.Message);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Try execute a custom function
        /// </summary>
        public string TryExecuteCustomFunction(string functionName, string args)
        {
            lock (_pluginLock)
            {
                string key = functionName.ToLower();
                if (_customFunctions.ContainsKey(key))
                {
                    try
                    {
                        return _customFunctions[key].Invoke(args);
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Custom function error [" + functionName + "]: " + ex.Message);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get list of loaded plugins
        /// </summary>
        public List<PluginInfo> GetLoadedPlugins()
        {
            List<PluginInfo> list = new List<PluginInfo>();
            lock (_pluginLock)
            {
                foreach (KeyValuePair<string, INimbusPlugin> kvp in _plugins)
                {
                    PluginInfo info = new PluginInfo();
                    info.Name = kvp.Value.Name;
                    info.Version = kvp.Value.Version;
                    info.Description = kvp.Value.Description;
                    list.Add(info);
                }
            }
            return list;
        }

        /// <summary>
        /// Notify all plugins of an event
        /// </summary>
        private void NotifyPlugins(string eventName, object data)
        {
            lock (_pluginLock)
            {
                foreach (KeyValuePair<string, INimbusPlugin> kvp in _plugins)
                {
                    try
                    {
                        kvp.Value.OnEvent(this, eventName, data);
                    }
                    catch (Exception ex)
                    {
                        if (_debugSwitches["LogPlugins"])
                        {
                            Log("WARN", "Plugin event error [" + kvp.Key + "]: " + ex.Message);
                        }
                    }
                }
            }
        }

        #endregion

        #region Data Binding System

        /// <summary>
        /// Bind a state variable to a control property
        /// When state changes, control updates automatically
        /// </summary>
        public void Bind(string stateKey, string controlName, string property)
        {
            lock (_bindingLock)
            {
                if (!_bindings.ContainsKey(stateKey))
                {
                    _bindings[stateKey] = new List<BindingInfo>();
                }

                BindingInfo binding = new BindingInfo();
                binding.StateKey = stateKey;
                binding.ControlName = controlName;
                binding.Property = property;
                binding.IsActive = true;

                // Check for duplicate
                bool exists = false;
                foreach (BindingInfo existing in _bindings[stateKey])
                {
                    if (existing.ControlName == controlName && existing.Property == property)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    _bindings[stateKey].Add(binding);

                    if (_debugSwitches["LogBindings"])
                    {
                        Log("BIND", stateKey + " -> " + controlName + "." + property);
                    }
                }
            }
        }

        /// <summary>
        /// Remove all bindings for a state key
        /// </summary>
        public void Unbind(string stateKey)
        {
            lock (_bindingLock)
            {
                if (_bindings.ContainsKey(stateKey))
                {
                    _bindings.Remove(stateKey);
                    Log("BIND", "Unbound: " + stateKey);
                }
            }
        }

        /// <summary>
        /// Remove specific binding
        /// </summary>
        public void Unbind(string stateKey, string controlName, string property)
        {
            lock (_bindingLock)
            {
                if (_bindings.ContainsKey(stateKey))
                {
                    List<BindingInfo> list = _bindings[stateKey];
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].ControlName == controlName && list[i].Property == property)
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process all bindings for a state key
        /// Called automatically when state changes
        /// </summary>
        private void ProcessBindings(string stateKey, object newValue)
        {
            List<BindingInfo> bindingsToProcess = null;

            lock (_bindingLock)
            {
                if (_bindings.ContainsKey(stateKey))
                {
                    bindingsToProcess = new List<BindingInfo>(_bindings[stateKey]);
                }
            }

            if (bindingsToProcess == null || bindingsToProcess.Count == 0) return;

            foreach (BindingInfo binding in bindingsToProcess)
            {
                if (!binding.IsActive) continue;

                try
                {
                    FrameworkElement control = GetControlDirect(binding.ControlName);
                    if (control != null)
                    {
                        string valueStr = newValue != null ? newValue.ToString() : "";

                        // Format support: "{stateKey:format}"
                        if (!string.IsNullOrEmpty(binding.Format))
                        {
                            valueStr = string.Format(INV, binding.Format, newValue);
                        }

                        // Apply via dispatcher if needed
                        if (control.Dispatcher.CheckAccess())
                        {
                            SetControlPropertyDirect(control, binding.Property, valueStr);
                        }
                        else
                        {
                            string capturedValue = valueStr;
                            string capturedProp = binding.Property;
                            FrameworkElement capturedControl = control;

                            control.Dispatcher.BeginInvoke(new Action(delegate
                            {
                                SetControlPropertyDirect(capturedControl, capturedProp, capturedValue);
                            }));
                        }

                        if (_debugSwitches["LogBindings"])
                        {
                            Log("BIND", stateKey + " -> " + binding.ControlName + "." + binding.Property + " = " + valueStr);
                        }

                        if (OnBindingUpdated != null)
                        {
                            OnBindingUpdated.Invoke(stateKey, binding.ControlName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("ERROR", "Binding update error [" + stateKey + " -> " + binding.ControlName + "]: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Parse binding expressions from XML attributes
        /// Format: Bind="{varName}" or Bind="{varName:PropertyName}"
        /// </summary>
        public void ParseBindingExpression(string controlName, string bindExpression)
        {
            if (string.IsNullOrEmpty(bindExpression)) return;

            // Remove { and }
            string expr = bindExpression.Trim();
            if (expr.StartsWith("{") && expr.EndsWith("}"))
            {
                expr = expr.Substring(1, expr.Length - 2);
            }

            string stateKey = expr;
            string property = "Text";

            // Check for property specification
            if (expr.Contains(":"))
            {
                string[] parts = expr.Split(new char[] { ':' }, 2);
                stateKey = parts[0].Trim();
                property = parts[1].Trim();
            }

            Bind(stateKey, controlName, property);

            // Apply current value immediately
            object currentValue = GetVariable(stateKey);
            if (currentValue != null)
            {
                ProcessBindings(stateKey, currentValue);
            }
        }

        /// <summary>
        /// Get all bindings info
        /// </summary>
        public Dictionary<string, List<BindingInfo>> GetAllBindings()
        {
            lock (_bindingLock)
            {
                Dictionary<string, List<BindingInfo>> copy = new Dictionary<string, List<BindingInfo>>();
                foreach (KeyValuePair<string, List<BindingInfo>> kvp in _bindings)
                {
                    copy[kvp.Key] = new List<BindingInfo>(kvp.Value);
                }
                return copy;
            }
        }

        #endregion

        #region Cache System

        /// <summary>
        /// Set cache entry with optional TTL
        /// </summary>
        public void CacheSet(string key, object value, int ttlSeconds)
        {
            lock (_cacheLock)
            {
                // Enforce max cache size
                if (_cache.Count >= MAX_CACHE_ENTRIES && !_cache.ContainsKey(key))
                {
                    // Remove oldest entry
                    string oldestKey = null;
                    DateTime oldestTime = DateTime.MaxValue;
                    foreach (KeyValuePair<string, CacheEntry> kvp in _cache)
                    {
                        if (kvp.Value.CreatedAt < oldestTime)
                        {
                            oldestTime = kvp.Value.CreatedAt;
                            oldestKey = kvp.Key;
                        }
                    }
                    if (oldestKey != null)
                    {
                        _cache.Remove(oldestKey);
                    }
                }

                CacheEntry entry = new CacheEntry();
                entry.Key = key;
                entry.Value = value;
                entry.CreatedAt = DateTime.Now;
                entry.TTLSeconds = ttlSeconds;
                entry.HitCount = 0;

                _cache[key] = entry;

                if (_debugSwitches["LogCache"])
                {
                    Log("CACHE", "SET: " + key + " (TTL: " + ttlSeconds + "s)");
                }
            }
        }

        /// <summary>
        /// Set cache entry with no expiration
        /// </summary>
        public void CacheSet(string key, object value)
        {
            CacheSet(key, value, 0);
        }

        /// <summary>
        /// Get cache entry (returns null if not found or expired)
        /// </summary>
        public object CacheGet(string key)
        {
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(key))
                {
                    CacheEntry entry = _cache[key];

                    // Check TTL
                    if (entry.TTLSeconds > 0)
                    {
                        double elapsed = (DateTime.Now - entry.CreatedAt).TotalSeconds;
                        if (elapsed > entry.TTLSeconds)
                        {
                            _cache.Remove(key);
                            if (_debugSwitches["LogCache"])
                            {
                                Log("CACHE", "EXPIRED: " + key);
                            }
                            return null;
                        }
                    }

                    entry.HitCount++;
                    entry.LastAccessedAt = DateTime.Now;

                    if (_debugSwitches["LogCache"])
                    {
                        Log("CACHE", "HIT: " + key + " (hits: " + entry.HitCount + ")");
                    }

                    return entry.Value;
                }

                if (_debugSwitches["LogCache"])
                {
                    Log("CACHE", "MISS: " + key);
                }

                return null;
            }
        }

        /// <summary>
        /// Check if cache key exists and not expired
        /// </summary>
        public bool CacheHas(string key)
        {
            return CacheGet(key) != null;
        }

        /// <summary>
        /// Remove cache entry
        /// </summary>
        public void CacheRemove(string key)
        {
            lock (_cacheLock)
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        public void CacheClear()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
                Log("CACHE", "Cache cleared");
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public CacheStats GetCacheStats()
        {
            lock (_cacheLock)
            {
                CacheStats stats = new CacheStats();
                stats.TotalEntries = _cache.Count;
                stats.TotalHits = 0;

                foreach (KeyValuePair<string, CacheEntry> kvp in _cache)
                {
                    stats.TotalHits += kvp.Value.HitCount;
                }

                return stats;
            }
        }

        #endregion

        #region Process Management

        /// <summary>
        /// Start an external process
        /// </summary>
        public int StartProcess(string name, string fileName, string arguments, bool redirectOutput)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = fileName;
                psi.Arguments = arguments;
                psi.UseShellExecute = !redirectOutput;
                psi.RedirectStandardOutput = redirectOutput;
                psi.RedirectStandardError = redirectOutput;
                psi.CreateNoWindow = redirectOutput;

                Process process = Process.Start(psi);

                lock (_processLock)
                {
                    _processes[name] = process;
                }

                Log("PROCESS", "Started: " + name + " (PID: " + process.Id + ")");
                return process.Id;
            }
            catch (Exception ex)
            {
                Log("ERROR", "Process start failed [" + name + "]: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Stop a managed process
        /// </summary>
        public bool StopProcess(string name)
        {
            lock (_processLock)
            {
                if (_processes.ContainsKey(name))
                {
                    try
                    {
                        Process p = _processes[name];
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                        _processes.Remove(name);
                        Log("PROCESS", "Stopped: " + name);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Process stop failed [" + name + "]: " + ex.Message);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get output from a process
        /// </summary>
        public string GetProcessOutput(string name)
        {
            lock (_processLock)
            {
                if (_processes.ContainsKey(name))
                {
                    Process p = _processes[name];
                    if (p.StartInfo.RedirectStandardOutput)
                    {
                        return p.StandardOutput.ReadToEnd();
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Run a CMD command and return output
        /// </summary>
        public string RunCmd(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c " + command;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit(30000);

                    if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(output))
                    {
                        return "ERROR: " + error;
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        /// <summary>
        /// Run a CMD command async
        /// </summary>
        public void RunCmdAsync(string command, string resultStateKey, string onComplete)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                string result = RunCmd(command);

                if (_mainWindow != null)
                {
                    _mainWindow.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        if (!string.IsNullOrEmpty(resultStateKey))
                        {
                            SetVariable(resultStateKey, result);
                        }

                        if (!string.IsNullOrEmpty(onComplete))
                        {
                            ExecuteHandler(onComplete);
                        }
                    }));
                }
            });
        }

        /// <summary>
        /// Clean up all managed processes
        /// </summary>
        private void CleanupProcesses()
        {
            lock (_processLock)
            {
                foreach (KeyValuePair<string, Process> kvp in _processes)
                {
                    try
                    {
                        if (!kvp.Value.HasExited)
                        {
                            kvp.Value.Kill();
                        }
                    }
                    catch { }
                }
                _processes.Clear();
            }
        }

        #endregion

        #region Timer Management

        /// <summary>
        /// Create and start a named timer
        /// </summary>
        public void CreateTimer(string name, int intervalMs, string handlerName, bool repeat)
        {
            lock (_timerLock)
            {
                // Stop existing timer with same name
                if (_timers.ContainsKey(name))
                {
                    _timers[name].Stop();
                    _timers.Remove(name);
                }
            }

            // Timer must be created on UI thread
            if (_mainWindow != null && !_mainWindow.Dispatcher.CheckAccess())
            {
                string capturedName = name;
                int capturedInterval = intervalMs;
                string capturedHandler = handlerName;
                bool capturedRepeat = repeat;

                _mainWindow.Dispatcher.BeginInvoke(new Action(delegate
                {
                    CreateTimerOnUIThread(capturedName, capturedInterval, capturedHandler, capturedRepeat);
                }));
            }
            else
            {
                CreateTimerOnUIThread(name, intervalMs, handlerName, repeat);
            }
        }

        private void CreateTimerOnUIThread(string name, int intervalMs, string handlerName, bool repeat)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(intervalMs);

            if (repeat)
            {
                timer.Tick += delegate
                {
                    ExecuteHandler(handlerName);
                };
            }
            else
            {
                timer.Tick += delegate
                {
                    timer.Stop();
                    lock (_timerLock) { _timers.Remove(name); }
                    ExecuteHandler(handlerName);
                };
            }

            lock (_timerLock)
            {
                _timers[name] = timer;
            }

            timer.Start();
            Log("TIMER", "Started: " + name + " (every " + intervalMs + "ms, repeat: " + repeat + ")");
        }

        /// <summary>
        /// Stop a named timer
        /// </summary>
        public void StopTimer(string name)
        {
            lock (_timerLock)
            {
                if (_timers.ContainsKey(name))
                {
                    if (_mainWindow != null && !_mainWindow.Dispatcher.CheckAccess())
                    {
                        string capturedName = name;
                        _mainWindow.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            lock (_timerLock)
                            {
                                if (_timers.ContainsKey(capturedName))
                                {
                                    _timers[capturedName].Stop();
                                    _timers.Remove(capturedName);
                                }
                            }
                        }));
                    }
                    else
                    {
                        _timers[name].Stop();
                        _timers.Remove(name);
                    }
                    Log("TIMER", "Stopped: " + name);
                }
            }
        }

        /// <summary>
        /// Stop all timers
        /// </summary>
        public void StopAllTimers()
        {
            lock (_timerLock)
            {
                foreach (KeyValuePair<string, DispatcherTimer> kvp in _timers)
                {
                    try { kvp.Value.Stop(); } catch { }
                }
                _timers.Clear();
                Log("TIMER", "All timers stopped");
            }
        }

        /// <summary>
        /// Check if timer exists and is running
        /// </summary>
        public bool IsTimerRunning(string name)
        {
            lock (_timerLock)
            {
                return _timers.ContainsKey(name) && _timers[name].IsEnabled;
            }
        }

        #endregion

        #region Logging

        public void Log(string level, string message)
        {
            DevLogEntry entry = new DevLogEntry();
            entry.Timestamp = DateTime.Now;
            entry.Level = level;
            entry.Message = message;

            lock (_logLock)
            {
                _devLogs.Add(entry);
                if (_devLogs.Count > MAX_LOG_ENTRIES)
                {
                    _devLogs.RemoveAt(0);
                }
            }

            if (OnLogMessage != null)
            {
                try { OnLogMessage.Invoke(level, message); } catch { }
            }

            if (IsDevMode)
            {
                WriteLogToFile(entry);
            }

            NotifyPlugins("log", entry);
        }

        public void LogClick(string controlName, string controlType)
        {
            if (!_debugSwitches.ContainsKey("LogClicks") || !_debugSwitches["LogClicks"]) return;

            ClickEvent click = new ClickEvent();
            click.Timestamp = DateTime.Now;
            click.ControlName = controlName;
            click.ControlType = controlType;

            lock (_clickLock)
            {
                _clickHistory.Add(click);
                if (_clickHistory.Count > MAX_CLICK_HISTORY)
                {
                    _clickHistory.RemoveAt(0);
                }
            }

            if (OnControlClick != null)
            {
                try { OnControlClick.Invoke(click); } catch { }
            }

            Log("CLICK", controlName + " (" + controlType + ")");
        }

        #endregion

        #region Run Application

        public void Run(string source, bool isContent)
        {
            DateTime startTime = DateTime.Now;

            try
            {
                _appDoc = new XmlDocument();

                if (isContent)
                {
                    _appDoc.LoadXml(source);
                    CurrentXmlPath = null;
                    lock (_stateLock) { _state["XmlPath"] = ""; }
                    Log("INFO", "Loading app from embedded content");
                }
                else
                {
                    CurrentXmlPath = Path.GetFullPath(source);
                    lock (_stateLock) { _state["XmlPath"] = CurrentXmlPath; }
                    _appDoc.Load(CurrentXmlPath);
                    Log("INFO", "Loading app from: " + CurrentXmlPath);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", "XML load error: " + ex.Message);
                MessageBox.Show("XML yuklashda xato:\n" + ex.Message, "Nimbus Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            XmlNode root = _appDoc.DocumentElement;
            if (root == null)
            {
                Log("ERROR", "XML root element not found");
                MessageBox.Show("XML root element topilmadi!", "Nimbus Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Parse app metadata
            ParseAppMetadata(root);

            // Check for dev mode
            string devMode = _xmlParser.GetAttribute(root, "DevMode", "false");
            if (devMode.ToLower() == "true") IsDevMode = true;

            Log("INFO", "App: " + _state["_appName"] + " v" + _state["_version"]);
            Log("INFO", "Engine: " + ENGINE_NAME + " v" + ENGINE_VERSION);
            Log("INFO", "DevMode: " + IsDevMode);
            Log("INFO", "OS: " + SystemInfo.Name + " (Build " + SystemInfo.Build + ")");

            // Load plugins BEFORE parsing logic (plugins may register commands)
            LoadPlugins();

            // Start IPC if dev mode
            if (IsDevMode)
            {
                StartIPC();
            }
            
            // Parse Components section
            XmlNode componentsNode = root.SelectSingleNode("Components");
            if (componentsNode != null)
            {
                _compSystem.ParseComponentsSection(componentsNode);
                Log("DEBUG", "Components section parsed");
            }
            
            // Parse Styles
            XmlNode stylesNode = root.SelectSingleNode("Styles");
            if (stylesNode != null)
            {
                _wpfUI.ParseStyles(stylesNode);
                Log("DEBUG", "Styles parsed");
            }

            // Parse Logic section
            XmlNode logicNode = root.SelectSingleNode("Logic");
            if (logicNode != null)
            {
                ParseLogic(logicNode);
                Log("DEBUG", "Logic handlers: " + _eventHandlers.Count);
            }

            // Parse Bindings section
            XmlNode bindingsNode = root.SelectSingleNode("Bindings");
            if (bindingsNode != null)
            {
                ParseBindings(bindingsNode);
            }

            // Parse UI
            XmlNode uiNode = root.SelectSingleNode("UI");
            if (uiNode == null)
            {
                Log("ERROR", "UI section not found");
                MessageBox.Show("XML da <UI> bo'limi topilmadi!", "Nimbus Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ===== CHECK FOR INMODULE =====
            bool useInModule = _xmlParser.GetBoolAttribute(root, "inModule", false);
            
            try
            {
                if (useInModule)
                {
                    Log("INFO", "Creating inModule UI (custom C#, no WPF)");
                    
                    // Render XML to UIModule tree
                    IUIModule rootModule = _moduleRenderer.RenderUI(root, uiNode);
                    if (rootModule == null)
                    {
                        throw new Exception("Failed to render UIModule tree");
                    }
                    
                    // Create ModuleWindow
                    _mainWindow = new ModuleWindow(this, root, uiNode, rootModule);
                    Log("INFO", "inModule window created successfully");
                }
                else
                {
                    Log("INFO", "Creating WPF window");
                    _mainWindow = _wpfUI.CreateWindow(root, uiNode);
                    Log("INFO", "WPF window created successfully");
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", "Window creation error: " + ex.Message);
                MessageBox.Show("Window yaratishda xato:\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Nimbus Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_mainWindow == null)
            {
                Log("ERROR", "Window is null");
                MessageBox.Show("Window yaratilmadi!", "Nimbus Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string windowName = _xmlParser.GetAttribute(root, "Name", "MainWindow");
            _windows[windowName] = _mainWindow;

            // Wire up events after window is loaded
            _mainWindow.Loaded += delegate
{
    try
    {
        // Qayta register - visual tree to'liq tayyor
        RegisterControlsFromContent(_mainWindow);

        // Event handlerlarni ulash
        _wpfUI.WireUpEventHandlers(_mainWindow, uiNode);

        lock (_stateLock)
        {
            _state["_controlCount"] = _controls.Count;
            _state["_handlerCount"] = _eventHandlers.Count;
        }

        Log("INFO", "Event handlers wired: " + _controls.Count + " controls");
        WriteStateToFile();
        ProcessAllBindings();

        // OnLoad - controllar 100% tayyor
        string onLoadHandler = _xmlParser.GetAttribute(root, "OnLoad", "");
        if (!string.IsNullOrEmpty(onLoadHandler))
        {
            Log("EVENT", "OnLoad -> " + onLoadHandler);
            ExecuteHandler(onLoadHandler, _mainWindow);
        }
    }
    catch (Exception ex)
    {
        Log("WARN", "Loaded event error: " + ex.Message);
    }
};

            // Shortcuts
            XmlNode shortcutsNode = root.SelectSingleNode("Shortcuts");
            if (shortcutsNode != null)
            {
                _wpfUI.WireUpShortcuts(_mainWindow, shortcutsNode);
            }

            // Window effects
            ApplyWindowEffects(root, _mainWindow);

            // Auto-refresh
            if (IsDevMode && !string.IsNullOrEmpty(CurrentXmlPath))
            {
                SetupAutoRefresh(CurrentXmlPath);
            }

            // OnLoad handler
//             // OnLoad handler
// string onLoad = _xmlParser.GetAttribute(root, "OnLoad", "");
// if (!string.IsNullOrEmpty(onLoad))
// {
//     _mainWindow.Loaded += delegate
//     {
//         // Controllar register bo'lgandan KEYIN ishga tushirish
//         _mainWindow.Dispatcher.BeginInvoke(new Action(delegate
//         {
//             Log("EVENT", "OnLoad -> " + onLoad);
//             ExecuteHandler(onLoad, _mainWindow);
//         }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
//     };
// }

            // Setup clock timer on UI thread
            _mainWindow.Loaded += delegate
            {
                _clockTimer = new DispatcherTimer();
                _clockTimer.Interval = TimeSpan.FromMilliseconds(CLOCK_INTERVAL_MS);
                _clockTimer.Tick += OnClockTick;
                _clockTimer.Start();
            };

            // Notify plugins of app start
            NotifyPlugins("appStart", root);

            double loadTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
            Log("INFO", "App loaded in " + loadTimeMs.ToString("F0") + "ms");

            // Start application
            try
            {
                if (Application.Current == null)
                {
                    Application app = new Application();
                    app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    Log("INFO", "Application starting...");
                    app.Run(_mainWindow);
                }
                else
                {
                    Application.Current.MainWindow = _mainWindow;
                    _mainWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", "Application run error: " + ex.Message);
                MessageBox.Show("Application ishga tushishda xato:\n" + ex.Message,
                    "Nimbus Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Cleanup
            Shutdown();
        }

        /// <summary>
        /// Full cleanup
        /// </summary>
        private void Shutdown()
        {
            Log("INFO", "Shutting down...");

            // Stop clock
            if (_clockTimer != null)
            {
                try { _clockTimer.Stop(); } catch { }
            }

            // Stop all timers
            StopAllTimers();

            // Stop IPC
            StopIPC();

            // Stop file watcher
            if (_fileWatcher != null)
            {
                try
                {
                    _fileWatcher.EnableRaisingEvents = false;
                    _fileWatcher.Dispose();
                }
                catch { }
            }

            // Cleanup processes
            CleanupProcesses();

            // Unload plugins
            lock (_pluginLock)
            {
                foreach (KeyValuePair<string, INimbusPlugin> kvp in _plugins)
                {
                    try { kvp.Value.OnUnload(this); } catch { }
                }
                _plugins.Clear();
            }

            // Notify
            NotifyPlugins("appShutdown", null);

            Log("INFO", "Shutdown complete");
        }

        private void OnClockTick(object sender, EventArgs e)
        {
            string currentTime = DateTime.Now.ToString("HH:mm:ss");
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            lock (_stateLock)
            {
                _state["_currentTime"] = currentTime;
                _state["_currentDate"] = currentDate;

                // Update uptime
                long uptickMs = Environment.TickCount;
                _state["_uptime"] = (uptickMs / 1000).ToString();
            }

            // Update clock labels via binding
            ProcessBindings("_currentTime", currentTime);
            ProcessBindings("_currentDate", currentDate);

            // Update specific clock control if exists
            FrameworkElement clockLabel = GetControlDirect("clockLabel");
            if (clockLabel != null && clockLabel is TextBlock)
            {
                ((TextBlock)clockLabel).Text = currentTime;
            }
        }

        private void ParseAppMetadata(XmlNode root)
        {
            lock (_stateLock)
            {
                _state["_appName"] = _xmlParser.GetAttribute(root, "Name", "Nimbus App");
                _state["_theme"] = _xmlParser.GetAttribute(root, "Theme", "Dark");
                _state["_version"] = _xmlParser.GetAttribute(root, "Version", "1.0.0");
                _state["_author"] = _xmlParser.GetAttribute(root, "Author", "");
            }
        }

        private void ParseLogic(XmlNode logicNode)
{
    if (logicNode == null) return;

    foreach (XmlNode child in logicNode.ChildNodes)
    {
        if (child.NodeType != XmlNodeType.Element) continue;

        string nodeName = child.Name;

        if (nodeName == "Include")
        {
            string source = _xmlParser.GetAttribute(child, "Source", "");
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(CurrentXmlPath))
            {
                string dir = Path.GetDirectoryName(CurrentXmlPath);
                string path = Path.Combine(dir, source);
                
                if (File.Exists(path))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(path);
                        
                        // Agar ildiz <Logic> bo'lsa yoki <Handlers> bo'lsa
                        if (doc.DocumentElement.Name == "Logic" || doc.DocumentElement.Name == "Handlers")
                        {
                            ParseLogic(doc.DocumentElement); // Rekursiv yuklash
                        }
                        // Agar ildiz <App> bo'lsa, uning ichidagi Logic ni qidiramiz
                        else if (doc.DocumentElement.Name == "App" || doc.DocumentElement.Name == "NimbusApp")
                        {
                            XmlNode subLogic = doc.DocumentElement.SelectSingleNode("Logic");
                            if (subLogic != null)
                            {
                                ParseLogic(subLogic);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Include failed [" + source + "]: " + ex.Message);
                    }
                }
                else
                {
                    Log("WARN", "Include file not found: " + source);
                }
            }
        }
        else if (nodeName == "Handler")
        {
            string name = _xmlParser.GetAttribute(child, "Name", "");
            if (!string.IsNullOrEmpty(name))
            {
                lock (_handlerLock) { _eventHandlers[name] = child; }
            }
        }
        else if (nodeName == "Var" || nodeName == "Variable")
        {
            string varName = _xmlParser.GetAttribute(child, "Name", "");
            string varValue = _xmlParser.GetAttribute(child, "Value", "");
            string varType = _xmlParser.GetAttribute(child, "Type", "string");

            if (!string.IsNullOrEmpty(varName))
            {
                object converted = ConvertValue(varValue, varType);
                _variables[varName] = converted;
                lock (_stateLock) { _state[varName] = converted; }
            }
        }
        else if (nodeName == "Timer")
        {
            string timerName = _xmlParser.GetAttribute(child, "Name", "timer_" + Guid.NewGuid().ToString("N").Substring(0, 6));
            string intervalStr = _xmlParser.GetAttribute(child, "Interval", "1000");
            string handler = _xmlParser.GetAttribute(child, "Handler", "");
            string repeatStr = _xmlParser.GetAttribute(child, "Repeat", "true");

            int interval = ParseInt(intervalStr, 1000);
            bool repeat = repeatStr.ToLower() != "false" && repeatStr != "0";

            if (!string.IsNullOrEmpty(handler))
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Loaded += delegate
                    {
                        CreateTimer(timerName, interval, handler, repeat);
                    };
                }
                else
                {
                    // Agar oyna hali yo'q bo'lsa, keyinroq yaratish uchun saqlab qo'yamiz
                    // (Bu holat kamdan-kam bo'ladi, chunki ParseLogic odatda Run ichida chaqiriladi)
                }
            }
        }
        else if (nodeName == "Bindings")
        {
            ParseBindings(child);
        }
        // ParseLogic ichida, "ManualC" case ni yangilang:

        if (nodeName == "ManualC" || nodeName == "CSharp" || nodeName == "Code")
        {
            try
            {
                string codeId = _xmlParser.GetAttribute(child, "id", "");
                if (string.IsNullOrEmpty(codeId))
                    codeId = _xmlParser.GetAttribute(child, "Id", "");
                if (string.IsNullOrEmpty(codeId))
                    codeId = _xmlParser.GetAttribute(child, "ID", "");
        
                if (string.IsNullOrEmpty(codeId))
                {
                    Log("ERROR", "ManualC: id attribute required");
                    continue;
                }
        
                bool compiled = _csharpCompiler.Compile(child);
                if (compiled)
                {
                    Log("MANUALC", "Compiled successfully: " + codeId);
                    lock (_stateLock)
                    {
                        int count = 0;
                        if (_state.ContainsKey("_manualCCount"))
                        {
                            int.TryParse(_state["_manualCCount"].ToString(), out count);
                        }
                        _state["_manualCCount"] = count + 1;
                    }
                }
                else
                {
                    Log("ERROR", "ManualC compilation failed: " + codeId);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", "ManualC error: " + ex.Message);
            }
        }
        if (nodeName == "Plugin" || nodeName == "UsePlugin")
        {
            string pName = _xmlParser.GetAttribute(child, "Name", "");
            if (!string.IsNullOrEmpty(pName))
            {
                EnsureNimbusPlugin(pName);
            }
        }
    }
}
public CSharpCompiler GetCompiler()
{
    return _csharpCompiler;
}

        private void ParseBindings(XmlNode bindingsNode)
        {
            foreach (XmlNode child in bindingsNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;

                if (child.Name == "Bind")
                {
                    string source = _xmlParser.GetAttribute(child, "Source", "");
                    string target = _xmlParser.GetAttribute(child, "Target", "");
                    string property = _xmlParser.GetAttribute(child, "Property", "Text");

                    if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target))
                    {
                        Bind(source, target, property);
                    }
                }
            }
        }

        /// <summary>
        /// Process all existing bindings with current state
        /// </summary>
        private void ProcessAllBindings()
        {
            Dictionary<string, List<BindingInfo>> bindingsCopy;
            lock (_bindingLock)
            {
                bindingsCopy = new Dictionary<string, List<BindingInfo>>(_bindings);
            }

            foreach (KeyValuePair<string, List<BindingInfo>> kvp in bindingsCopy)
            {
                object value = GetVariable(kvp.Key);
                if (value != null)
                {
                    ProcessBindings(kvp.Key, value);
                }
            }
        }

        #endregion

        #region IPC Communication

        public void StartIPC()
        {
            if (_ipcRunning) return;
            _ipcRunning = true;

            // Clear old files
            try
            {
                if (File.Exists(_ipcLogFile)) File.Delete(_ipcLogFile);
                if (File.Exists(_ipcCommandFile)) File.Delete(_ipcCommandFile);
                if (File.Exists(_ipcStateFile)) File.Delete(_ipcStateFile);
            }
            catch { }

            // IPC timer must be created on UI thread
            if (_mainWindow != null)
            {
                _mainWindow.Loaded += delegate
                {
                    _ipcTimer = new DispatcherTimer();
                    _ipcTimer.Interval = TimeSpan.FromMilliseconds(IPC_POLL_INTERVAL_MS);
                    _ipcTimer.Tick += ProcessIPCCommands;
                    _ipcTimer.Start();
                };
            }

            WriteStateToFile();
            Log("IPC", "IPC communication started");
        }

        public void StopIPC()
        {
            _ipcRunning = false;
            if (_ipcTimer != null)
            {
                try { _ipcTimer.Stop(); } catch { }
            }
        }

        private void ProcessIPCCommands(object sender, EventArgs e)
        {
            if (!_ipcRunning) return;

            try
            {
                if (File.Exists(_ipcCommandFile))
                {
                    string content = "";
                    // Retry read with small delay for file lock
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            content = File.ReadAllText(_ipcCommandFile, Encoding.UTF8);
                            File.Delete(_ipcCommandFile);
                            break;
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(10);
                        }
                    }

                    if (!string.IsNullOrEmpty(content))
                    {
                        string[] commands = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string cmd in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(cmd))
                            {
                                ProcessIPCCommand(cmd.Trim());
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void ProcessIPCCommand(string command)
        {
            try
            {
                string[] parts = command.Split(new char[] { ' ' }, 2);
                string cmd = parts[0].ToLower();
                string args = parts.Length > 1 ? parts[1] : "";

                switch (cmd)
                {
                    case "reload":
                        ReloadUI();
                        Log("IPC", "Hot reload triggered via IPC");
                        break;

                    case "exec":
                        if (!string.IsNullOrEmpty(args))
                        {
                            ExecuteHandler(args);
                            Log("IPC", "Handler executed: " + args);
                        }
                        break;

                    case "set":
                        string[] setParts = args.Split(new char[] { ' ' }, 2);
                        if (setParts.Length == 2)
                        {
                            SetVariable(setParts[0], setParts[1]);
                        }
                        break;

                    case "get":
                        if (!string.IsNullOrEmpty(args))
                        {
                            object val = GetVariable(args);
                            Log("IPC", args + " = " + (val != null ? val.ToString() : "null"));
                        }
                        break;

                    case "switch":
                        string[] swParts = args.Split(' ');
                        if (swParts.Length == 2 && _debugSwitches.ContainsKey(swParts[0]))
                        {
                            _debugSwitches[swParts[0]] = swParts[1].ToLower() == "on" || swParts[1] == "1";
                            Log("IPC", "Switch " + swParts[0] + " = " + (_debugSwitches[swParts[0]] ? "ON" : "OFF"));
                        }
                        break;

                    case "state":
                        WriteStateToFile();
                        break;

                    case "cmd":
                        if (!string.IsNullOrEmpty(args))
                        {
                            string output = RunCmd(args);
                            Log("CMD", output);
                        }
                        break;

                    case "cache_clear":
                        CacheClear();
                        break;

                    case "timer_stop":
                        if (!string.IsNullOrEmpty(args))
                        {
                            StopTimer(args);
                        }
                        break;

                    case "plugin_list":
                        List<PluginInfo> plugins = GetLoadedPlugins();
                        foreach (PluginInfo pi in plugins)
                        {
                            Log("IPC", "Plugin: " + pi.Name + " v" + pi.Version);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", "IPC command error: " + ex.Message);
            }
        }

        public void WriteStateToFile()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");

                // State
                sb.AppendLine("  \"state\": {");
                int i = 0;
                Dictionary<string, object> stateCopy;
                lock (_stateLock) { stateCopy = new Dictionary<string, object>(_state); }

                foreach (KeyValuePair<string, object> kvp in stateCopy)
                {
                    string value = kvp.Value != null ? kvp.Value.ToString().Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "") : "null";
                    sb.Append("    \"" + kvp.Key + "\": \"" + value + "\"");
                    if (i < stateCopy.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  },");

                // Controls
                sb.AppendLine("  \"controls\": [");
                i = 0;
                List<string> controlKeys;
                lock (_controlLock) { controlKeys = new List<string>(_controls.Keys); }

                foreach (string key in controlKeys)
                {
                    sb.Append("    \"" + key + "\"");
                    if (i < controlKeys.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  ],");

                // Handlers
                sb.AppendLine("  \"handlers\": [");
                i = 0;
                List<string> handlerKeys;
                lock (_handlerLock) { handlerKeys = new List<string>(_eventHandlers.Keys); }

                foreach (string key in handlerKeys)
                {
                    sb.Append("    \"" + key + "\"");
                    if (i < handlerKeys.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  ],");

                // Plugins
                sb.AppendLine("  \"plugins\": [");
                i = 0;
                List<PluginInfo> pluginList = GetLoadedPlugins();
                foreach (PluginInfo pi in pluginList)
                {
                    sb.Append("    {\"name\":\"" + pi.Name + "\",\"version\":\"" + pi.Version + "\"}");
                    if (i < pluginList.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  ],");

                // Switches
                sb.AppendLine("  \"switches\": {");
                i = 0;
                foreach (KeyValuePair<string, bool> kvp in _debugSwitches)
                {
                    sb.Append("    \"" + kvp.Key + "\": " + (kvp.Value ? "true" : "false"));
                    if (i < _debugSwitches.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  },");

                // Cache stats
                CacheStats cacheStats = GetCacheStats();
                sb.AppendLine("  \"cache\": {");
                sb.AppendLine("    \"entries\": " + cacheStats.TotalEntries + ",");
                sb.AppendLine("    \"hits\": " + cacheStats.TotalHits);
                sb.AppendLine("  },");

                // Timers
                sb.AppendLine("  \"timers\": [");
                i = 0;
                List<string> timerKeys;
                lock (_timerLock) { timerKeys = new List<string>(_timers.Keys); }
                foreach (string key in timerKeys)
                {
                    sb.Append("    \"" + key + "\"");
                    if (i < timerKeys.Count - 1) sb.Append(",");
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine("  ],");

                // Bindings
                sb.AppendLine("  \"bindings\": " + GetBindingsCount());
                sb.AppendLine("}");

                File.WriteAllText(_ipcStateFile, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        private int GetBindingsCount()
        {
            int count = 0;
            lock (_bindingLock)
            {
                foreach (KeyValuePair<string, List<BindingInfo>> kvp in _bindings)
                {
                    count += kvp.Value.Count;
                }
            }
            return count;
        }

        private void WriteLogToFile(DevLogEntry entry)
        {
            if (!IsDevMode) return;

            try
            {
                string logLine = "{\"time\":\"" + entry.Timestamp.ToString("HH:mm:ss.fff") +
                    "\",\"level\":\"" + entry.Level +
                    "\",\"msg\":\"" + entry.Message.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "") +
                    "\"}\n";

                // Retry for file lock
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        File.AppendAllText(_ipcLogFile, logLine, Encoding.UTF8);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(5);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Window Effects

        private void ApplyWindowEffects(XmlNode root, Window window)
{
    string blur = _xmlParser.GetAttribute(root, "Blur", "");
    string mica = _xmlParser.GetAttribute(root, "Mica", "");
    string acrylic = _xmlParser.GetAttribute(root, "Acrylic", "");
    string darkMode = _xmlParser.GetAttribute(root, "DarkMode", "");
    string transparent = _xmlParser.GetAttribute(root, "Transparent", "");

    // ══════════════════════════════════════════════════════════════════
    // FIX: Transparency ni Loaded eventidan OLDIN o'rnatish shart!
    // ══════════════════════════════════════════════════════════════════
    if (!string.IsNullOrEmpty(transparent) && transparent.ToLower() == "true")
    {
        try 
        {
            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = true;
            window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            Log("EFFECT", "Transparency enabled (Pre-load)");
        } 
        catch (Exception ex) 
        {
            Log("ERROR", "Failed to set transparency: " + ex.Message);
        }
    }

    // Qolgan effektlar (DWM, Mica, Blur) oyna HANDLE (HWND) talab qiladi,
    // shuning uchun ular Loaded eventida qolishi kerak.
    window.Loaded += delegate
    {
        // Dark mode
        if (!string.IsNullOrEmpty(darkMode) && darkMode.ToLower() == "true")
        {
            SetDarkMode(window, true);
            Log("EFFECT", "Dark mode enabled");
        }

        // Mica (Windows 11) - Note: Transparency bilan konflikt qilishi mumkin, lekin kod qolaversin
        if (!string.IsNullOrEmpty(mica) && mica.ToLower() != "false")
        {
            if (SystemInfo.SupportsMica)
            {
                EnableMica(window, mica.ToLower() == "alt");
                Log("EFFECT", "Mica enabled" + (mica.ToLower() == "alt" ? " (Alt)" : ""));
            }
            else
            {
                Log("WARN", "Mica not supported on " + SystemInfo.Name);
            }
        }
        // Acrylic (Windows 10 1803+)
        else if (!string.IsNullOrEmpty(acrylic) && acrylic.ToLower() == "true")
        {
            if (SystemInfo.SupportsAcrylic)
            {
                int opacity = ParseInt(_xmlParser.GetAttribute(root, "AcrylicOpacity", "80"), 80);
                EnableBlur(window, BlurType.Acrylic, opacity);
                Log("EFFECT", "Acrylic enabled (opacity: " + opacity + ")");
            }
            else
            {
                Log("WARN", "Acrylic not supported on " + SystemInfo.Name);
            }
        }
        // Blur (Windows 10+)
        else if (!string.IsNullOrEmpty(blur) && blur.ToLower() == "true")
        {
            if (SystemInfo.SupportsBlur)
            {
                EnableBlur(window, BlurType.Blur, 100);
                Log("EFFECT", "Blur enabled");
            }
            else
            {
                Log("WARN", "Blur not supported on " + SystemInfo.Name);
            }
        }
    };
}

        public void SetDarkMode(Window window, bool enabled)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;

                int value = enabled ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
            }
            catch (Exception ex)
            {
                Log("ERROR", "SetDarkMode failed: " + ex.Message);
            }
        }

        public void EnableMica(Window window, bool useMicaAlt)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;

                int value = useMicaAlt ? 4 : 2;
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));
            }
            catch (Exception ex)
            {
                Log("ERROR", "EnableMica failed: " + ex.Message);
            }
        }

        public void EnableBlur(Window window, BlurType blurType, int opacity)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;

                AccentPolicy accent = new AccentPolicy();

                switch (blurType)
                {
                    case BlurType.Blur:
                        accent.AccentState = 3;
                        break;
                    case BlurType.Acrylic:
                        accent.AccentState = 4;
                        accent.GradientColor = (opacity << 24) | 0x000000;
                        break;
                    case BlurType.Transparent:
                        accent.AccentState = 2;
                        break;
                }

                int accentSize = Marshal.SizeOf(accent);
                IntPtr accentPtr = Marshal.AllocHGlobal(accentSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                WindowCompositionAttributeData data = new WindowCompositionAttributeData();
                data.Attribute = 19;
                data.SizeOfData = accentSize;
                data.Data = accentPtr;

                SetWindowCompositionAttribute(hwnd, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
            catch (Exception ex)
            {
                Log("ERROR", "EnableBlur failed: " + ex.Message);
            }
        }

        #endregion

        #region Auto-Refresh (Hot Reload)

        private void SetupAutoRefresh(string xmlPath)
        {
            try
            {
                string fullPath = Path.GetFullPath(xmlPath);
                string directory = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);

                _fileWatcher = new FileSystemWatcher();
                _fileWatcher.Path = directory;
                _fileWatcher.Filter = fileName;
                _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;

                Log("WATCH", "Monitoring: " + fileName);
            }
            catch (Exception ex)
            {
                Log("ERROR", "File watcher setup failed: " + ex.Message);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if ((DateTime.Now - _lastReload).TotalMilliseconds < FILE_WATCH_DEBOUNCE_MS)
                return;
            _lastReload = DateTime.Now;

            Thread.Sleep(100);

            Log("RELOAD", "File change detected: " + e.Name);

            if (OnHotReload != null)
            {
                try { OnHotReload.Invoke(e.FullPath); } catch { }
            }

            if (_mainWindow != null)
            {
                _mainWindow.Dispatcher.BeginInvoke(new Action(delegate
                {
                    try
                    {
                        ReloadUI();
                        Log("RELOAD", "Hot reload successful!");
                        NotifyPlugins("hotReload", e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR", "Hot reload failed: " + ex.Message);
                    }
                }));
            }
        }

        public void ReloadUI()
        {
            if (string.IsNullOrEmpty(CurrentXmlPath)) return;

            // Clear old data
            lock (_controlLock) { _controls.Clear(); }
            lock (_handlerLock) { _eventHandlers.Clear(); }

            Thread.Sleep(50);

            try
            {
                // Reload XML
                _appDoc = new XmlDocument();
                _appDoc.Load(CurrentXmlPath);

                XmlNode root = _appDoc.DocumentElement;

                // Re-parse logic
                XmlNode logicNode = root.SelectSingleNode("Logic");
                if (logicNode != null)
                {
                    ParseLogic(logicNode);
                }

                // Re-parse styles
                XmlNode stylesNode = root.SelectSingleNode("Styles");
                if (stylesNode != null)
                {
                    _wpfUI.ParseStyles(stylesNode);
                }

                // Re-parse bindings
                XmlNode bindingsNode = root.SelectSingleNode("Bindings");
                if (bindingsNode != null)
                {
                    ParseBindings(bindingsNode);
                }

                // Re-create UI
                XmlNode uiNode = root.SelectSingleNode("UI");
                if (uiNode != null)
                {
                    Window newWindow = _wpfUI.CreateWindow(root, uiNode);

                    if (newWindow != null && newWindow.Content != null)
                    {
                        _mainWindow.Content = newWindow.Content;
                        _mainWindow.Title = newWindow.Title;

                        RegisterControlsFromContent(_mainWindow);
                        _wpfUI.WireUpEventHandlers(_mainWindow, uiNode);

                        // Re-process bindings
                        ProcessAllBindings();
                    }
                }

                lock (_stateLock)
                {
                    _state["_controlCount"] = _controls.Count;
                    _state["_handlerCount"] = _eventHandlers.Count;
                }

                WriteStateToFile();
            }
            catch (Exception ex)
            {
                Log("ERROR", "Reload error: " + ex.Message);
                throw;
            }
        }

        private void RegisterControlsFromContent(Window window)
        {
            if (window.Content is FrameworkElement)
            {
                RegisterControlsRecursive((FrameworkElement)window.Content);
            }
        }

        private void RegisterControlsRecursive(FrameworkElement element)
{
    if (element == null) return;

    if (!string.IsNullOrEmpty(element.Name))
    {
        lock (_controlLock) { _controls[element.Name] = element; }
    }

    if (element is Panel)
    {
        Panel panel = (Panel)element;
        foreach (UIElement child in panel.Children)
        {
            if (child is FrameworkElement)
            {
                RegisterControlsRecursive((FrameworkElement)child);
            }
        }
    }
    else if (element is ScrollViewer)
    {
        ScrollViewer sv = (ScrollViewer)element;
        if (sv.Content is FrameworkElement)
        {
            RegisterControlsRecursive((FrameworkElement)sv.Content);
        }
    }
    else if (element is ContentControl)
    {
        ContentControl cc = (ContentControl)element;
        if (cc.Content is FrameworkElement)
        {
            RegisterControlsRecursive((FrameworkElement)cc.Content);
        }
    }
    else if (element is Decorator)
    {
        Decorator dec = (Decorator)element;
        if (dec.Child is FrameworkElement)
        {
            RegisterControlsRecursive((FrameworkElement)dec.Child);
        }
    }
    else if (element is ItemsControl)
    {
        ItemsControl ic = (ItemsControl)element;
        for (int i = 0; i < ic.Items.Count; i++)
        {
            if (ic.Items[i] is FrameworkElement)
            {
                RegisterControlsRecursive((FrameworkElement)ic.Items[i]);
            }
        }
    }
}
        #endregion

        #region Control Management

        /// <summary>
        /// Register a control with the engine
        /// </summary>
        public void RegisterControl(string name, FrameworkElement control)
        {
            lock (_controlLock)
            {
                _controls[name] = control;
            }
        }

        /// <summary>
        /// Get control by name (thread-safe copy check)
        /// </summary>
        public FrameworkElement GetControl(string name)
        {
            lock (_controlLock)
            {
                if (_controls.ContainsKey(name))
                {
                    return _controls[name];
                }
            }
            return null;
        }

        /// <summary>
        /// Direct control access (no copy, faster)
        /// </summary>
        internal FrameworkElement GetControlDirect(string name)
        {
            lock (_controlLock)
            {
                if (_controls.ContainsKey(name))
                {
                    return _controls[name];
                }
            }
            return null;
        }

        /// <summary>
        /// Set a property on a control with proper type conversion
        /// </summary>
        public void SetControlProperty(string controlName, string property, object value)
{
    FrameworkElement control = GetControlDirect(controlName);
    if (control == null)
    {
        Log("WARN", "Control not found: " + controlName);
        return;
    }

    // Ensure UI thread
    if (!control.Dispatcher.CheckAccess())
    {
        string capturedName = controlName;
        string capturedProp = property;
        object capturedValue = value;
        control.Dispatcher.Invoke(new Action(delegate
        {
            FrameworkElement ctrl = GetControlDirect(capturedName);
            if (ctrl != null)
            {
                SetControlPropertyDirect(ctrl, capturedProp, capturedValue);
            }
        }));
        return;
    }

    SetControlPropertyDirect(control, property, value);
}

        /// <summary>
        /// Direct property set (must be on UI thread)
        /// </summary>
        private void SetControlPropertyDirect(FrameworkElement control, string property, object value)
{
    if (control == null) return;

    string strValue = value != null ? value.ToString() : "";

    try
    {
        // Direct known properties first (faster and more reliable)
        switch (property)
        {
            case "Text":
                if (control is TextBlock)
                {
                    ((TextBlock)control).Text = strValue;
                    return;
                }
                if (control is TextBox)
                {
                    ((TextBox)control).Text = strValue;
                    return;
                }
                break;

            case "Content":
                if (control is ContentControl)
                {
                    ((ContentControl)control).Content = strValue;
                    return;
                }
                break;

            case "Visibility":
                string lower = strValue.ToLower();
                if (lower == "visible" || lower == "true")
                    control.Visibility = Visibility.Visible;
                else if (lower == "collapsed" || lower == "false")
                    control.Visibility = Visibility.Collapsed;
                else if (lower == "hidden")
                    control.Visibility = Visibility.Hidden;
                return;

            case "IsEnabled":
                control.IsEnabled = strValue.ToLower() == "true" || strValue == "1";
                return;

            case "Background":
                if (control is Control)
                {
                    try
                    {
                        ((Control)control).Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(strValue));
                    }
                    catch { }
                    return;
                }
                if (control is Panel)
                {
                    try
                    {
                        ((Panel)control).Background = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(strValue));
                    }
                    catch { }
                    return;
                }
                break;

            case "Foreground":
                if (control is Control)
                {
                    try
                    {
                        ((Control)control).Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(strValue));
                    }
                    catch { }
                    return;
                }
                if (control is TextBlock)
                {
                    try
                    {
                        ((TextBlock)control).Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(strValue));
                    }
                    catch { }
                    return;
                }
                break;

            case "FontSize":
                double fontSize;
                if (double.TryParse(strValue, NumberStyles.Any, INV, out fontSize))
                {
                    if (control is Control)
                    {
                        ((Control)control).FontSize = fontSize;
                        return;
                    }
                    if (control is TextBlock)
                    {
                        ((TextBlock)control).FontSize = fontSize;
                        return;
                    }
                }
                break;

            case "Width":
                double w;
                if (double.TryParse(strValue, NumberStyles.Any, INV, out w))
                {
                    control.Width = w;
                    return;
                }
                break;

            case "Height":
                double h;
                if (double.TryParse(strValue, NumberStyles.Any, INV, out h))
                {
                    control.Height = h;
                    return;
                }
                break;

            case "Opacity":
                double op;
                if (double.TryParse(strValue, NumberStyles.Any, INV, out op))
                {
                    control.Opacity = op;
                    return;
                }
                break;
        }

        // Fallback: reflection
        PropertyInfo prop = control.GetType().GetProperty(property);
        if (prop != null && prop.CanWrite)
        {
            object convertedValue = ConvertPropertyValue(value, prop.PropertyType);
            prop.SetValue(control, convertedValue, null);
        }
    }
    catch (Exception ex)
    {
        Log("ERROR", "SetProperty failed [" + control.Name + "." + property + "]: " + ex.Message);
    }
}
        /// <summary>
        /// Convert value to target property type
        /// </summary>
        private object ConvertPropertyValue(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            string strValue = value.ToString();

            // WPF-specific conversions
            if (targetType == typeof(Brush) || targetType == typeof(SolidColorBrush))
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(strValue));
                }
                catch { return Brushes.Transparent; }
            }

            if (targetType == typeof(Thickness))
            {
                string[] parts = strValue.Split(',');
                if (parts.Length == 1)
                {
                    double v = ParseDouble(parts[0], 0);
                    return new Thickness(v);
                }
                if (parts.Length == 2)
                {
                    return new Thickness(ParseDouble(parts[0], 0), ParseDouble(parts[1], 0),
                        ParseDouble(parts[0], 0), ParseDouble(parts[1], 0));
                }
                if (parts.Length == 4)
                {
                    return new Thickness(ParseDouble(parts[0], 0), ParseDouble(parts[1], 0),
                        ParseDouble(parts[2], 0), ParseDouble(parts[3], 0));
                }
            }

            if (targetType == typeof(CornerRadius))
            {
                double r = ParseDouble(strValue, 0);
                return new CornerRadius(r);
            }

            if (targetType == typeof(Visibility))
            {
                string lower = strValue.ToLower();
                if (lower == "visible" || lower == "true") return Visibility.Visible;
                if (lower == "collapsed" || lower == "false") return Visibility.Collapsed;
                if (lower == "hidden") return Visibility.Hidden;
            }

            if (targetType == typeof(FontWeight))
            {
                string lower = strValue.ToLower();
                if (lower == "bold") return FontWeights.Bold;
                if (lower == "normal") return FontWeights.Normal;
                if (lower == "light") return FontWeights.Light;
                if (lower == "thin") return FontWeights.Thin;
                if (lower == "semibold") return FontWeights.SemiBold;
                if (lower == "extrabold") return FontWeights.ExtraBold;
            }

            if (targetType == typeof(HorizontalAlignment))
            {
                return (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), strValue, true);
            }

            if (targetType == typeof(VerticalAlignment))
            {
                return (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), strValue, true);
            }

            // Standard conversions
            try
            {
                return Convert.ChangeType(value, targetType, INV);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Get a property value from a control
        /// </summary>
        public object GetControlProperty(string controlName, string property)
        {
            FrameworkElement control = GetControlDirect(controlName);
            if (control == null) return null;

            try
            {
                PropertyInfo prop = control.GetType().GetProperty(property);
                if (prop != null)
                {
                    return prop.GetValue(control, null);
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region Handler Execution

        public void ExecuteHandler(string handlerName)
        {
            ExecuteHandler(handlerName, null);
        }

        public void ExecuteHandler(string handlerName, object sender, NimbusEvent evt = null)
        {
            if (_debugSwitches.ContainsKey("LogEvents") && _debugSwitches["LogEvents"])
            {
                Log("HANDLER", "Executing: " + handlerName + (evt != null ? " [" + evt.Type + "]" : ""));
            }

            XmlNode handler = null;
            lock (_handlerLock)
            {
                if (_eventHandlers.ContainsKey(handlerName))
                {
                    handler = _eventHandlers[handlerName];
                }
            }

            if (handler != null)
            {
                Stopwatch sw = null;
                if (_debugSwitches.ContainsKey("LogPerformance") && _debugSwitches["LogPerformance"])
                {
                    sw = Stopwatch.StartNew();
                }

                // Pass event to logic runner if available
                if (evt != null)
                {
                    _logicRunner.Execute(handler, sender, evt);
                }
                else
                {
                    _logicRunner.Execute(handler, sender);
                }

                if (sw != null)
                {
                    sw.Stop();
                    Log("PERF", handlerName + " executed in " + sw.ElapsedMilliseconds + "ms");
                }

                NotifyPlugins("handlerExecuted", handlerName);
            }
            else
            {
                Log("WARN", "Handler not found: " + handlerName);
            }
        }

        #endregion

        #region Variable Operations (with Binding Support)

        public void SetVariable(string name, object value)
        {
            object oldValue = null;

            lock (_stateLock)
            {
                if (_state.ContainsKey(name))
                {
                    oldValue = _state[name];
                }

                _state[name] = value;
            }

            _variables[name] = value;

            // Trigger bindings
            ProcessBindings(name, value);

            // Fire state changed event
            if (OnStateChanged != null)
            {
                try { OnStateChanged.Invoke(name, oldValue, value); } catch { }
            }

            if (_debugSwitches.ContainsKey("LogState") && _debugSwitches["LogState"])
            {
                Log("VAR", name + " = " + (value != null ? value.ToString() : "null"));
            }

            NotifyPlugins("stateChanged", new KeyValuePair<string, object>(name, value));
        }

        public object GetVariable(string name)
        {
            lock (_stateLock)
            {
                if (_state.ContainsKey(name))
                    return _state[name];
            }
            return null;
        }

        public void IncrementVariable(string name, object value)
        {
            lock (_stateLock)
            {
                if (!_state.ContainsKey(name))
                    _state[name] = 0;

                double current = ParseDouble(_state[name].ToString(), 0);
                double addValue = ParseDouble(value.ToString(), 0);
                double result = current + addValue;

                // Keep as int if both are integers
                if (result == Math.Floor(result) && addValue == Math.Floor(addValue))
                {
                    _state[name] = (int)result;
                    _variables[name] = (int)result;
                }
                else
                {
                    _state[name] = result;
                    _variables[name] = result;
                }
            }

            ProcessBindings(name, GetVariable(name));
        }

        public void DecrementVariable(string name, object value)
        {
            lock (_stateLock)
            {
                if (!_state.ContainsKey(name))
                    _state[name] = 0;

                double current = ParseDouble(_state[name].ToString(), 0);
                double subValue = ParseDouble(value.ToString(), 0);
                double result = current - subValue;

                if (result == Math.Floor(result) && subValue == Math.Floor(subValue))
                {
                    _state[name] = (int)result;
                    _variables[name] = (int)result;
                }
                else
                {
                    _state[name] = result;
                    _variables[name] = result;
                }
            }

            ProcessBindings(name, GetVariable(name));
        }

        public void MultiplyVariable(string name, object value)
        {
            lock (_stateLock)
            {
                if (!_state.ContainsKey(name))
                    _state[name] = 0;

                double current = ParseDouble(_state[name].ToString(), 0);
                double mulValue = ParseDouble(value.ToString(), 0);
                _state[name] = current * mulValue;
                _variables[name] = _state[name];
            }

            ProcessBindings(name, GetVariable(name));
        }

        public void DivideVariable(string name, object value)
        {
            lock (_stateLock)
            {
                if (!_state.ContainsKey(name))
                    _state[name] = 0;

                double current = ParseDouble(_state[name].ToString(), 0);
                double divValue = ParseDouble(value.ToString(), 0);

                if (divValue != 0)
                {
                    _state[name] = current / divValue;
                    _variables[name] = _state[name];
                }
            }

            ProcessBindings(name, GetVariable(name));
        }

        #endregion

        #region State Snapshots

        public Dictionary<string, object> GetStateSnapshot()
        {
            lock (_stateLock) { return new Dictionary<string, object>(_state); }
        }

        public List<string> GetControlsList()
        {
            lock (_controlLock) { return new List<string>(_controls.Keys); }
        }

        public List<string> GetHandlersList()
        {
            lock (_handlerLock) { return new List<string>(_eventHandlers.Keys); }
        }

        #endregion

        #region Utility Methods

        public object ConvertValue(string value, string type)
        {
            switch (type.ToLower())
            {
                case "int":
                case "integer":
                    int intVal;
                    int.TryParse(value, out intVal);
                    return intVal;
                case "double":
                case "float":
                case "number":
                    return ParseDouble(value, 0);
                case "bool":
                case "boolean":
                    return value.ToLower() == "true" || value == "1";
                case "list":
                case "array":
                    List<object> list = new List<object>();
                    string[] items = value.Split(',');
                    foreach (string item in items)
                    {
                        list.Add(item.Trim());
                    }
                    return list;
                default:
                    return value;
            }
        }

        /// <summary>
        /// Culture-invariant double parsing
        /// </summary>
        public static double ParseDouble(string value, double defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            double result;
            if (double.TryParse(value, NumberStyles.Any, INV, out result))
                return result;

            // Try with comma as decimal separator
            if (double.TryParse(value.Replace(',', '.'), NumberStyles.Any, INV, out result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Culture-invariant int parsing
        /// </summary>
        public static int ParseInt(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            int result;
            if (int.TryParse(value, out result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Raise error event
        /// </summary>
        private void RaiseError(string message)
        {
            if (OnError != null)
            {
                try { OnError.Invoke(message); } catch { }
            }
        }

        /// <summary>
        /// Get main window reference
        /// </summary>
        public Window GetMainWindow()
        {
            return _mainWindow;
        }

        /// <summary>
        /// Get XmlParser reference (for subsystems)
        /// </summary>
        public XmlParser GetXmlParser()
        {
            return _xmlParser;
        }

        /// <summary>
        /// Get LogicRunner reference (for subsystems)
        /// </summary>
        public LogicRunner GetLogicRunner()
        {
            return _logicRunner;
        }

        #endregion
    }

    #region Plugin Interface

    /// <summary>
    /// Interface for Nimbus plugins
    /// Implement this in your plugin .cs files in the plugins/ folder
    /// </summary>
    public interface INimbusPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }

        void OnLoad(WpfEngine engine);
        void OnUnload(WpfEngine engine);
        void OnEvent(WpfEngine engine, string eventName, object data);
    }

    #endregion

    #region Data Classes

    public enum BlurType
    {
        None,
        Blur,
        Acrylic,
        Transparent
    }
    public enum PopupType
{
    Alert,
    Confirm,
    Modal,
    Drawer,
    Toast,
    BottomSheet,
    ContextMenu,
    Tooltip
}

public enum PopupPosition
{
    Center,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Mouse
}

public enum PopupAnimation
{
    None,
    Fade,
    SlideUp,
    SlideDown,
    SlideLeft,
    SlideRight,
    Scale,
    Bounce
}
    public class OSInfo
    {
        public string Name { get; private set; }
        public int Build { get; private set; }
        public Version Version { get; private set; }

        public bool IsWindows7 { get; private set; }
        public bool IsWindows8 { get; private set; }
        public bool IsWindows10 { get; private set; }
        public bool IsWindows11 { get; private set; }

        public bool SupportsBlur { get; private set; }
        public bool SupportsAcrylic { get; private set; }
        public bool SupportsMica { get; private set; }

        public OSInfo()
        {
            Version = Environment.OSVersion.Version;
            Build = Version.Build;

            if (Version.Major == 6 && Version.Minor == 1)
            {
                Name = "Windows 7";
                IsWindows7 = true;
            }
            else if (Version.Major == 6 && (Version.Minor == 2 || Version.Minor == 3))
            {
                Name = "Windows 8";
                IsWindows8 = true;
            }
            else if (Version.Major == 10 && Build < 22000)
            {
                Name = "Windows 10";
                IsWindows10 = true;
                SupportsBlur = Build >= 10240;
                SupportsAcrylic = Build >= 17134;
            }
            else if (Version.Major == 10 && Build >= 22000)
            {
                Name = "Windows 11";
                IsWindows11 = true;
                SupportsBlur = true;
                SupportsAcrylic = true;
                SupportsMica = true;
            }
            else
            {
                Name = "Unknown (" + Version.Major + "." + Version.Minor + ")";
            }
        }
    }
    public class PopupOptions
{
    public PopupType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Icon { get; set; }
    public PopupPosition Position { get; set; }
    public PopupAnimation Animation { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Background { get; set; }
    public string Foreground { get; set; }
    public string BorderColor { get; set; }
    public double CornerRadius { get; set; }
    public string ButtonText { get; set; }
    public string ButtonBackground { get; set; }
    public bool ShowCloseButton { get; set; }
    public bool ShowOverlay { get; set; }
    public string OverlayColor { get; set; }
    public double OverlayOpacity { get; set; }
    public int AutoCloseMs { get; set; }
    public bool Draggable { get; set; }
}

public class ConfirmOptions
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string Icon { get; set; }
    public PopupPosition Position { get; set; }
    public PopupAnimation Animation { get; set; }
    public string Background { get; set; }
    public string Foreground { get; set; }
    public string YesText { get; set; }
    public string NoText { get; set; }
    public string YesBackground { get; set; }
    public string NoBackground { get; set; }
    public bool Draggable { get; set; }
    public Action OnYes { get; set; }
    public Action OnNo { get; set; }
}

public class ModalOptions
{
    public string Name { get; set; }
    public string Title { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public PopupPosition Position { get; set; }
    public PopupAnimation Animation { get; set; }
    public string Background { get; set; }
    public string Foreground { get; set; }
    public string BorderColor { get; set; }
    public double CornerRadius { get; set; }
    public bool ShowCloseButton { get; set; }
    public bool ShowOverlay { get; set; }
    public bool CloseOnOverlayClick { get; set; }
    public bool Draggable { get; set; }
    public bool Resizable { get; set; }
    public XmlNode ContentNode { get; set; }
    public object Sender { get; set; }
}

public class DrawerOptions
{
    public string Name { get; set; }
    public DrawerSide Side { get; set; }
    public double Width { get; set; }
    public string Height { get; set; }
    public string Background { get; set; }
    public PopupAnimation Animation { get; set; }
    public int DurationMs { get; set; }
    public bool ShowOverlay { get; set; }
    public bool CloseOnOverlayClick { get; set; }
    public XmlNode ContentNode { get; set; }
    public object Sender { get; set; }
}

public class ToastOptions
{
    public string Message { get; set; }
    public string Type { get; set; }
    public string Icon { get; set; }
    public PopupPosition Position { get; set; }
    public int DurationMs { get; set; }
    public PopupAnimation Animation { get; set; }
}

public class BottomSheetOptions
{
    public string Name { get; set; }
    public string Height { get; set; }
    public string MaxHeight { get; set; }
    public string Background { get; set; }
    public string CornerRadius { get; set; }
    public bool ShowHandle { get; set; }
    public PopupAnimation Animation { get; set; }
    public int DurationMs { get; set; }
    public bool ShowOverlay { get; set; }
    public bool SwipeToClose { get; set; }
    public XmlNode ContentNode { get; set; }
    public object Sender { get; set; }
}

public class ContextMenuOptions
{
    public PopupPosition Position { get; set; }
    public string Background { get; set; }
    public string Foreground { get; set; }
    public string BorderColor { get; set; }
    public double CornerRadius { get; set; }
    public PopupAnimation Animation { get; set; }
    public bool ShowShadow { get; set; }
    public List<ContextMenuItem> Items { get; set; }
    public object Sender { get; set; }
}

public class ContextMenuItem
{
    public string Text { get; set; }
    public string Icon { get; set; }
    public string Shortcut { get; set; }
    public string Handler { get; set; }
    public bool Disabled { get; set; }
    public bool Danger { get; set; }
    public bool IsSeparator { get; set; }
}

public enum DrawerSide
{
    Left,
    Right,
    Top,
    Bottom
}
    public class DevLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
    }

    public class BindingInfo
    {
        public string StateKey { get; set; }
        public string ControlName { get; set; }
        public string Property { get; set; }
        public string Format { get; set; }
        public bool IsActive { get; set; }
    }

    public class CacheEntry
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public int TTLSeconds { get; set; }
        public int HitCount { get; set; }
    }

    public class CacheStats
    {
        public int TotalEntries { get; set; }
        public int TotalHits { get; set; }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
    public class NimbusMathPlugin : INimbusPlugin
{
    public string Name { get { return "MathPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Math functions: abs, min, max, pow, sqrt, round, random"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("math.abs", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Abs(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.min", delegate(string args)
        {
            string[] parts = args.Split(',');
            if (parts.Length >= 2)
            {
                double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
                double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
                return Math.Min(a, b).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return "0";
        });

        engine.RegisterFunction("math.max", delegate(string args)
        {
            string[] parts = args.Split(',');
            if (parts.Length >= 2)
            {
                double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
                double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
                return Math.Max(a, b).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return "0";
        });

        engine.RegisterFunction("math.pow", delegate(string args)
        {
            string[] parts = args.Split(',');
            if (parts.Length >= 2)
            {
                double baseVal = WpfEngine.ParseDouble(parts[0].Trim(), 0);
                double exp = WpfEngine.ParseDouble(parts[1].Trim(), 0);
                return Math.Pow(baseVal, exp).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return "0";
        });

        engine.RegisterFunction("math.sqrt", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Sqrt(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.round", delegate(string args)
        {
            string[] parts = args.Split(',');
            double v = WpfEngine.ParseDouble(parts[0].Trim(), 0);
            int decimals = parts.Length > 1 ? WpfEngine.ParseInt(parts[1].Trim(), 0) : 0;
            return Math.Round(v, decimals).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.random", delegate(string args)
        {
            Random rnd = new Random();
            string[] parts = args.Split(',');
            if (parts.Length >= 2)
            {
                int min = WpfEngine.ParseInt(parts[0].Trim(), 0);
                int max = WpfEngine.ParseInt(parts[1].Trim(), 100);
                return rnd.Next(min, max + 1).ToString();
            }
            return rnd.Next(0, 101).ToString();
        });

        engine.RegisterFunction("math.floor", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Floor(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.ceil", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Ceiling(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.sin", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Sin(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.cos", delegate(string args)
        {
            double v = WpfEngine.ParseDouble(args, 0);
            return Math.Cos(v).ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        engine.RegisterFunction("math.pi", delegate(string args)
        {
            return Math.PI.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        // NimbusMathPlugin.OnLoad ichiga qo'shing (mavjud bo'lmasa):

engine.RegisterFunction("math.add", delegate(string args)
{
    string[] parts = args.Split(',');
    if (parts.Length >= 2)
    {
        double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
        double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
        return (a + b).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    return "0";
});

engine.RegisterFunction("math.subtract", delegate(string args)
{
    string[] parts = args.Split(',');
    if (parts.Length >= 2)
    {
        double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
        double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
        return (a - b).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    return "0";
});

engine.RegisterFunction("math.multiply", delegate(string args)
{
    string[] parts = args.Split(',');
    if (parts.Length >= 2)
    {
        double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
        double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
        return (a * b).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    return "0";
});

engine.RegisterFunction("math.divide", delegate(string args)
{
    string[] parts = args.Split(',');
    if (parts.Length >= 2)
    {
        double a = WpfEngine.ParseDouble(parts[0].Trim(), 0);
        double b = WpfEngine.ParseDouble(parts[1].Trim(), 0);
        if (b != 0) return (a / b).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return "NaN";
    }
    return "0";
});
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusStringPlugin : INimbusPlugin
{
    public string Name { get { return "StringPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "String functions: upper, lower, trim, length, contains, split, join"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("string.upper", delegate(string args) { return args.ToUpper(); });
        engine.RegisterFunction("string.lower", delegate(string args) { return args.ToLower(); });
        engine.RegisterFunction("string.trim", delegate(string args) { return args.Trim(); });

        engine.RegisterFunction("string.length", delegate(string args)
        {
            return args.Length.ToString();
        });

        engine.RegisterFunction("string.contains", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length >= 2)
                return parts[0].Contains(parts[1].Trim()) ? "true" : "false";
            return "false";
        });

        engine.RegisterFunction("string.startswith", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length >= 2)
                return parts[0].StartsWith(parts[1].Trim()) ? "true" : "false";
            return "false";
        });

        engine.RegisterFunction("string.endswith", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length >= 2)
                return parts[0].EndsWith(parts[1].Trim()) ? "true" : "false";
            return "false";
        });

        engine.RegisterFunction("string.replace", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 3);
            if (parts.Length >= 3)
                return parts[0].Replace(parts[1].Trim(), parts[2].Trim());
            return args;
        });

        engine.RegisterFunction("string.reverse", delegate(string args)
        {
            char[] chars = args.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        });

        engine.RegisterFunction("string.repeat", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length >= 2)
            {
                int count = WpfEngine.ParseInt(parts[1].Trim(), 1);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < count; i++) sb.Append(parts[0]);
                return sb.ToString();
            }
            return args;
        });

        engine.RegisterFunction("string.padleft", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 3);
            if (parts.Length >= 2)
            {
                int totalWidth = WpfEngine.ParseInt(parts[1].Trim(), 0);
                char padChar = parts.Length >= 3 && parts[2].Trim().Length > 0 ? parts[2].Trim()[0] : ' ';
                return parts[0].PadLeft(totalWidth, padChar);
            }
            return args;
        });

        engine.RegisterFunction("string.padright", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 3);
            if (parts.Length >= 2)
            {
                int totalWidth = WpfEngine.ParseInt(parts[1].Trim(), 0);
                char padChar = parts.Length >= 3 && parts[2].Trim().Length > 0 ? parts[2].Trim()[0] : ' ';
                return parts[0].PadRight(totalWidth, padChar);
            }
            return args;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusDatePlugin : INimbusPlugin
{
    public string Name { get { return "DatePlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Date/Time functions: now, format, add, diff"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("date.now", delegate(string args)
        {
            string format = string.IsNullOrEmpty(args) ? "yyyy-MM-dd HH:mm:ss" : args;
            return DateTime.Now.ToString(format);
        });

        engine.RegisterFunction("date.today", delegate(string args)
        {
            return DateTime.Today.ToString("yyyy-MM-dd");
        });

        engine.RegisterFunction("date.time", delegate(string args)
        {
            return DateTime.Now.ToString("HH:mm:ss");
        });

        engine.RegisterFunction("date.year", delegate(string args)
        {
            return DateTime.Now.Year.ToString();
        });

        engine.RegisterFunction("date.month", delegate(string args)
        {
            return DateTime.Now.Month.ToString();
        });

        engine.RegisterFunction("date.day", delegate(string args)
        {
            return DateTime.Now.Day.ToString();
        });

        engine.RegisterFunction("date.dayofweek", delegate(string args)
        {
            return DateTime.Now.DayOfWeek.ToString();
        });

        engine.RegisterFunction("date.timestamp", delegate(string args)
        {
            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ((long)span.TotalSeconds).ToString();
        });

        engine.RegisterFunction("date.adddays", delegate(string args)
        {
            string[] parts = args.Split(',');
            if (parts.Length >= 2)
            {
                DateTime dt;
                if (DateTime.TryParse(parts[0].Trim(), out dt))
                {
                    int days = WpfEngine.ParseInt(parts[1].Trim(), 0);
                    return dt.AddDays(days).ToString("yyyy-MM-dd");
                }
            }
            return DateTime.Now.ToString("yyyy-MM-dd");
        });

        engine.RegisterFunction("date.format", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length >= 2)
            {
                DateTime dt;
                if (DateTime.TryParse(parts[0].Trim(), out dt))
                {
                    return dt.ToString(parts[1].Trim());
                }
            }
            return args;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusFilePlugin : INimbusPlugin
{
    public string Name { get { return "FilePlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "File operations: read, write, exists, delete, list"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("file.read", delegate(string args)
        {
            try { return File.Exists(args) ? File.ReadAllText(args, Encoding.UTF8) : ""; }
            catch { return ""; }
        });

        engine.RegisterFunction("file.exists", delegate(string args)
        {
            return File.Exists(args) ? "true" : "false";
        });

        engine.RegisterFunction("file.direxists", delegate(string args)
        {
            return Directory.Exists(args) ? "true" : "false";
        });

        engine.RegisterFunction("file.size", delegate(string args)
        {
            try
            {
                if (File.Exists(args))
                {
                    FileInfo fi = new FileInfo(args);
                    return fi.Length.ToString();
                }
            }
            catch { }
            return "0";
        });

        engine.RegisterFunction("file.extension", delegate(string args)
        {
            return Path.GetExtension(args);
        });

        engine.RegisterFunction("file.name", delegate(string args)
        {
            return Path.GetFileName(args);
        });

        engine.RegisterFunction("file.directory", delegate(string args)
        {
            return Path.GetDirectoryName(args);
        });

        // Write command
        engine.RegisterCommand("file.write", delegate(XmlNode node, object sender)
        {
            string path = "";
            string content = "";
            if (node.Attributes != null)
            {
                if (node.Attributes["Path"] != null) path = node.Attributes["Path"].Value;
                if (node.Attributes["Content"] != null) content = node.Attributes["Content"].Value;
            }
            try
            {
                File.WriteAllText(path, content, Encoding.UTF8);
                engine.Log("FILE", "Written: " + path);
            }
            catch (Exception ex)
            {
                engine.Log("ERROR", "File write failed: " + ex.Message);
            }
            return true;
        });

        engine.RegisterCommand("file.delete", delegate(XmlNode node, object sender)
        {
            string path = "";
            if (node.Attributes != null && node.Attributes["Path"] != null)
                path = node.Attributes["Path"].Value;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    engine.Log("FILE", "Deleted: " + path);
                }
            }
            catch (Exception ex)
            {
                engine.Log("ERROR", "File delete failed: " + ex.Message);
            }
            return true;
        });

        engine.RegisterCommand("file.append", delegate(XmlNode node, object sender)
        {
            string path = "";
            string content = "";
            if (node.Attributes != null)
            {
                if (node.Attributes["Path"] != null) path = node.Attributes["Path"].Value;
                if (node.Attributes["Content"] != null) content = node.Attributes["Content"].Value;
            }
            try
            {
                File.AppendAllText(path, content, Encoding.UTF8);
                engine.Log("FILE", "Appended: " + path);
            }
            catch (Exception ex)
            {
                engine.Log("ERROR", "File append failed: " + ex.Message);
            }
            return true;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusJsonPlugin : INimbusPlugin
{
    public string Name { get { return "JsonPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Simple JSON parser: get value by key from JSON string"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("json.get", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length < 2) return "";

            string json = parts[0].Trim();
            string key = parts[1].Trim();

            return GetJsonValue(json, key);
        });

        engine.RegisterFunction("json.count", delegate(string args)
        {
            int count = 0;
            int depth = 0;
            bool inString = false;

            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];
                if (c == '"' && (i == 0 || args[i - 1] != '\\')) inString = !inString;
                if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    if (c == '}' || c == ']') depth--;
                    if (c == ',' && depth == 1) count++;
                }
            }

            if (args.Trim().Length > 2) count++;
            return count.ToString();
        });

        engine.RegisterFunction("json.has", delegate(string args)
        {
            string[] parts = args.Split(new char[] { ',' }, 2);
            if (parts.Length < 2) return "false";

            string json = parts[0].Trim();
            string key = parts[1].Trim();
            string val = GetJsonValue(json, key);

            return string.IsNullOrEmpty(val) ? "false" : "true";
        });
    }

    private string GetJsonValue(string json, string key)
    {
        string searchKey = "\"" + key + "\"";
        int keyIndex = json.IndexOf(searchKey);
        if (keyIndex < 0) return "";

        int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);
        if (colonIndex < 0) return "";

        int valueStart = colonIndex + 1;
        while (valueStart < json.Length && json[valueStart] == ' ') valueStart++;

        if (valueStart >= json.Length) return "";

        char firstChar = json[valueStart];

        if (firstChar == '"')
        {
            int endQuote = json.IndexOf('"', valueStart + 1);
            while (endQuote > 0 && json[endQuote - 1] == '\\')
            {
                endQuote = json.IndexOf('"', endQuote + 1);
            }
            if (endQuote > valueStart)
            {
                return json.Substring(valueStart + 1, endQuote - valueStart - 1);
            }
        }
        else if (firstChar == '{' || firstChar == '[')
        {
            int depth = 0;
            int i = valueStart;
            for (; i < json.Length; i++)
            {
                if (json[i] == '{' || json[i] == '[') depth++;
                if (json[i] == '}' || json[i] == ']') depth--;
                if (depth == 0) break;
            }
            return json.Substring(valueStart, i - valueStart + 1);
        }
        else
        {
            int endIndex = valueStart;
            while (endIndex < json.Length && json[endIndex] != ',' && json[endIndex] != '}' && json[endIndex] != ']')
            {
                endIndex++;
            }
            return json.Substring(valueStart, endIndex - valueStart).Trim();
        }

        return "";
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusUIPlugin : INimbusPlugin
{
    public string Name { get { return "UIPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "UI utilities: toast, notification, shake effect"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterCommand("ui.toast", delegate(XmlNode node, object sender)
        {
            string message = "";
            string duration = "3000";
            if (node.Attributes != null)
            {
                if (node.Attributes["Message"] != null) message = node.Attributes["Message"].Value;
                if (node.Attributes["Duration"] != null) duration = node.Attributes["Duration"].Value;
            }

            Window mainWindow = engine.GetMainWindow();
            if (mainWindow == null) return true;

            mainWindow.Dispatcher.Invoke(delegate
            {
                ShowToast(mainWindow, message, WpfEngine.ParseInt(duration, 3000));
            });

            return true;
        });

        engine.RegisterCommand("ui.shake", delegate(XmlNode node, object sender)
        {
            string controlName = "";
            if (node.Attributes != null && node.Attributes["Control"] != null)
                controlName = node.Attributes["Control"].Value;

            FrameworkElement control = engine.GetControl(controlName);
            if (control != null)
            {
                control.Dispatcher.Invoke(delegate
                {
                    ShakeControl(control);
                });
            }
            return true;
        });
    }

    private void ShowToast(Window parent, string message, int durationMs)
    {
        // Toast overlay
        Border toast = new Border();
        toast.Background = new SolidColorBrush(Color.FromArgb(230, 50, 50, 50));
        toast.CornerRadius = new CornerRadius(8);
        toast.Padding = new Thickness(20, 12, 20, 12);
        toast.HorizontalAlignment = HorizontalAlignment.Center;
        toast.VerticalAlignment = VerticalAlignment.Bottom;
        toast.Margin = new Thickness(0, 0, 0, 40);
        toast.Opacity = 0;

        toast.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = 12,
            ShadowDepth = 4,
            Opacity = 0.3
        };

        TextBlock text = new TextBlock();
        text.Text = message;
        text.Foreground = Brushes.White;
        text.FontSize = 14;
        text.FontFamily = new FontFamily("Segoe UI");
        toast.Child = text;

        // Add to window
        if (parent.Content is Grid)
        {
            Grid grid = (Grid)parent.Content;
            grid.Children.Add(toast);

            // Fade in
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Auto remove
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(durationMs);
            timer.Tick += delegate
            {
                timer.Stop();
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                fadeOut.Completed += delegate
                {
                    grid.Children.Remove(toast);
                };
                toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };
            timer.Start();
        }
    }

    private void ShakeControl(FrameworkElement control)
    {
        TranslateTransform transform = new TranslateTransform();
        control.RenderTransform = transform;

        DoubleAnimation shakeX = new DoubleAnimation();
        shakeX.From = -5;
        shakeX.To = 5;
        shakeX.Duration = TimeSpan.FromMilliseconds(50);
        shakeX.AutoReverse = true;
        shakeX.RepeatBehavior = new RepeatBehavior(4);
        shakeX.Completed += delegate
        {
            transform.X = 0;
        };

        transform.BeginAnimation(TranslateTransform.XProperty, shakeX);
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusNetPlugin : INimbusPlugin
{
    public string Name { get { return "NetPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Network utilities: ping, download, encode, decode"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("net.urlencode", delegate(string args)
        {
            return Uri.EscapeDataString(args);
        });

        engine.RegisterFunction("net.urldecode", delegate(string args)
        {
            return Uri.UnescapeDataString(args);
        });

        engine.RegisterFunction("net.base64encode", delegate(string args)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(args);
            return Convert.ToBase64String(bytes);
        });

        engine.RegisterFunction("net.base64decode", delegate(string args)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(args);
                return Encoding.UTF8.GetString(bytes);
            }
            catch { return ""; }
        });

        engine.RegisterFunction("net.hostname", delegate(string args)
        {
            return System.Net.Dns.GetHostName();
        });

        engine.RegisterCommand("net.download", delegate(XmlNode node, object sender)
        {
            string url = "";
            string path = "";
            if (node.Attributes != null)
            {
                if (node.Attributes["Url"] != null) url = node.Attributes["Url"].Value;
                if (node.Attributes["Path"] != null) path = node.Attributes["Path"].Value;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, path);
                    engine.Log("NET", "Downloaded: " + url + " -> " + path);
                }
            }
            catch (Exception ex)
            {
                engine.Log("ERROR", "Download failed: " + ex.Message);
            }
            return true;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusCryptoPlugin : INimbusPlugin
{
    public string Name { get { return "CryptoPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Crypto: md5, sha256, guid"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("crypto.md5", delegate(string args)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(args);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        });

        engine.RegisterFunction("crypto.sha256", delegate(string args)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(args);
                byte[] hashBytes = sha.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        });

        engine.RegisterFunction("crypto.guid", delegate(string args)
        {
            return Guid.NewGuid().ToString();
        });

        engine.RegisterFunction("crypto.shortid", delegate(string args)
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusClipboardPlugin : INimbusPlugin
{
    public string Name { get { return "ClipboardPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Clipboard operations"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterFunction("clipboard.get", delegate(string args)
        {
            try { return Clipboard.GetText(); }
            catch { return ""; }
        });

        engine.RegisterCommand("clipboard.set", delegate(XmlNode node, object sender)
        {
            string text = "";
            if (node.Attributes != null && node.Attributes["Text"] != null)
                text = node.Attributes["Text"].Value;

            try { Clipboard.SetText(text); }
            catch { }
            return true;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class NimbusDialogPlugin : INimbusPlugin
{
    public string Name { get { return "DialogPlugin"; } }
    public string Version { get { return "1.0"; } }
    public string Description { get { return "Dialog utilities: input, color picker, folder picker"; } }

    public void OnLoad(WpfEngine engine)
    {
        engine.RegisterCommand("dialog.input", delegate(XmlNode node, object sender)
        {
            string title = "Input";
            string message = "Enter value:";
            string defaultValue = "";
            string toState = "";

            if (node.Attributes != null)
            {
                if (node.Attributes["Title"] != null) title = node.Attributes["Title"].Value;
                if (node.Attributes["Message"] != null) message = node.Attributes["Message"].Value;
                if (node.Attributes["Default"] != null) defaultValue = node.Attributes["Default"].Value;
                if (node.Attributes["ToState"] != null) toState = node.Attributes["ToState"].Value;
            }

            Window inputWin = new Window();
            inputWin.Title = title;
            inputWin.Width = 400;
            inputWin.Height = 200;
            inputWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            inputWin.WindowStyle = WindowStyle.ToolWindow;
            inputWin.ResizeMode = ResizeMode.NoResize;
            inputWin.Background = new SolidColorBrush(Color.FromRgb(44, 44, 44));

            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(20);

            TextBlock msgBlock = new TextBlock();
            msgBlock.Text = message;
            msgBlock.Foreground = Brushes.White;
            msgBlock.FontSize = 14;
            msgBlock.FontFamily = new FontFamily("Segoe UI");
            msgBlock.Margin = new Thickness(0, 0, 0, 12);
            panel.Children.Add(msgBlock);

            TextBox inputBox = new TextBox();
            inputBox.Text = defaultValue;
            inputBox.FontSize = 14;
            inputBox.Padding = new Thickness(10, 8, 10, 8);
            inputBox.Margin = new Thickness(0, 0, 0, 16);
            panel.Children.Add(inputBox);

            StackPanel btnPanel = new StackPanel();
            btnPanel.Orientation = Orientation.Horizontal;
            btnPanel.HorizontalAlignment = HorizontalAlignment.Right;

            Button cancelBtn = new Button();
            cancelBtn.Content = "Cancel";
            cancelBtn.Width = 80;
            cancelBtn.Margin = new Thickness(0, 0, 8, 0);
            cancelBtn.Click += delegate { inputWin.DialogResult = false; inputWin.Close(); };
            btnPanel.Children.Add(cancelBtn);

            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Width = 80;
            okBtn.Click += delegate { inputWin.DialogResult = true; inputWin.Close(); };
            btnPanel.Children.Add(okBtn);

            panel.Children.Add(btnPanel);
            inputWin.Content = panel;

            inputBox.Focus();
            inputBox.SelectAll();

            bool? result = inputWin.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(toState))
            {
                engine.SetVariable(toState, inputBox.Text);
            }

            return true;
        });

        engine.RegisterCommand("dialog.folder", delegate(XmlNode node, object sender)
        {
            string toState = "";
            if (node.Attributes != null && node.Attributes["ToState"] != null)
                toState = node.Attributes["ToState"].Value;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(toState))
                {
                    engine.SetVariable(toState, dialog.SelectedPath);
                }
            }
            return true;
        });
    }

    public void OnUnload(WpfEngine engine) { }
    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}
public class XmlPluginAdapter : INimbusPlugin
{
    public string PluginName { get; set; }
    public string PluginVersion { get; set; }
    public string PluginDescription { get; set; }
    public Dictionary<string, XmlNode> Handlers { get; set; }
    public Dictionary<string, string> CommandMap { get; set; }

    public string Name { get { return PluginName; } }
    public string Version { get { return PluginVersion; } }
    public string Description { get { return PluginDescription; } }

    public XmlPluginAdapter()
    {
        Handlers = new Dictionary<string, XmlNode>();
        CommandMap = new Dictionary<string, string>();
    }

    public void OnLoad(WpfEngine engine)
    {
        // Register handlers as event handlers
        foreach (KeyValuePair<string, XmlNode> kvp in Handlers)
        {
            engine.EventHandlers[PluginName + "." + kvp.Key] = kvp.Value;
        }

        // Register commands
        foreach (KeyValuePair<string, string> kvp in CommandMap)
        {
            string cmdName = kvp.Key;
            string handlerName = PluginName + "." + kvp.Value;

            engine.RegisterCommand(cmdName, delegate(XmlNode node, object sender)
            {
                engine.ExecuteHandler(handlerName, sender);
                return true;
            });
        }
    }

    public void OnUnload(WpfEngine engine)
    {
        foreach (KeyValuePair<string, XmlNode> kvp in Handlers)
        {
            string key = PluginName + "." + kvp.Key;
            if (engine.EventHandlers.ContainsKey(key))
            {
                engine.EventHandlers.Remove(key);
            }
        }
    }

    public void OnEvent(WpfEngine engine, string eventName, object data) { }
}

    #endregion
}