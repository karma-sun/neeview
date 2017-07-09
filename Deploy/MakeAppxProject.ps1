## sample msi
$msi = (dir "*.msi" | select -First 1).Name
$msi = "_oldversions\NeeView1.25.msi"

## DAC
& DesktopAppConverter.exe -Installer $msi -Destination Appx -PackageName "NeeView" -Publisher "CN=NeeLaboratory" -Version 1.0.0.0 -MakeAppx -Verbose

