Param(
	[ValidateSet("All", "Zip", "Installer")]$Target = "All"
)

# error to break
trap { break }

$product = 'NeeView'

$config = 'Release'


#---------------------
# get fileversion
function Get-FileVersion($fileName)
{
	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart
	$build = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileBuildPart
	if ($build -eq 0)
	{
		"$major.$minor"
	}
	else
	{
		"$major.$minor.$build"
	}	
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




#-----------------------
# variables
$solutionDir = "E:\Documents\Visual Studio 2015\Projects\$product"
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$productDir = "$projectDir\bin\$config"

#----------------------
# build
$msbuild = 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
& $msbuild $solution /p:Configuration=$config /t:Clean,Build
if ($? -ne $true)
{
	throw "build error"
}

# get assembly version
$version = Get-FileVersion "$productDir\$product.exe"

$packageDir = $product + $version
$packageZip = $packageDir + ".zip"
$packageMsi = $packageDir + ".msi"

$packageLibraryDir = $packageDir + "\Libraries"

# remove packageDir
if (Test-Path $packageDir)
{
	Remove-Item $packageDir -Recurse -Force
}
if (Test-Path $packageZip)
{
	Remove-Item $packageZip
}
if (Test-Path $packageMsi)
{
	Remove-Item $packageMsi
}


Start-Sleep -m 100


#----------------------
# package section
function New-Package
{
	# make package folder
	$temp = New-Item $packageDir -ItemType Directory
	$temp = New-Item $packageLibraryDir -ItemType Directory

	# copy
	Copy-Item "$productDir\$product.exe" $packageDir
	Copy-Item "$productDir\$product.exe.config" $packageDir
	Copy-Item "$productDir\*.dll" $packageLibraryDir

	# copy language dll
	$langs = "de","en","es","fr","it","ja","ko","ru","zh-Hans","zh-Hant"
	foreach($lang in $langs)
	{
		Copy-Item "$productDir\$lang" $packageLibraryDir -Recurse
	}

	#------------------------
	# generate README.html

	$readmeDir = $packageDir + "\readme"
	$temp = New-Item $readmeDir -ItemType Directory 

	Copy-Item "$solutionDir\*.md" $readmeDir
	#Copy-Item "$solutionDir\Style.html" $readmeDir

	# edit README.md
	Replace-Content "$readmeDir\README.md" "# $product" "# $product $version"

	# markdown to html by pandoc
	pandoc -s -t html5 -o "$packageDir\README.html" -H Style.html "$readmeDir\README.md" "$readmeDir\LICENSE.md" "$readmeDir\THIRDPARTY_LICENSES.md"

	Remove-Item $readmeDir -Recurse

	return $temp
}

#--------------------------
# archive to ZIP
function New-Zip
{
	Compress-Archive $packageDir -DestinationPath $packageZip
}


#--------------------------
# WiX
function New-Msi
{
	$candle = 'C:\Program Files (x86)\WiX Toolset v3.10\bin\candle.exe'
	$light = 'C:\Program Files (x86)\WiX Toolset v3.10\bin\light.exe'
	$heat = 'C:\Program Files (x86)\WiX Toolset v3.10\bin\heat.exe'


	$config = "$product.exe.config"
	$packageAppendDir = $packageDir + ".append"

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

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$sw = New-Object System.IO.StreamWriter("$packageAppendDir\$config", $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()

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

	& $candle -d"BuildVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries"  -out "$packageDir.append\\"  WixSource\*.wxs
	if ($? -ne $true)
	{
		throw "candle error"
	}

	& $light -out "$packageMsi" -ext WixUIExtension -ext WixNetFxExtension -cultures:ja-JP "$packageDir.append\*.wixobj"
	if ($? -ne $true)
	{
		throw "light error" 
	}
}

#--------------------------
# main

Write-Host "`n[Package] ...`n" -fore Cyan
New-Package

if (($Target -eq "All") -or ($Target -eq "Zip"))
{
	Write-Host "`[Zip] ...`n" -fore Cyan
	New-Zip
	Write-Host "`nExport $packageZip successed.`n" -fore Green
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	Write-Host "`[Installer] ...`n" -fore Cyan
	New-Msi
	Write-Host "`nExport $packageMsi successed.`n" -fore Green
}


# current
Copy-Item "$packageDir\*" "NeeView\" -Recurse -Force


#-------------------------
# Finish.
Write-Host "`nAll done.`n" -fore Green





