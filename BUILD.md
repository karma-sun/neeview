# ビルド方法

## 開発環境

Windows 10 / VisualStudio 2015

## Gitからプロジェクトを取得

% git clone --recursive https://neelabo@bitbucket.org/neelabo/neeview.git NeeView

## 配布パッケージ作成

配布用のZip,Msiを作成します。  
Deploy.ps1 (PowerScript) でビルドからパッケージ化までを行っています。  
markdown から ドキュメント用html を作成するために [pandoc](http://pandoc.org/) を使用しています。
msiパッケージ作成には [WiX Toolset](http://wixtoolset.org/) を使用しています。
