$ProjectDir = "..\.."

$ResTextDir = "$ProjectDir\Language\Source"
$ResxDir = "$ProjectDir\NeeView\Properties"

$title = "Convert .resx to .restext"
$message = "Overwrite .restext, OK?"
$objYes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "do convert"
$objNo = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "exit"
$objOptions = [System.Management.Automation.Host.ChoiceDescription[]]($objYes, $objNo)

$resultVal = $host.ui.PromptForChoice($title, $message, $objOptions, 1)
if ($resultVal -ne 0) { exit }

Get-ChildItem "$ResxDir\Resources*.resx" | Foreach-Object {
    resgen.exe $_ "$ResTextDir\$($_.BaseName).restext"
}

Write-Host "done."
