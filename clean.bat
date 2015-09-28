@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo ShaderCompiler && call ShaderCompiler\clean.bat || goto error
echo.& echo ShaderWorkbench && call ShaderWorkbench\clean.bat || goto error
call .\patch.bat || goto error
echo.& echo 成功：全ファイルの削除に成功しました

popd
goto end

:error

echo.& echo 失敗：削除中にエラーが発生しました
exit /b 1

:end
