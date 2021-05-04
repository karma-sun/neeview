# *.restext to Resources.xlsx
#
# Usage: NVRestextToXlsx.ps1

if ($null -eq $env:NVROOT)
{
    Write-Host "The environment is bad. Please start with Start-NVDevPowerShell.bat"
    exit 1
}

$root = $env:NVROOT
$sourceDir = "$root\Language\Source"
$objDir = mkdir "$root\Language\obj" -Force
$outpuXlsx = "$root\Language\Resources.xlsx"

Get-ChildItem "$sourceDir\Resources*.restext" | Foreach-Object {
    resgen.exe $_ "$objDir\$($_.BaseName).resx"
    if ($? -ne $true)
    {
        throw "ResGen.exe failed"
    }
}

& $env:NVTOOLS\ResxXlsxConv\ResxXlsxConv.exe -to-xlsx "$objDir\Resources.resx" $outpuXlsx
if ($? -ne $true)
{
    throw "ResxXlsxConv.exe failed"
}