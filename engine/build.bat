

@echo off
echo ========================================
echo   Nimbus Engine v3.0 - Build Script
echo ========================================
echo.

set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set WPF=C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF

if not exist "%CSC%" (
    echo ERROR: CSC.exe not found!
    pause
    exit /b 1
)

if not exist "%WPF%\PresentationCore.dll" (
    set WPF=C:\Windows\Microsoft.NET\Framework\v4.0.30319
)

if not exist "%WPF%\PresentationCore.dll" (
    echo ERROR: WPF DLLs not found!
    pause
    exit /b 1
)

if not exist "bin" mkdir bin

echo Compiling Nimbus Engine...
echo.

:: Collect all plugin .cs files
set PLUGIN_FILES=
if exist "plugins\*.cs" (
    for %%f in (plugins\*.cs) do (
        set PLUGIN_FILES=!PLUGIN_FILES! "%%f"
    )
)

:: Enable delayed expansion for plugin files
setlocal EnableDelayedExpansion

set PLUGIN_LIST=
if exist "plugins" (
    for %%f in (plugins\*.cs) do (
        set PLUGIN_LIST=!PLUGIN_LIST! "%%f"
    )
    echo Found external plugins: !PLUGIN_LIST!
)

"%CSC%" ^
/target:exe ^
/out:nimbus.exe ^
/optimize+ ^
/warn:0 ^
/platform:anycpu ^
/win32icon:logo.ico ^
/r:System.dll ^
/r:System.Core.dll ^
/r:System.Xml.dll ^
/r:System.Xml.Linq.dll ^
/r:System.Xaml.dll ^
/r:System.Net.dll ^
/r:System.Drawing.dll ^
/r:Microsoft.CSharp.dll ^
/r:"%WPF%\WindowsBase.dll" ^
/r:"%WPF%\PresentationCore.dll" ^
/r:"%WPF%\PresentationFramework.dll" ^
UILayout\Colors.cs ^
UILayout\Buttons.cs ^
UILayout\Inputs.cs ^
UILayout\Widgets.cs ^
UILayout\Typography.cs ^
UILayout\Layouts.cs ^
WpfEngine.cs ^
WpfUI.cs ^
XamlRenderer.cs ^
LogicRunner.cs ^
CSharpCompiler.cs ^
XmlParser.cs ^
DevToolsServer.cs ^
Program.cs ^
ComponentSystem.cs ^
IUIModule.cs ^
ModuleRenderer.cs ^
ModuleWindow.cs ^
!PLUGIN_LIST!

endlocal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   BUILD SUCCESS!
    echo   Output: bin\nimbus.exe
    echo ========================================
    echo.
    echo   Builtin plugins: 10
    if exist "plugins\*.cs" (
        echo   External plugins: compiled into binary
    )
    echo.
) else (
    echo.
    echo   BUILD FAILED!
    echo.
)
pause
