@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

pushd "%CM3D2_MOD_DIR%"
pushd UnityInjector\Config\ShaderWorkbench
start ShaderCompiler.exe
popd

@rem http://stackoverflow.com/a/735603/2132223
ping -n 3 127.0.0.1 > nul

start CM3D2%CM3D2_PLATFORM%.exe
popd
