@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

del /y "%UNITY_INJECTOR_DIR%\CM3D2.ShaderWorkbench.Plugin.dll"
del /y "%CM3D2_MOD_MANAGED_DIR%\CM3D2.ShaderWorkbench.Managed.dll"
del /y "%REIPATCHER_DIR%\Patches\CM3D2.ShaderWorkbench.Patcher.dll"

popd
