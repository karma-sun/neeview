# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx")]$Target = "All",
	[switch]$continue
)

# error to break
trap { break }

$ErrorActionPreference = "stop"


#
$product = 'NeeView'
$config = 'Release'

#
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x64"


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
function Set-AssemblyVersion($projectFile, $assemblyInfoFile, $title, $version)
{
	$content = Get-Content $projectFile
	$content = $content -replace "<AssemblyName>.+</AssemblyName>", "<AssemblyName>$title</AssemblyName>"
	$content | Out-File -Encoding UTF8 $projectFile

    $content = Get-Content $assemblyInfoFile
    $content = $content -replace "AssemblyTitle\(.+\)", "AssemblyTitle(`"$title`")"
    $content = $content -replace "AssemblyProduct\(.+\)", "AssemblyProduct(`"$title`")"
    $content = $content -replace "AssemblyVersion\(.+\)", "AssemblyVersion(`"$version`")"
    $content = $content -replace "AssemblyFileVersion\(.+\)", "AssemblyFileVersion(`"$version`")"
	$content | Out-File -Encoding UTF8 $assemblyInfoFile
}


#
$tempProjectFile = [System.IO.Path]::GetTempFileName()
$tempAssemblyInfoFile = [System.IO.Path]::GetTempFileName()

#--------------------
# store AssemblyInfo.cs
function Save-AssemblyInfo($projectFile, $assemblyInfoFile)
{
	Write-Host "Store: Copy-Item $projectFile $tempProjectFile"
	Copy-Item $projectFile $tempProjectFile

	Write-Host "Store: Copy-Item $assemblyInfoFile $tempAssemblyInfoFile"
	Copy-Item $assemblyInfoFile $tempAssemblyInfoFile
}

#--------------------
# reset AssemblyInfo.cs
function Restore-AssemblyInfo($projectFile, $assemblyInfoFile)
{
	Write-Host "Restore: Move-Item $tempProjectFile $projectFile -Force"
	Move-Item $tempProjectFile $projectFile -Force

	Write-Host "Restore: Move-Item $tempAssemblyInfoFile $assemblyInfoFile -Force"
	Move-Item $tempAssemblyInfoFile $assemblyInfoFile -Force
}





#-----------------------
# variables
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Convert-Path "$scriptPath\.."
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$projectFile = "$projectDir\$product.csproj"
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
		Set-AssemblyVersion $projectFile $assemblyInfoFile "${product}S" $assemblyVersion
	}
	else
	{
		$platform = "Any CPU"
		Set-AssemblyVersion $projectFile $assemblyInfoFile "${product}" $assemblyVersion
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
	Copy-Item "$productDir\*.exe" $packageDir
	Copy-Item "$productDir\*.exe.config" $packageDir
	Copy-Item "$productDir\*.dll" $packageLibraryDir

	#Copy-Item "$productX64Dir\$product.exe" "$packageDir\${product}64.exe"
	#Copy-Item "$productX64Dir\$product.exe.config" "$packageDir\${product}64.exe.config"


	# copy language dll
	$langs = "ja-JP","x64","x86"
	foreach($lang in $langs)
	{
		Copy-Item "$productDir\$lang" $packageLibraryDir -Recurse
	}

	#------------------------
	# generate README.html

	New-Readme $packageDir "en-us" $FALSE
	New-Readme $packageDir "ja-jp" $FALSE
}

#----------------------
# generate README.html
function New-Readme($packageDir, $culture, $isAppx)
{
	$readmeSource = "Readme\$culture"

	$readmeDir = $packageDir + "\readme.$culture"
	

	$temp = New-Item $readmeDir -ItemType Directory 

	Copy-Item "$readmeSource\Overview.md" $readmeDir
	Copy-Item "$readmeSource\Susie.md" $readmeDir
	Copy-Item "$readmeSource\Emvironment.md" $readmeDir
	Copy-Item "$readmeSource\Contact.md" $readmeDir

	Copy-Item "$solutionDir\LICENSE.md" $readmeDir
	Copy-Item "$solutionDir\LICENSE.ja-jp.md" $readmeDir
	Copy-Item "$solutionDir\THIRDPARTY_LICENSES.md" $readmeDir
	Copy-Item "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	$susie = ""
	if (-not $isAppx)
	{
		$susie = Get-Content -Path "$readmeDir/Susie.md" -Raw -Encoding UTF8
	}

	# edit README.md
	Replace-Content "$readmeDir\Overview.md" "<VERSION/>" "$version"
	Replace-Content "$readmeDir\Overview.md" "<SUSIE/>" "$susie"
	Replace-Content "$readmeDir\Emvironment.md" "<VERSION/>" "$version"
	Replace-Content "$readmeDir\Contact.md" "<VERSION/>" "$version"

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

	if (-not $isAppx)
	{
		$readmeEnvironment = """$readmeDir\Emvironment.md"""
	}


	# markdown to html by pandoc
	pandoc -s -t html5 -o "$packageDir\$readmeHtml" -H "Readme\Style.html" "$readmeDir\Overview.md" $readmeEnvironment "$readmeDir\Contact.md" "$readmeDir\LICENSE.md" $readmeLicenseAppendix "$readmeDir\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	Remove-Item $readmeDir -Recurse
}

#--------------------------
# archive to ZIP
function New-Zip
{
	Copy-Item $packageX64Dir $packageDir -Recurse

	Copy-Item "$packageX86Dir\*.exe" $packageDir
	Copy-Item "$packageX86Dir\*.exe.config" $packageDir
	Copy-Item "$packageX86Dir\Libraries\ja-JP\NeeViewS.resources.dll" "$packageDir\Libraries\ja-JP"

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

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}



#--------------------------
# WiX
function New-Msi($packageDir, $packageMsi)
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
		& $heat dir "$packageDir\Libraries" -cg DllComponents -ag -pog:Binaries -sfrag -var var.LibrariesDir -dr INSTALLFOLDER -out WixSource\DllComponents.wxs
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

		& $candle -d"BuildVersion=$buildVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -d"culture=$culture" -ext WixNetFxExtension -out "$wixObjDir\\"  WixSource\*.wxs
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

	### Create DllComponents.wxs
	#New-DllComponents

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
# Appx ready
function New-AppxReady
{
	# update assembly
	Copy-Item $packageX64Dir $packageAppxProduct -Recurse -Force
	New-ConfigForAppx $packageX64Dir "${product}.exe.config" $packageAppxProduct

	# generate README.html
	New-Readme $packageAppxProduct "en-us" $TRUE
	New-Readme $packageAppxProduct "ja-jp" $TRUE

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


	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packageAppxFiles" /p "$appx"
	if ($? -ne $true)
	{
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "Appx/_neeview.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true)
	{
		throw "signtool.exe error"
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
	if (Test-Path $packageMsi)
	{
		Remove-Item $packageMsi
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
$packageZip = "${product}${version}.zip"
$packageMsi = "${product}${version}.msi"
$packageAppxRoot = "Appx\$product"
$packageAppxFiles = "$packageAppxRoot\PackageFiles"
$packageAppxProduct = "$packageAppxRoot\PackageFiles\$product"
$packageX86Appx = "${product}${version}-x86.appx"
$packageX64Appx = "${product}${version}-x64.appx"


if (-not $continue)
{
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects
	
	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Save-AssemblyInfo $projectFile $assemblyInfoFile
	Build-Project "x86" $assemblyVersion
	Build-Project "x64" $assemblyVersion
	Restore-AssemblyInfo  $projectFile $assemblyInfoFile

	#
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package $productX86Dir $packageX86Dir
	New-Package $productX64Dir $packageX64Dir
}

#
if (($Target -eq "All") -or ($Target -eq "Zip"))
{
	Write-Host "`[Zip] ...`n" -fore Cyan
	New-Zip
	Write-Host "`nExport $packageZip successed.`n" -fore Green
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	Write-Host "`n[Installer] ...`n" -fore Cyan

	New-PackageAppend $packageDir
	New-Msi $packageDir $packageMsi

	Write-Host "`nExport $packageMsi successed.`n" -fore Green
}


if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if ((Test-Path $packageAppxRoot) -and (Test-Path "Appx/_Parameter.ps1"))
	{
		New-AppxReady
		New-Appx "x64" $packageX64Appx
		Write-Host "`nExport $packageX64Appx successed.`n" -fore Green
		New-Appx "x86" $packageX86Appx
		Write-Host "`nExport $packageX86Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist make appx envionment. skip!`n" -fore Yellow
	}
}

# current
Write-Host "`n[Current] ...`n" -fore Cyan
if (Test-Path $packageDir)
{
	if (-not (Test-Path $product))
	{
		New-Item $product -ItemType Directory
	}
	Copy-Item "$packageDir\*" "$product\" -Recurse -Force
}
else
{
	Write-Host "`nWarning: not exist$packageDir. skip!`n" -fore Yellow
}

#--------------------------
# saev buid version
Set-BuildCount $buildCount

#-------------------------
# Finish.
Write-Host "`nBuild $buildVersion All done.`n" -fore Green





