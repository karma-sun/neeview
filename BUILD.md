# ビルド方法

## 環境

Windows / VisualStudio 2015 update2

## Clone

% git clone --recursive https://neelabo@bitbucket.org/neelabo/neeview.git NeeView

## Deploy

配布用のZip,Msiを作成します。  
Deploy.ps1 (PowerScript) でビルドからパッケージ化までを行っています。  
markdown から ドキュメント用html を作成するために [pandoc](http://pandoc.org/) を使用しています。
msiパッケージ作成には [WiX Toolset](http://wixtoolset.org/) を使用しています。
