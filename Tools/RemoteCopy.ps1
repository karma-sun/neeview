# タブレットリモートデバッグ用
# ビルド後にリモートPCにコピーする。ビルドイベントから呼ばれる。
# 環境依存大

Param
(
    [parameter(Mandatory)][string]$targetDir 
)

$ErrorActionPreference = "Stop"

$sourceDir = $targetDir.TrimEnd("\")
$remoteDir = $sourceDir.Replace("C:", "\\CHLOE");

robocopy $sourceDir $remoteDir /e /xo