@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

call .\download.bat || exit /b 1

mkdir "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1
copy /y UnityInjector\Config\ShaderWorkbench\*.* "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1
copy /y SharpDX\Bin\DirectX11-net20\*.* "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1

popd
