@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

call compile-patcher.bat || exit /b 1
call compile-managed.bat || exit /b 1

set "TYPE=/t:library"
set "OUT=%UNITY_INJECTOR_DIR%\CM3D2.ShaderWorkbench.Plugin.dll"
set SRCS="ShaderWorkbenchPlugin.cs" "DetailedException.cs"
set "OPTS=/r:CM3D2.ShaderWorkbench.Managed.dll"

call "..\scripts\csc-compile.bat" || exit /b 1

xcopy /y /d ..\UnityInjector\Config\ShaderWorkbench\*.* "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench"

popd
