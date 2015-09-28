@echo on && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "ROOT=%~dp0"
for %%a in ("%ROOT%\.") do set "ROOT=%%~fa"

set _7Z_URL=http://sourceforge.net/projects/sevenzip/files/7-Zip/9.20/7za920.zip
set _7Z_FILE=7za920.zip

if not exist "%_7Z_FILE%" (
  powershell -Command "(New-Object Net.WebClient).DownloadFile('%_7Z_URL%', '%_7Z_FILE%')"
)
powershell -Command "$s=new-object -com shell.application;$z=$s.NameSpace('%ROOT%\%_7Z_FILE%');foreach($i in $z.items()){$s.Namespace('%ROOT%').copyhere($i,0x14)}"

popd
