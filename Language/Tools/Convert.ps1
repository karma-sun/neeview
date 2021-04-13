$ProjectDir = "..\.."

$ResTextDir = "$ProjectDir\Language\Source"
$ResxDir = "$ProjectDir\NeeView\Properties"

Get-ChildItem "$ResTextDir\Resources*.restext" | Foreach-Object {
    resgen.exe $_ "$ResxDir\$($_.BaseName).resx"
}

Write-Host "done."
