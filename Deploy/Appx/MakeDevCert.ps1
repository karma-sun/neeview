# 開発用オレオレ証明書作成

# error to break
trap { break }
$ErrorActionPreference = "Stop"

$Name = "_my"
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\x64"

#
& "$Win10SDK\makecert.exe" -r -h 0 -n "CN=NeeLaboratory" -eku 1.3.6.1.5.5.7.3.3 -pe -sv "$Name.pvk" "$Name.cer"
if ($? -ne $true)
{
	throw "makecert.exe error"
}

#
& "$Win10SDK\pvk2pfx.exe" -pvk "$Name.pvk" -spc "$Name.cer" -pfx "$Name.pfx"
if ($? -ne $true)
{
	throw "pvk2pfx.exe error"
}