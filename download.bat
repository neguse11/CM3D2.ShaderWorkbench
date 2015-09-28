@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo 7z && call 7z\download.bat || goto error
echo.& echo SharpDX && call SharpDX\download.bat || goto error

echo.& echo 成功：全ファイルのダウンロードに成功しました

popd
goto end

:error

echo.& echo 失敗：ファイルのダウンロードに失敗しました
exit /b 1

:end
