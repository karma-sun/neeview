$candle = 'C:\Program Files (x86)\WiX Toolset v3.10\bin\candle.exe'
$light = 'C:\Program Files (x86)\WiX Toolset v3.10\bin\light.exe'

$name = 'NeeView'
$version = '1.15'

$target = "$name$version"


$packageDir = $target

$config = "$name.exe.config"
$packageAppendDir = $packageDir + ".append"


# error to break
trap { break }

# remove append folder
if (Test-Path $packageAppendDir)
{
	Remove-Item $packageAppendDir -Recurse
	Start-Sleep -m 100
}

# make append folder
$temp = New-Item $packageAppendDir -ItemType Directory

# make config for installer
[xml]$xml = Get-Content "$packageDir\$config"

$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
$add.value = 'True'

$xml.Save("$packageAppendDir\$config")

# make llicense.rtf
#pandoc -s -t rtf -o "$packageAppendDir\license.rtf" "$packageDir\LICENSE.html"


#-------------------------
# WiX
#-------------------------

#& $candle -d"BuildVersion=$version" -d"ContentDir=$target\\" -d"AppendDir=$target.append\\" -out "$target.append\\"  "$name.wxs" WixUI_InstallDirEx.wxs
& $candle -d"BuildVersion=$version" -d"ContentDir=$target\\" -d"AppendDir=$target.append\\" -out "$target.append\\"  WixSource\*.wxs

#& $light -out "$target.msi" -ext WixUIExtension -ext WixNetFxExtension -cultures:ja-JP "$target.append\$name.wixobj" "$target.append\WixUI_InstallDirEx.wixobj"
& $light -out "$target.msi" -ext WixUIExtension -ext WixNetFxExtension -cultures:ja-JP "$target.append\*.wixobj"
