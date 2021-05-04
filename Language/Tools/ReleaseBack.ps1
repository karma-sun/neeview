# ReleaseBack.ps1
#
# ** Not used in translation work. ***
#
# Convert from NeeView/Properties/Resources.*.resx
# Overwrites *.restext.
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

$title = "Convert .resx to .restext"
$message = "Overwrite .restext, OK?"
$objYes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "do convert"
$objNo = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "exit"
$objOptions = [System.Management.Automation.Host.ChoiceDescription[]]($objYes, $objNo)
$resultVal = $host.ui.PromptForChoice($title, $message, $objOptions, 1)
if ($resultVal -ne 0) { exit }

Get-ChildItem "$propertiesDir\Resources*.resx" | Foreach-Object {
    resgen.exe $_ "$sourceDir\$($_.BaseName).restext"
}

if ($xlsx)
{
    Write-Host "convert to Resources.xslx ..."
    NVRestextToXlsx.ps1
}

Write-Host "done."
