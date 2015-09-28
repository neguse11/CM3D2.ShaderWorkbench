@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "TYPE=/t:exe"
set "OUT=%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench\ShaderCompiler.exe"
set SRCS="D3Dcompiler.cs" "Program.cs" "PseudoHex.cs" "ShaderFile.cs" "SimpleLexer.cs"
set "OPTS=/lib:..\SharpDX\Bin\DirectX11-net20 /r:SharpDX.dll /r:SharpDX.D3DCompiler.dll"

mkdir "%UNITY_INJECTOR_DIR%\Config\ShaderWorkbench" >nul 2>&1
call "%~dp0..\scripts\csc-compile.bat" || exit /b 1

popd
