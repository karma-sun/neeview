# DesktopAppConverter で msi から appx を作成
# 最初の１回のみ必要。その後は Deploy.ps1 で差し替え更新する。

## sample msi
$msi = (dir "*.msi" | select -First 1).Name
$msi = "..\_oldversions\NeeView1.25.msi" #1.26系だとエラーが？

## DAC
& DesktopAppConverter.exe -Installer $msi -Destination . -PackageName "NeeView" -Publisher "CN=NeeLaboratory" -Version 1.0.0.0 -MakeAppx -Verbose

