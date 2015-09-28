@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo ShaderCompiler && call ShaderCompiler\compile.bat || goto error
echo.& echo ShaderWorkbench && call ShaderWorkbench\compile.bat || goto error

echo.& echo 成功：全ファイルのコンパイルに成功しました

popd
goto end

:error

echo.& echo 失敗：コンパイル中にエラーが発生しました
exit /b 1

:end
