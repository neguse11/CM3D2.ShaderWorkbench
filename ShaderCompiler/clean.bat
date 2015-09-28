@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

del /y "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench\ShaderCompiler.exe"

popd
