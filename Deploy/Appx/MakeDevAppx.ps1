# 開発用オレオレ署名APPX作成

$AppName = "NeeView"
$Appx = "_$AppName.appx"
$PackageFiles = "$AppName\PackageFiles"

$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x64"

# error to break
trap { break }
$ErrorActionPreference = "Stop"

# generate AppManifest
$content = Get-Content "Resources\AppxManifest.xml"
$content = $content -replace "%NAME%","$AppName"
$content = $content -replace "%PUBLISHER%","CN=NeeLaboratory"
$content = $content -replace "%VERSION%","1.2.3.4"
$content = $content -replace "%ARCH%", "x64"
$content | Out-File -Encoding UTF8 "$PackageFiles\AppxManifest.xml"

## re-package
Write-Host "`[Package] ...`n" -fore Cyan
& "$Win10SDK\makeappx.exe" pack /l /d "$PackageFiles" /p "$Appx"
if ($? -ne $true)
{
	throw "makeappx.exe error"
}

# signing
Write-Host "`[Sign] ...`n" -fore Cyan
& "$Win10SDK\signtool.exe" sign -f "_my.pfx" -fd SHA256 -v "$Appx"
if ($? -ne $true)
{
	throw "signtool.exe error"
}