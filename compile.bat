@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo ShaderCompiler && call ShaderCompiler\compile.bat || goto error
echo.& echo ShaderWorkbench && call ShaderWorkbench\compile.bat || goto error

echo.& echo �����F�S�t�@�C���̃R���p�C���ɐ������܂���

popd
goto end

:error

echo.& echo ���s�F�R���p�C�����ɃG���[���������܂���
exit /b 1

:end
