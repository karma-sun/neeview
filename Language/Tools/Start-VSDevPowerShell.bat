@if not defined _echo echo off

set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

for /f "usebackq delims=" %%i in (`%VSWHERE% -prerelease -latest -property installationPath`) do (
  if exist "%%i\Common7\Tools\vsdevcmd.bat" (
    call "%%i\Common7\Tools\vsdevcmd.bat" %* && powershell.exe -nologo -noexit
    exit /b
  )
)

exit /b 2