# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx")]$Target = "All"
)

# error to break
trap { break }

$ErrorActionPreference = "stop"


#
$product = 'NeeView'
$config = 'Release'


#---------------------
# get fileversion
function Get-FileVersion($fileName)
{
	throw "not supported."

	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart

	"$major.$minor"
}



#---------------------
# get base vsersion
function Get-Version()
{
	$xml = [xml](Get-Content "Version.xml")
	return $xml.version
}


#---------------------
# get build count
function Get-BuildCount()
{
	# auto increment build version
	$xml = [xml](Get-Content "BuildCount.xml")
	return [int]$xml.build + 1
}

#---------------------
# set build count
function Set-BuildCount($buildCount)
{
	$xml = [xml](Get-Content "BuildCount.xml")
	$xml.build = [string]$buildCount
	$xml.Save("BuildCount.xml")
}


#--------------------
# replace keyword
function Replace-Content
{
	Param([string]$filepath, [string]$rep1, [string]$rep2)
	if ( $(Test-Path $filepath) -ne $True )
	{
		Write-Error "file not found"
		return
	}
	# input UTF8, output UTF8
	$file_contents = $(Get-Content -Encoding UTF8 $filepath) -replace $rep1, $rep2
	$file_contents | Out-File -Encoding UTF8 $filepath
}

#--------------------
# set AssemblyInfo.cs
function Set-AssemblyVersion($assemblyInfoFile, $title, $version)
{
    $content = Get-Content $assemblyInfoFile
   
    $content = $content -replace "AssemblyTitle\(.+\)", "AssemblyTitle(`"$title`")"
    $content = $content -replace "AssemblyVersion\(.+\)", "AssemblyVersion(`"$version`")"
    $content = $content -replace "AssemblyFileVersion\(.+\)", "AssemblyFileVersion(`"$version`")"

	$content | Out-File -Encoding UTF8 $assemblyInfoFile
}

#--------------------
# reset AssemblyInfo.cs
function Reset-AssemblyInfo($assemblyInfoFile)
{
	& git checkout $assemblyInfoFile 
}



#-----------------------
# variables
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Convert-Path "$scriptPath\.."
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$productx86Dir = "$projectDir\bin\x86\$config"
$productX64Dir = "$projectDir\bin\$config"
$assemblyInfoFile = "$projectDir\Properties\AssemblyInfo.cs"


#----------------------
# build
function Build-Project($arch, $assemblyVersion)
{
	if ($arch -eq "x86")
	{
		$platform = "x86"
		Set-AssemblyVersion $assemblyInfoFile "NeeViewS" $assemblyVersion
	}
	else
	{
		$platform = "Any CPU"
		Set-AssemblyVersion $assemblyInfoFile "NeeView" $assemblyVersion
	}

	$vswhere = "$solutionDir\Tools\vswhere.exe"

    $vspath = & $vswhere -property installationPath
    $msbuild = "$vspath\MSBuild\15.0\Bin\MSBuild.exe"
	& $msbuild $solution /p:Configuration=$config /p:Platform=$platform /t:Clean,Build
	if ($? -ne $true)
	{
		throw "build error"
	}
}




#----------------------
# package section
function New-Package($productDir, $packageDir)
{
	$packageLibraryDir = $packageDir + "\Libraries"

	# make package folder
	$temp = New-Item $packageDir -ItemType Directory
	$temp = New-Item $packageLibraryDir -ItemType Directory

	# copy
	Copy-Item "$productDir\$product.exe" $packageDir
	Copy-Item "$productDir\$product.exe.config" $packageDir
	Copy-Item "$productDir\*.dll" $packageLibraryDir

	#Copy-Item "$productX64Dir\$product.exe" "$packageDir\${product}64.exe"
	#Copy-Item "$productX64Dir\$product.exe.config" "$packageDir\${product}64.exe.config"


	# copy language dll
	$langs = "de","en","es","fr","it","ja","ko","ru","zh-Hans","zh-Hant","x64","x86"
	foreach($lang in $langs)
	{
		Copy-Item "$productDir\$lang" $packageLibraryDir -Recurse
	}

	#------------------------
	# generate README.html

	New-Readme $packageDir "ReadmeTemplate.md"
}

#----------------------
# generate README.html
function New-Readme($packageDir, $template)
{
	$readmeDir = $packageDir + "\readme"
	$temp = New-Item $readmeDir -ItemType Directory 

	Copy-Item $template "$readmeDir/README.md"
	Copy-Item "$solutionDir\LICENSE.md" $readmeDir
	Copy-Item "$solutionDir\THIRDPARTY_LICENSES.md" $readmeDir
	Copy-Item "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	# edit README.md
	Replace-Content "$readmeDir\README.md" "<VERSION/>" "$version"

	# markdown to html by pandoc
	pandoc -s -t html5 -o "$packageDir\README.html" -H Style.html "$readmeDir\README.md" "$readmeDir\LICENSE.md" "$readmeDir\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	Remove-Item $readmeDir -Recurse
}

#--------------------------
# archive to ZIP
function New-Zip
{
	Copy-Item $packageX64Dir $packageDir -Recurse
	Copy-Item "$packageX86Dir\$product.exe" "$packageDir\${product}S.exe"
	Copy-Item "$packageX86Dir\$product.exe.config" "$packageDir\${product}S.exe.config"

	Compress-Archive $packageDir -DestinationPath $packageZip
}


#--------------------------
#
function New-ConfigForMsi($inputDir, $config, $outputDir)
{
	# make config for installer
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.msi'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'True'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config
	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}


#--------------------------
#
function New-ConfigForAppx($inputDir, $config, $outputDir)
{
	# make config for installer
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.appx'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'True'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}



#---------------------------
#
function New-EmptyFolder($dir)
{
	# remove folder
	if (Test-Path $dir)
	{
		Remove-Item $dir -Recurse
		Start-Sleep -m 100
	}

	# make folder
	$temp = New-Item $dir -ItemType Directory
}

#---------------------------
#
function New-PackageAppend($packageDir)
{
	#$config = "$product.exe.config"
	$packageAppendDir = $packageDir + ".append"
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.exe.config" $packageAppendDir
	New-ConfigForMsi $packageDir "${product}S.exe.config" $packageAppendDir
}


#--------------------------
# WiX
function New-Msi($arch, $packageDir, $packageMsi)
{
	$candle = $env:WIX + 'bin\candle.exe'
	$light = $env:WIX + 'bin\light.exe'
	$heat = $env:WIX +  'bin\heat.exe'

	# wix object folder
	$objDir = $packageDir + ".append\" + $arch
	New-EmptyFolder $objDir


	# make DllComponents.wxs
	#& $heat dir "$packageDir\Libraries" -cg DllComponents -ag -pog:Binaries -sfrag -var var.LibrariesDir -dr INSTALLFOLDER -out WixSource\DllComponents.wxs
	#if ($? -ne $true)
	#{
	#	throw "heat error"
	#}

	#-------------------------
	# WiX
	#-------------------------

	$ErrorActionPreference = "stop"

	& $candle -d"Platform=$arch" -d"BuildVersion=$buildVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -ext WixNetFxExtension -out "$objDir\\"  WixSource\*.wxs
	if ($? -ne $true)
	{
		throw "candle error"
	}

	& $light -out "$packageMsi" -ext WixUIExtension -ext WixNetFxExtension -cultures:ja-JP "$objDir\*.wixobj"
	if ($? -ne $true)
	{
		throw "light error" 
	}
}


#--------------------------
# Appx ready
function New-AppxReady
{
	# update assembly
	Copy-Item $packageX64Dir $packageAppxProduct -Recurse -Force
	New-ConfigForAppx $packageX64Dir "${product}.exe.config" $packageAppxProduct

	# generate README.html
	New-Readme $packageAppxProduct "ReadmeAppxTemplate.md"

	# copy icons
	Copy-Item "Appx\Resources\Assets\*.png" "$packageAppxFiles\Assets\" 
}

#--------------------------
# Appx
function New-Appx($arch, $appx)
{
	. Appx/_Parameter.ps1
	$param = Get-AppxParameter
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\Resources\AppxManifest.xml"
	$content = $content -replace "%NAME%","$appxName"
	$content = $content -replace "%PUBLISHER%","$appxPublisher"
	$content = $content -replace "%VERSION%","$assemblyVersion"
	$content = $content -replace "%ARCH%", "$arch"
	$content | Out-File -Encoding UTF8 "$packageAppxFiles\AppxManifest.xml"


	## re-package
	$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x86"
	& "$Win10SDK\makeappx.exe" pack /l /d "$packageAppxFiles" /p "$appx"
	if ($? -ne $true)
	{
		throw "makeappx.exe error"
	}
}


#--------------------------
# remove build objects
function Remove-BuildObjects
{
	if (Test-Path $packageDir)
	{
		Remove-Item $packageDir -Recurse -Force
	}
	if (Test-Path $packageAppendDir)
	{
		Remove-Item $packageAppendDir -Recurse -Force
	}
	if (Test-Path $packageX86Dir)
	{
		Remove-Item $packageX86Dir -Recurse -Force
	}
	if (Test-Path $packageX64Dir)
	{
		Remove-Item $packageX64Dir -Recurse -Force
	}
	if (Test-Path $packageZip)
	{
		Remove-Item $packageZip
	}
	if (Test-Path $packageX86Msi)
	{
		Remove-Item $packageX86Msi
	}
	if (Test-Path $packageX64Msi)
	{
		Remove-Item $packageX64Msi
	}
	if (Test-Path $packageAppxProduct)
	{
		Remove-Item $packageAppxProduct -Recurse -Force
	}
	if (Test-Path $packageX86Appx)
	{
		Remove-Item $packageX86Appx
	}
	if (Test-Path $packageX64Appx)
	{
		Remove-Item $packageX64Appx
	}

	Start-Sleep -m 100
}



#======================
# main
#======================

# versions
$version = Get-Version
$buildCount = Get-BuildCount
$buildVersion = "$version.$buildCount"
$assemblyVersion = "$version.$buildCount.0"

$packageDir = "$product$version"
$packageAppendDir = $packageDir + ".append"
$packageX86Dir = "$product${version}-x86"
$packageX64Dir = "$product${version}-x64"
$packageZip = "$product$version.zip"
#$packageMsi = "$product$version.msi"
$packageX86Msi = "${product}S${version}.msi"
$packageX64Msi = "${product}${version}.msi"
$packageAppxRoot = "Appx\$product"
$packageAppxFiles = "$packageAppxRoot\PackageFiles"
$packageAppxProduct = "$packageAppxRoot\PackageFiles\$product"
$packageX86Appx = "${product}${version}-x86.appx"
$packageX64Appx = "${product}${version}-x64.appx"

# clear
Write-Host "`n[Clear] ...`n" -fore Cyan
Remove-BuildObjects
	
# build
Write-Host "`n[Build] ...`n" -fore Cyan
Build-Project "x86" $assemblyVersion
Build-Project "x64" $assemblyVersion
Reset-AssemblyInfo $assemblyInfoFile

#
Write-Host "`n[Package] ...`n" -fore Cyan
New-Package $productX86Dir $packageX86Dir
New-Package $productX64Dir $packageX64Dir

#
if (($Target -eq "All") -or ($Target -eq "Zip"))
{
	Write-Host "`[Zip] ...`n" -fore Cyan
	New-Zip
	Write-Host "`nExport $packageZip successed.`n" -fore Green
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	Write-Host "`[Installer] ...`n" -fore Cyan

	New-PackageAppend $packageDir
	New-Msi "x64" $packageDir $packageX64Msi
	Write-Host "`nExport $packageX64Msi successed.`n" -fore Green
	#New-Msi "x86" $packageDir $packageX86Msi
	#Write-Host "`nExport $packageX86Msi successed.`n" -fore Green
}


if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	Write-Host "`[Appx] ...`n" -fore Cyan

	if (Test-Path $packageAppxRoot)
	{
		New-AppxReady
		New-Appx "x64" $packageX64Appx
		Write-Host "`nExport $packageX64Appx successed.`n" -fore Green
		New-Appx "x86" $packageX86Appx
		Write-Host "`nExport $packageX86Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist $packageAppxRoot. skip!`n" -fore Yellow
	}
}

# current
Write-Host "`[Current] ...`n" -fore Cyan
if (-not (Test-Path $product))
{
	New-Item $product -ItemType Directory
}
Copy-Item "$packageDir\*" "$product\" -Recurse -Force

#--------------------------
# saev buid version
Set-BuildCount $buildCount

#-------------------------
# Finish.
Write-Host "`nBuild $buildVersion All done.`n" -fore Green





