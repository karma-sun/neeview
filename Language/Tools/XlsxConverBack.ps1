$ResTextDir = "..\Source"
$OutpuXlsx = "..\Resources.xlsx"

$TempDir = New-TemporaryFile | Foreach-Object { Remove-Item $_; mkdir $_ }
try {
    ResxXlsxConv\ResxXlsxConv.exe -to-resx -trunc -designer:NeeView "$TempDir\Resources.resx" $OutpuXlsx
    Get-ChildItem "$TempDir\Resources*.resx" | Foreach-Object {
        resgen.exe $_ "$ResTextDir\$($_.BaseName).restext"
    }
    Write-Host "done."
}
finally
{
    $TempDir | Where-Object { Test-Path $_ } | ForEach-Object { Get-ChildItem $_ -File -Recurse | Remove-Item; $_} | Remove-Item -Recurse
}


