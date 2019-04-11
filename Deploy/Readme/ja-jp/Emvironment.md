## 動作環境

  * Windows 7 SP1, Windows 8.1, Windows 10
  * .NET Framework 4.7.2 以降が必要です。起動しない場合は [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet-framework-runtime) から入手してインストールしてください。


## NeeView と NeeViewS

  NeeView と NeeViewS の２種類の実行ファイルがあります。  
  NeeViewは OS によって 32bit/64bit 動作を切り替えますが、 NeeViewSは32bit動作限定です。64bit動作のほうがより多くのメモリを使用可能です。  
  32bitと64bit動作では使用できるSusieプラグインの種類が異なります。

  |        |64bitOS   |   32bitOS|
  |--------|----------|----------|
  |NeeView |64bit/.sph|32bit/.spi|
  |NeeViewS|32bit/.spi|32bit/.spi|

* .spi ... [たけちん氏制作の画像ビューアSusie用のプラグイン形式](http://www.digitalpad.co.jp/~takechin/)
* .sph ... [TORO氏提案の64bit仕様のSusieプラグイン形式](http://toro.d.dooo.jp/slplugin.html)


## インストール・アンインストール方法

### Zip版

  * NeeView<VERSION/>.zip

  NeeView と NeeViewS 両方の実行ファイルが含まれています。
  設定ファイルは共通です。

  インストール不要です。Zipを展開後、そのまま `NeeView.exe` もしくは `NeeViewS.exe` を実行してください。  
  設定ファイル等ユーザーデータも同じ場所に保存されます。  

  アンインストールはファイルを削除するだけです。レジストリは使用していません。

### インストーラー版

  * NeeView<VERSION/>.msi

  NeeView と NeeViewS 両方の実行ファイルが含まれています。
  設定ファイルは共通です。

  実行するとインストールが開始されます。インストーラーの指示に従ってください。  
  設定ファイル等ユーザーデータは各ユーザのアプリデータフォルダーに保存されます。  
  このフォルダーは NeeView のメニューの「その他」＞「設定ファイルの場所を開く」で確認できます。  
  
  アンインストールには、OSの「アプリと機能」を使用します。  
  ただし、設定データ等のユーザデータはアンインストールだけでは消えません。
  手動で消すか、アンインストール前に NeeView の設定の「データの削除」(インストール版のみの機能)を実行してください。
