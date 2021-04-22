@if not defined _echo echo off

echo NeeView DevPowerShell

set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
call :NORMALIZEPATH "%~dp0..\..\.."
set NVROOT=%RETVAL%
set NVTOOLS=%NVROOT%\Language\Tools
set PATH=%NVTOOLS%;%PATH%

for /f "usebackq delims=" %%i in (`%VSWHERE% -prerelease -latest -property installationPath`) do (
  if exist "%%i\Common7\Tools\vsdevcmd.bat" (
    call "%%i\Common7\Tools\vsdevcmd.bat" %* && powershell.exe -nologo -noexit -command .\Convert.ps1
    exit /b
  )
)

exit /b 2

:NORMALIZEPATH
  set RETVAL=%~dpfn1
  exit /B