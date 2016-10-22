$fxc ="C:\Program Files (x86)\Windows Kits\10\bin\x86\fxc.exe"

trap {break}

foreach($fx in Get-ChildItem "*.fx")
{
    $ps = [System.IO.Path]::ChangeExtension($fx, ".ps");
    & $fxc $fx /T ps_2_0 /Fo $ps
    if(!$?)
    {
        throw "compile error."
    }
}
