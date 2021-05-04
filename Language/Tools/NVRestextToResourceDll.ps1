# Resources.[culture].restext to Neeview.resources.dll
#
# Usage: NVRestextToResourceDll.ps1 [-restext] <Resources.[culture].restext> [[-output] <Neeview.resources.dll>]

Param(
    [Parameter(Mandatory=$true)]
    [string]
    $restext,

    [Parameter()]
    [string]
    $output = "NeeView.resources.dll"
)

if ($null -eq $env:NVROOT)
{
    Wrte-Hoist "The environment is bad. Please start with Start-NVDevPowerShell.bat"
    exit 1
}

if ($restext -notmatch "(^.+\\)?Resources\.(?<Culture>.+?)\.restext")
{
    Write-Host "Need input Resources.[culture].restext"
    exit 1
}

$root = $env:NVROOT
$objDir = mkdir "$root\Language\obj" -Force
$culture = $Matches.Culture
$resources = "$objDir\NeeView.Properties.Resources.$culture.resources";

Write-Host "ResGen.exe: output: $resources" -ForegroundColor Cyan
resgen $restext $resources
if ($? -ne $true)
{
    throw "ResGen.exe failed" 
}
Write-Host "ResGen.exe: done." -ForegroundColor Cyan

Write-Host "Al.exe: output: $output" -ForegroundColor Cyan
al /culture:$culture /out:$output /embed:$resources /fullpaths
if ($? -ne $true)
{
    throw "Al.exe failed"
}
Write-Host "Al.exe: done." -ForegroundColor Cyan
