# x86 バイナリを x64 環境で動作するときのメモリ上限を 4GB に拡張

Param( [parameter(Mandatory)][string]$target )

Write-Host "### LARGEADDRESSAWARE ###"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$vswhere = "$scriptPath\vswhere.exe"

$vspath = & $vswhere -property installationPath
$editbin = "$vspath\VC\Tools\MSVC\14.10.25017\bin\HostX64\x86\editbin.exe"

& $editbin /LARGEADDRESSAWARE $target


