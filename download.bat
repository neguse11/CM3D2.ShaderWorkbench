@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo 7z && call 7z\download.bat || goto error
echo.& echo SharpDX && call SharpDX\download.bat || goto error

echo.& echo �����F�S�t�@�C���̃_�E�����[�h�ɐ������܂���

popd
goto end

:error

echo.& echo ���s�F�t�@�C���̃_�E�����[�h�Ɏ��s���܂���
exit /b 1

:end
