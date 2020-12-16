# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx", "Canary", "Beta")]$Target = "All",
	[switch]$continue
)

# error to break
trap { break }

$ErrorActionPreference = "stop"

# MSI作成時にDllComponents.wsxを更新する?
$isCreateDllComponentsWxs = $false;

# AnyCPU版ZIPを作成する？
$isAnyCPU = $false;

#
$product = 'NeeView'
$configuration = 'Release'
$framework = 'net48'

#
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64"


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
function Get-Version($projectFile)
{
	$xml = [xml](Get-Content $projectFile)
	$version = [String]$xml.Project.PropertyGroup.Version;
	if ($version -match '(\d+\.\d+)\.\d+')
	{
		return $Matches[1]
	}
	
    throw "Cannot get Version."
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

#---------------------
# get git log
function Get-GitLog()
{
    $branch = Invoke-Expression "git rev-parse --abbrev-ref HEAD"
    $descrive = Invoke-Expression "git describe --abbrev=0 --tags"
	$date = Invoke-Expression 'git log -1 --pretty=format:"%ad" --date=iso'
	$result = Invoke-Expression "git log $descrive..head --encoding=Shift_JIS --pretty=format:`"%ae %s`""
	$result = $result | Where-Object {$_ -match "^nee.laboratory"} | ForEach-Object {$_ -replace "^[\w\.@]+ ",""}
	$result = $result | Where-Object { -not ($_ -match '^m.rge|^開発用|^作業中|\(dev\)|^-|^\.\.') } 

    return "[${branch}] $descrive to head", $date, $result
}

#---------------------
# get git log (markdown)
function Get-GitLogMarkdown($title)
{
    $result = Get-GitLog
	$header = $result[0]
	$date = $result[1]
    $logs = $result[2]

	"## $title"
	"### $header"
	"Rev. $revision / $date"
	""
	$logs | ForEach-Object { "- $_" }
	""
	"This list of changes was auto generated."
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
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Convert-Path "$scriptPath\.."
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$project = "$projectDir\$product.csproj"
$projectSusieDir = "$solutionDir\NeeView.Susie.Server"


#-----------------------
# procject output dir
function Get-ProjectOutputDir($projectDir, $platform)
{
	if ($platform -eq "AnyCPU")
	{
		"$projectDir\bin\$configuration\$framework"
	}
	else
	{
		"$projectDir\bin\$platform\$configuration\$framework"
	}
}

#----------------------
# build
function Build-Project($platform)
{
	$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
	$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1

	$output = Get-ProjectOutputDir $projectDir $platform
	if (Test-Path $output)
	{
		Remove-Item $output -Recurse
	}

	$output = Get-ProjectOutputDir $projectSusieDir $platform
	if (Test-Path $output)
	{
		Remove-Item $output -Recurse
	}

	$msPlatform = $platform
	if ($platform -eq "AnyCPU")
	{
		$msPlatform = "Any CPU"
	}

	"$msbuild -restore $solution /p:Configuration=$configuration /p:Platform=""$msPlatform"" /t:Clean,Build"

	& $msbuild -restore $solution /p:Configuration=$configuration /p:Platform="$msPlatform" /t:Clean,Build
	if ($? -ne $true)
	{
		throw "build error"
	}
}

#----------------------
# publish
function Export-Publish($platform, $projectDir, $export)
{
	$output = Get-ProjectOutputDir $projectDir $platform

	Copy-Item $output $export -Recurse
}



#----------------------
# package section
function New-Package($platform, $productName, $productDir, $publishSusieDir, $packageDir)
{
	$packageLibraryDir = $packageDir + "\Libraries"
	$packageSusieDir = $packageLibraryDir + "\Susie"

	# make package folder
	$temp = New-Item $packageDir -ItemType Directory
	$temp = New-Item $packageLibraryDir -ItemType Directory
	$temp = New-Item $packageSusieDir -ItemType Directory

	# copy
	Copy-Item "$productDir\$productName.exe" $packageDir
	Copy-Item "$productDir\*.dll" $packageLibraryDir
	Copy-Item "$productDir\Scripts\" $packageDir -Recurse

	# custom config
	New-ConfigForZip $productDir "$productName.exe.config" $packageDir

	# copy NeeView.Susie.Server
	Copy-Item "$publishSusieDir\NeeView.Susie.Server.exe" $packageSusieDir
	Copy-Item "$publishSusieDir\NeeView.Susie.Server.exe.config" $packageSusieDir
	Copy-Item "$publishSusieDir\*.dll" $packageSusieDir

	# copy language dll
	$langs = "ja-JP"
	foreach($lang in $langs)
	{
		Copy-Item "$productDir\$lang" $packageLibraryDir -Recurse
	}

	# copy platform dll
	if ($platform -eq "AnyCPU")
	{
		Copy-Item "$productDir\x86" $packageLibraryDir -Recurse
		Copy-Item "$productDir\x64" $packageLibraryDir -Recurse
	}
	else
	{
		Copy-Item "$productDir\$platform" $packageLibraryDir -Recurse
	}

	# generate README.html
	New-Readme $packageDir "en-us" ".zip"
	New-Readme $packageDir "ja-jp" ".zip"
}

#----------------------
# generate README.html
function New-Readme($packageDir, $culture, $target)
{
	$readmeSource = "Readme\$culture"

	$readmeDir = $packageDir + "\readme.$culture"
	

	$temp = New-Item $readmeDir -ItemType Directory 

	Copy-Item "$readmeSource\Overview.md" $readmeDir
	Copy-Item "$readmeSource\Canary.md" $readmeDir
	Copy-Item "$readmeSource\Emvironment.md" $readmeDir
	Copy-Item "$readmeSource\Contact.md" $readmeDir

	Copy-Item "$solutionDir\LICENSE.md" $readmeDir
	Copy-Item "$solutionDir\LICENSE.ja-jp.md" $readmeDir
	Copy-Item "$solutionDir\THIRDPARTY_LICENSES.md" $readmeDir
	Copy-Item "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	if ($target -eq ".canary")
	{
		Get-GitLogMarkdown "NeeView <VERSION/> - ChangeLog" | Set-Content -Encoding UTF8 "$readmeDir\ChangeLog.md"
	}
	else
	{
		Copy-Item "$readmeSource\ChangeLog.md" $readmeDir
	}

	$postfix = $version
	$announce = ""
	if ($target -eq ".canary")
	{
		$postfix = "Canary ${dateVersion}"
		$announce = "Rev. ${revision}`r`n`r`n" + (Get-Content -Path "$readmeDir/Canary.md" -Raw -Encoding UTF8)
	}

	# edit README.md
	Replace-Content "$readmeDir\Overview.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Overview.md" "<ANNOUNCE/>" "$announce"
	Replace-Content "$readmeDir\Emvironment.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Contact.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\ChangeLog.md" "<VERSION/>" "$postfix"

	$readmeHtml = "README.html"
	$readmeEnvironment = ""
	$readmeLicenseAppendix = ""

	if (-not ($culture -eq "en-us"))
	{
		$readmeHtml = "README.$culture.html"
	}

	if ($culture -eq "ja-jp")
	{
		$readmeLicenseAppendix = """$readmeDir\LICENSE.ja-jp.md"""
	}

	if ($target -ne ".appx")
	{
		$readmeEnvironment = """$readmeDir\Emvironment.md"""
	}

	# markdown to html by pandoc
	pandoc -s -t html5 -o "$packageDir\$readmeHtml" -H "Readme\Style.html" "$readmeDir\Overview.md" $readmeEnvironment "$readmeDir\Contact.md" "$readmeDir\LICENSE.md" $readmeLicenseAppendix "$readmeDir\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md" "$readmeDir\ChangeLog.md"

	Remove-Item $readmeDir -Recurse
}


#--------------------------
# archive to ZIP
function New-Zip($packageDir, $packageZip)
{
	Compress-Archive $packageDir -DestinationPath $packageZip
}

#--------------------------
#
function New-ConfigForZip($inputDir, $config, $outputDir)
{
	# make config for zip
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.zip'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'False'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
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

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

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
	# make config for appx
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.appx'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'True'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}

#--------------------------
#
function New-ConfigForDevPackage($inputDir, $config, $target, $outputDir)
{
	# make config for canary
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = $target

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'False'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

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
function New-PackageAppend($packageDir, $packageAppendDir)
{
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.exe.config" $packageAppendDir

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}



#--------------------------
# WiX
function New-Msi($arch, $packageDir, $packageAppendDir, $packageMsi)
{
	$candle = $env:WIX + 'bin\candle.exe'
	$light = $env:WIX + 'bin\light.exe'
	$heat = $env:WIX + 'bin\heat.exe'
	$torch = $env:WIX + 'bin\torch.exe'
	$wisubstg = "$Win10SDK\wisubstg.vbs"
	$wilangid = "$Win10SDK\wilangid.vbs"

	$1041Msi = "$packageAppendDir\1041.msi"
	$1041Mst = "$packageAppendDir\1041.mst"

	#-------------------------
	# WiX
	#-------------------------

	$ErrorActionPreference = "stop"

	function New-DllComponents
	{
		& $heat dir "$packageDir\Libraries" -cg DllComponents -ag -pog:Binaries -sfrag -sreg -var var.LibrariesDir -dr INSTALLFOLDER -out WixSource\$arch\DllComponents.wxs
		if ($? -ne $true)
		{
			throw "heat error"
		}
	}

	function New-MsiSub($packageMsi, $culture)
	{
		Write-Host "$packageMsi : $culture" -fore Cyan
		
		$wixObjDir = "$packageAppendDir\obj.$culture"
		New-EmptyFolder $wixObjDir

		& $candle -arch $arch -d"Platform=$arch" -d"BuildVersion=$buildVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -d"culture=$culture" -ext WixNetFxExtension -out "$wixObjDir\\"  WixSource\*.wxs .\WixSource\$arch\*.wxs
		if ($? -ne $true)
		{
			throw "candle error"
		}

		& $light -out "$packageMsi" -ext WixUIExtension -ext WixNetFxExtension -cultures:$culture -loc WixSource\Language-$culture.wxl  "$wixObjDir\*.wixobj"
		if ($? -ne $true)
		{
			throw "light error" 
		}
	}

	## Create DllComponents.wxs
	if ($isCreateDllComponentsWxs)
	{
		New-DllComponents
	}

	New-MsiSub $packageMsi "en-us"
	New-MsiSub $1041Msi "ja-jp"

	& $torch -p -t language $packageMsi $1041Msi -out $1041Mst
	if ($? -ne $true)
	{
		throw "torch error"
	}

	#-------------------------
	# WinSDK
	#-------------------------

	& cscript "$wisubstg" "$packageMsi" $1041Mst 1041
	if ($? -ne $true)
	{
		throw "wisubstg.vbs error"
	}

	& cscript "$wilangid" "$packageMsi" Package 1033,1041
	if ($? -ne $true)
	{
		throw "wilangid.vbs error"
	}
}



#--------------------------
# Appx 
function New-Appx($arch, $packageDir, $packageAppendDir, $appx)
{


	$packgaeFilesDir = "$packageAppendDir/PackageFiles"
	$contentDir = "$packgaeFilesDir/NeeView"

	# copy package base files
	Copy-Item "Appx\Resources" $packgaeFilesDir -Recurse -Force

	# update assembly
	Copy-Item $packageDir $contentDir -Recurse -Force
	New-ConfigForAppx $packageDir "${product}.exe.config" $contentDir

	# generate README.html
	New-Readme $contentDir "en-us" ".appx"
	New-Readme $contentDir "ja-jp" ".appx"


	. $env:CersPath/_Parameter.ps1
	$param = Get-AppxParameter
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\Resources\AppxManifest.xml"
	$content = $content -replace "%NAME%","$appxName"
	$content = $content -replace "%PUBLISHER%","$appxPublisher"
	$content = $content -replace "%VERSION%","$assemblyVersion"
	$content = $content -replace "%ARCH%", "$arch"
	$content | Out-File -Encoding UTF8 "$packgaeFilesDir\AppxManifest.xml"


	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packgaeFilesDir" /p "$appx"
	if ($? -ne $true)
	{
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "$env:CersPath/_neeview.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true)
	{
		throw "signtool.exe error"
	}
}


#--------------------------
# archive to Canary.ZIP
function New-Canary($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir $packageCanary ".canary"
}

function New-CanaryAnyCPU($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir_AnyCPU $packageCanary_AnyCPU ".canary"
}

#--------------------------
# archive to Beta.ZIP
function New-Beta($packageDir)
{
	New-DevPackage $packageDir $packageBetaDir $packageBeta ".beta"
}

#--------------------------
# archive to Canary/Beta.ZIP
function New-DevPackage($packageDir, $devPackageDir, $devPackage, $target)
{
	# update assembly
	Copy-Item $packageDir $devPackageDir -Recurse
	New-ConfigForDevPackage $packageDir "${product}.exe.config" $target $devPackageDir

	# generate README.html
	New-Readme $devPackageDir "en-us" $target
	New-Readme $devPackageDir "ja-jp" $target

	Compress-Archive $devPackageDir -DestinationPath $devPackage
}



#--------------------------
# remove build objects
function Remove-BuildObjects
{
	Get-ChildItem -Directory "$packagePrefix*" | Remove-Item -Recurse

	Get-ChildItem -File "$packagePrefix*.*" | Remove-Item

	if (Test-Path $publishDir)
	{
		Remove-Item $publishDir -Recurse
	}
	if (Test-Path $packageCanaryDir)
	{
		Remove-Item $packageCanaryDir -Recurse -Force
	}
	if (Test-Path $packageBetaDir)
	{
		Remove-Item $packageBetaDir -Recurse -Force
	}
	if (Test-Path $packageCanaryWild)
	{
		Remove-Item $packageCanaryWild
	}
	if (Test-Path $packageBetaWild)
	{
		Remove-Item $packageBetaWild
	}

	Start-Sleep -m 100
}



function Build-PackageSorce
{
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects
	
	# build
	Write-Host "`n[Build] ...`n" -fore Cyan


	Build-Project "x64"
	Export-Publish "x64" $projectDir $publishDir_x64
	
	Build-Project "x86"
	Export-Publish "x86" $projectDir $publishDir_x86
	Export-Publish "x86" $projectSusieDir $publishSusieDir

	if ($isAnyCPU)
	{
		Build-Project "AnyCPU"
		Export-Publish "AnyCPU" $projectDir $publishDir_AnyCPU
		Export-Publish "AnyCPU" $projectSusieDir $publishSusieDir_AnyCPU
	}
	
	# create package source
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64 $publishSusieDir $packageDir_x64
	New-Package "x86" $product $publishDir_x86 $publishSusieDir $packageDir_x86

	if ($isAnyCPU)
	{
		New-Package "AnyCPU" $product $publishDir_AnyCPU $publishSusieDir_AnyCPU $packageDir_AnyCPU
	}
}


function Build-Zip
{
	Write-Host "`[Zip] ...`n" -fore Cyan

	New-Zip $packageDir_x64 $packageZip_x64
	Write-Host "`nExport $packageZip_x64 successed.`n" -fore Green

	New-Zip $packageDir_x86 $packageZip_x86
	Write-Host "`nExport $packageZip_x86 successed.`n" -fore Green
	
	if ($isAnyCPU)
	{
		New-Zip $packageDir_AnyCPU $packageZip_AnyCPU
		Write-Host "`nExport $packageZip_AnyCPU successed.`n" -fore Green
	}
}


function Build-Installer
{
	Write-Host "`n[Installer] ...`n" -fore Cyan
	
	New-PackageAppend $packageDir_x64 $packageAppendDir_x64
	New-Msi "x64" $packageDir_x64 $packageAppendDir_x64 $packageMsi_x64
	Write-Host "`nExport $packageMsi_x64 successed.`n" -fore Green

	New-PackageAppend $packageDir_x86 $packageAppendDir_x86
	New-Msi "x86" $packageDir_x86 $packageAppendDir_x86 $packageMsi_x86
	Write-Host "`nExport $packageMsi_x86 successed.`n" -fore Green
}


function Build-Appx
{
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1")
	{
		New-Appx "x64" $packageDir_x64 $packageAppxDir_x64 $packageX64Appx
		Write-Host "`nExport $packageX64Appx successed.`n" -fore Green

		New-Appx "x86" $packageDir_x86 $packageAppxDir_x86 $packageX86Appx
		Write-Host "`nExport $packageX86Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist make appx envionment. skip!`n" -fore Yellow
	}
}

function Build-Canary
{
	Write-Host "`n[Canary] ...`n" -fore Cyan
	New-Canary $packageDir_x64
	Write-Host "`nExport $packageCanary successed.`n" -fore Green

	if ($isAnyCPU)
	{
		New-CanaryAnyCPU $packageDir_AnyCPU
		Write-Host "`nExport $packageCanary successed.`n" -fore Green
	}
}

function Build-Beta
{
	Write-Host "`n[Beta] ...`n" -fore Cyan
	New-Beta $packageDir_x64
	Write-Host "`nExport $packageBeta successed.`n" -fore Green
}


function Export-Current
{
	Write-Host "`n[Current] ...`n" -fore Cyan
	if (Test-Path $packageDir_x64)
	{
		if (-not (Test-Path $product))
		{
			New-Item $product -ItemType Directory
		}
		Copy-Item "$packageDir_x64\*" "$product\" -Recurse -Force
	}
	else
	{
		Write-Host "`nWarning: not exist $packageDir_x64. skip!`n" -fore Yellow
	}
}


#======================
# main
#======================

# versions
$version = Get-Version $project
$buildCount = Get-BuildCount
$buildVersion = "$version.$buildCount"
$assemblyVersion = "$version.$buildCount.0"
$revision = (& git rev-parse --short HEAD).ToString()
$dateVersion = (Get-Date).ToString("MMdd")

$publishDir = "Publish"
$publishDir_AnyCPU = "$publishDir\NeeView-AnyCPU"
$publishDir_x64 = "$publishDir\NeeView-x64"
$publishDir_x86 = "$publishDir\NeeView-x86"
$publishSusieDir = "$publishDir\NeeView.Susie.Server"
$publishSusieDir_AnyCPU = "$publishDir\NeeView.Susie.Server-AnyCPU"
$packagePrefix = "$product$version"
$packageDir_AnyCPU = "$product$version-AnyCPU"
$packageDir_x64 = "$product$version-x64"
$packageDir_x86 = "$product$version-x86"
$packageAppendDir_x64 = "$packageDir_x64.append"
$packageAppendDir_x86 = "$packageDir_x86.append"
$packageZip_AnyCPU = "${product}${version}-AnyCPU.zip"
$packageZip_x64 = "${product}${version}-x64.zip"
$packageZip_x86 = "${product}${version}-x86.zip"
$packageMsi_x64 = "${product}${version}-x64.msi"
$packageMsi_x86 = "${product}${version}-x86.msi"
$packageAppxDir_x64 = "${product}${version}-appx-x64"
$packageAppxDir_x86 = "${product}${version}-appx-x84"
$packageX86Appx = "${product}${version}-x86.appx"
$packageX64Appx = "${product}${version}-x64.appx"
$packageCanaryDir = "${product}Canary"
$packageCanaryDir_AnyCPU = "${product}Canary-AnyCPU"
$packageCanary = "${product}Canary${dateVersion}.zip"
$packageCanary_AnyCPU = "${product}Canary${dateVersion}_AnyCPU.zip"
$packageCanaryWild = "${product}Canary*.zip"
$packageBetaDir = "${product}Beta"
$packageBeta = "${product}Beta${dateVersion}.zip"
$packageBetaWild = "${product}Beta*.zip"


if (-not $continue)
{
	Build-PackageSorce
}

if (($Target -eq "All") -or ($Target -eq "Zip") -or ($Target -eq "Canary") -or ($Target -eq "Beta"))
{
	Build-Zip
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	Build-Installer
}

if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	Build-Appx
}

if (($Target -eq "All") -or ($Target -eq "Canary"))
{
	Build-Canary
}

if (($Target -eq "All") -or ($Target -eq "Beta"))
{
	Build-Beta
}

Export-Current

#--------------------------
# saev buid version
Set-BuildCount $buildCount

#-------------------------
# Finish.
Write-Host "`nBuild $buildVersion All done.`n" -fore Green





