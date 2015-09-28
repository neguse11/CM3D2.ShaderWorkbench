@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set _7Z=..\7z\7za.exe

set SHARPDX_URL=http://sharpdx.org/upload/SharpDX-SDK-2.6.3.exe
set SHARPDX_FILE=SharpDX-SDK-2.6.3.exe

if not exist "%SHARPDX_FILE%" (
  powershell -Command "(New-Object Net.WebClient).DownloadFile('%SHARPDX_URL%', '%SHARPDX_FILE%')"
)
"%_7Z%" x -y SharpDX-SDK-2.6.3.exe Bin\DirectX11-net20\SharpDX.dll Bin\DirectX11-net20\SharpDX.D3DCompiler.dll

mkdir "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1
copy /y Bin\DirectX11-net20\*.* "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1

popd
