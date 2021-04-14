$ProjectDir = "..\.."

$ResTextDir = "$ProjectDir\Language\Source"
$ResxDir = "$ProjectDir\NeeView\Properties"

Get-ChildItem "$ResTextDir\Resources*.restext" | Foreach-Object {

    $resx = "$ResxDir\$($_.BaseName).resx";
    $designer = "$ResxDir\$($_.BaseName).Designer.cs";

    if ($_.Name -eq "Resources.restext") {
        resgen.exe $_ $resx /str:cs,NeeView.Properties,Resources,$designer /publicClass
    }
    else {
        resgen.exe $_ $resx
    }

}

Write-Host "done."
