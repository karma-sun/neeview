@echo off
rem ---------------------------------
rem create Project Resources.resx from .xmls
rem ---------------------------------

echo make Resources.resx
ResxXlsxConv\ResxXlsxConv.exe -to-resx -trunc ..\NeeView\Properties\Resources.resx Resources.xlsx

if not "%ERRORLEVEL%" == "0" (
    echo **** ERROR ****
    pause
    exit 1
)

echo done.
exit 0
