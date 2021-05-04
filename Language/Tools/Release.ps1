# Release.ps1
#
# ** Not used in translation work. ***
#
# Convert to NeeView/Properties/Resources.*.resx
# Overwrites project resources.
# This is for development.

Param(
    [switch]$xlsx
)

if ($null -eq $env:NVROOT)
{
    Write-Host "The environment is bad. Please start with Start-NVDevPowerShell.bat"
    exit 1
}

$root = $env:NVROOT
$sourceDir = "$root\Language\Source"
$propertiesDir = "$root\NeeView\Properties"

if ($xlsx)
{
    Write-Host "convert from Resources.xslx ..."
    NVXlsxToRestext.ps1
}

Get-ChildItem "$sourceDir\Resources*.restext" | Foreach-Object {

    $resx = "$propertiesDir\$($_.BaseName).resx";
    $designer = "$propertiesDir\$($_.BaseName).Designer.cs";

    if ($_.Name -eq "Resources.restext") {
        resgen.exe $_ $resx /str:cs,NeeView.Properties,Resources,$designer /publicClass
    }
    else {
        resgen.exe $_ $resx
    }

}

Write-Host "done."
