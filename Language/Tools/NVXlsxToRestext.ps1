## Resources.xlsx to *.restext
## Usage: NVXlsxToRestext.ps1

if ($null -eq $env:NVROOT)
{
    Write-Host "The environment is bad. Please start with Start-NVDevPowerShell.bat"
    exit 1
}

$root = $env:NVROOT
$sourceDir = "$root\Language\Source"
$objDir = mkdir "$root\Language\obj" -Force
$outpuXlsx = "$root\Language\Resources.xlsx"

& $env:NVTOOLS\ResxXlsxConv\ResxXlsxConv.exe -to-resx -trunc -designer:NeeView "$objDir\Resources.resx" $outpuXlsx
if ($? -ne $true)
{
    throw "ResxXlsxConv.exe failed"
}

Get-ChildItem "$objDir\Resources*.resx" | Foreach-Object {
    resgen.exe $_ "$sourceDir\$($_.BaseName).restext"
    if ($? -ne $true)
    {
        throw "ResGen.exe failed"
    }
}

Write-Host "done."



