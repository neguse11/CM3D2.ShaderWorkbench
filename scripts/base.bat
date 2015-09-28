if not exist "%~dp0\..\config.bat" (
  echo �G���[�Fconfig.bat �����ݒ�̂��߁A�������I�����܂��B
  echo �ڂ����� README.md ���Q�Ƃ��Ă�������
  exit /b 1
)

call "%~dp0\..\config.bat" || exit /b 1


@rem
@rem INSTALL_PATH�Ƀ��W�X�g�����̃C���X�g�[���p�X������
@rem
set INSTALL_PATH_REG_KEY="HKCU\Software\KISS\�J�X�^�����C�h3D2"
set INSTALL_PATH_REG_VALUE=InstallPath
set INSTALL_PATH=

@rem http://stackoverflow.com/questions/445167/
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
    set "INSTALL_PATH=%%C"
)

if not exist "%INSTALL_PATH%\GameData\csv.arc" (
    set INSTALL_PATH=
)

if defined INSTALL_PATH (
    set "INSTALL_PATH=%INSTALL_PATH:~0,-1%"
)

if not defined CM3D2_VANILLA_DIR (
    set "CM3D2_VANILLA_DIR=INSTALL_PATH"
)



if not defined CM3D2_VANILLA_DIR (
  echo �G���[�Fconfig.bat����CM3D2_VANILLA_DIR��ݒ肵�Ă��������B
  exit /b 1
)

if not defined CM3D2_MOD_DIR (
  echo �G���[�Fconfig.bat����CM3D2_MOD_DIR��ݒ肵�Ă��������B
  exit /b 1
)

if not defined CM3D2_PLATFORM (
  echo �G���[�Fconfig.bat����CM3D2_PLATFORM��ݒ肵�Ă��������B
  exit /b 1
)

set "CM3D2_VANILLA_DATA_DIR=%CM3D2_VANILLA_DIR%\CM3D2%CM3D2_PLATFORM%_Data"
set "CM3D2_VANILLA_MANAGED_DIR=%CM3D2_VANILLA_DATA_DIR%\Managed"

set "CM3D2_MOD_DATA_DIR=%CM3D2_MOD_DIR%\CM3D2%CM3D2_PLATFORM%_Data"
set "CM3D2_MOD_MANAGED_DIR=%CM3D2_MOD_DATA_DIR%\Managed"

set "REIPATCHER_DIR=%CM3D2_MOD_DIR%\ReiPatcher"
set "UNITY_INJECTOR_DIR=%CM3D2_MOD_DIR%\UnityInjector"

set "OKIBA_LIB=%~dp0..\Lib"

set "RF=temp.rsp"

@rem
@rem CSC��csc.exe�̃p�X������
@rem
@rem https://gist.github.com/asm256/8f5472657c1675bdc77a
@rem https://support.microsoft.com/en-us/kb/318785
set CSC_REG_KEY="HKLM\SoftWare\Microsoft\NET Framework Setup\NDP\v3.5"
set CSC_REG_VALUE=InstallPath
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %CSC_REG_KEY% /v %CSC_REG_VALUE% 2^>nul`) do (
    set CSC_PATH=%%C
)
set "CSC=%CSC_PATH%\csc.exe"

if not defined OKIBA_BRANCH (
  set "OKIBA_BRANCH=master"
)

if not exist "%CM3D2_VANILLA_DIR%" (
  echo "�G���[�Fconfig.bat����CM3D2_VANILLA_DIR�������t�H���_�[�u%CM3D2_VANILLA_DIR%�v�����݂��܂���"
  exit /b 1
)

if not exist "%CM3D2_MOD_DIR%" (
  echo "�G���[�Fconfig.bat����CM3D2_MOD_DIR�������t�H���_�[�u%CM3D2_MOD_DIR%�v�����݂��܂���"
  exit /b 1
)

if not exist "%REIPATCHER_DIR%" (
  echo "�G���[�FReiPatcher�t�H���_�[�u%REIPATCHER_DIR%�v�����݂��܂���"
  exit /b 1
)

if not exist "%CM3D2_MOD_MANAGED_DIR%\UnityInjector.dll" (
  echo "�G���[�FUnityInjector.dll���t�H���_�[�u%CM3D2_MOD_MANAGED_DIR%�v���ɑ��݂��܂���"
  exit /b 1
)

if not exist "%UNITY_INJECTOR_DIR%" (
  echo "�G���[�FUnityInjector�t�H���_�[�u%UNITY_INJECTOR_DIR%�v�����݂��܂���"
  exit /b 1
)

if not exist "%CSC%" (
  echo �G���[�FC# �R���p�C���[�ucsc.exe�v�����݂��܂���
  exit /b 1
)
