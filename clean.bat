@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

echo.& echo ShaderCompiler && call ShaderCompiler\clean.bat || goto error
echo.& echo ShaderWorkbench && call ShaderWorkbench\clean.bat || goto error
call .\patch.bat || goto error
echo.& echo �����F�S�t�@�C���̍폜�ɐ������܂���

popd
goto end

:error

echo.& echo ���s�F�폜���ɃG���[���������܂���
exit /b 1

:end
