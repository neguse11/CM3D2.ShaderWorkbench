@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

pushd "%REIPATCHER_DIR%"
del /q "%CM3D2_MOD_MANAGED_DIR%\Assembly-CSharp.dll.*.bak" >nul 2>&1
copy /y "%CM3D2_VANILLA_MANAGED_DIR%\Assembly-CSharp.dll" "%CM3D2_MOD_MANAGED_DIR%" >nul 2>&1 || ( echo �t�@�C���̃R�s�[�Ɏ��s���܂��� && exit /b 1 )
if not exist "%REIPATCHER_INI%" (
  echo.&echo ReiPatcher�̐ݒ�t�@�C�� %REIPATCHER_DIR%\%REIPATCHER_INI% �����݂��܂���
  goto error
)
.\ReiPatcher -c "%REIPATCHER_INI%" || goto error
popd

echo.& echo �����FReiPatcher�̃p�b�`�����ɐ������܂���

popd
goto end

:error

echo.& echo ���s�FReiPatcher�̃p�b�`�����Ɏ��s���܂���
exit /b 1

:end
