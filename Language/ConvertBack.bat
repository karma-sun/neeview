@echo off
rem ---------------------------------
rem crete .xmls from Project Resources.resx
rem ---------------------------------

echo.
echo *** Overwrite Resources.xlsx. ***
echo.
echo really?
echo.
pause

echo make Resources.resx
ResxXlsxConv\ResxXlsxConv.exe -to-xlsx ..\NeeView\Properties\Resources.resx Resources.xlsx

if not "%ERRORLEVEL%" == "0" (
    echo **** ERROR ****
    pause
    exit 1
)

echo done.
exit 0
