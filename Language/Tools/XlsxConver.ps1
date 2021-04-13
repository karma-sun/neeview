$ResTextDir = "..\Source"
$OutpuXlsx = "_Resources.xlsx"

$TempDir = New-TemporaryFile | Foreach-Object { Remove-Item $_; mkdir $_ }
try {
    Get-ChildItem "$ResTextDir\Resources*.restext" | Foreach-Object {
        resgen.exe $_ "$TempDir\$($_.BaseName).resx"
    }
    ResxXlsxConv\ResxXlsxConv.exe -to-xlsx "$TempDir\Resources.resx" $OutpuXlsx
    Write-Host "done."
}
finally
{
    $TempDir | Where-Object { Test-Path $_ } | ForEach-Object { Get-ChildItem $_ -File -Recurse | Remove-Item; $_} | Remove-Item -Recurse
}
