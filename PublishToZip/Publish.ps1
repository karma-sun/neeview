$target = 'NeeView'

$config = 'Release'

# error to break
trap { break }

#---------------------
# Functions

function Get-FileVersion($fileName)
{
	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart
	"$major.$minor"
}


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


#----------------------
# build
$msbuild = 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
$solutionDir = "E:\Documents\Visual Studio 2015\Projects\$target"
$solution = "$solutionDir\$target.sln"
$projectDir = "$solutionDir\$target"
$targetDir = "$projectDir\bin\$config"

& $msbuild $solution /p:Configuration=$config /t:Clean,Build


# get assembly version
$version = Get-FileVersion "$targetDir\$target.exe"

#----------------------
# package section
$packageDir = $target + $version
$packageZip = $packageDir + ".zip"

# remove packageDir
if (Test-Path $packageDir)
{
	Remove-Item $packageDir -Recurse
}
if (Test-Path $packageZip)
{
	Remove-Item $packageZip
}

# make package folder
$temp = New-Item $packageDir -ItemType Directory

# copy
Copy-Item "$targetDir\$target.exe" $packageDir
Copy-Item "$targetDir\$target.exe.config" $packageDir
Copy-Item "$targetDir\*.dll" $packageDir

#------------------------
# generate README.html

$readmeDir = $packageDir + "\readme"
$temp = New-Item $readmeDir -ItemType Directory 

Copy-Item "$solutionDir\*.md" $readmeDir
#Copy-Item "$solutionDir\Style.html" $readmeDir

# edit README.md
Replace-Content "$readmeDir\README.md" "# $target" "# $target $version"

# markdown to html by pandoc
pandoc -s -t html5 -o "$packageDir\README.html" -H Style.html "$readmeDir\README.md" "$readmeDir\LICENSE.md" "$readmeDir\THIRDPARTY_LICENSES.md"

Remove-Item $readmeDir -Recurse

#--------------------------
# archive to ZIP
Compress-Archive $packageDir -DestinationPath $packageZip

#-------------------------
# Finish.
Write-Host "`nExport $packageZip successed.`n" -fore Green





