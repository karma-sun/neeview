"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" .\NeeView.sln /p:Configuration=Release;Platform=x64 /m /t:Restore;Build
7z.exe a -r NeeView.7z .\NeeView\bin\x64\Release
