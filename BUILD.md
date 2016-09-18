# ビルド方法

## 環境

Windows / VisualStudio 2015 update2

## 構成

このプロジェクトとは別に SevenZipSharp のサブプロジェクトが必要です。  
別リポジトリで管理されているので、このプロジェクトの作業フォルダーと並列な場所にチェックアウトしてください。

|作業フォルダ名   | リポジトリ
|------------------|-------------------
|/NeeView          | [NeeView本体(現在のソリューション)](https://neelabo@bitbucket.org/neelabo/neeview.git)
|/SevenZipSharp | [RAR5対応カスタムSevenZipSharp](https://github.com/neelabo/SevenZipSharp.git)


## PublishToZip

配布用のZipを作成します。  
Publish.ps1 (PowerScript) でビルドからパッケージ化までを行っています。  
markdown から ドキュメント用html を作成するために [pandoc](http://pandoc.org/) を使用しています。

