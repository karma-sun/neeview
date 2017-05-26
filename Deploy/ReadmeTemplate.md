# NeeView <VERSION/>

## ソフトの概要

  フォルダー内の画像を本のように閲覧できる画像ビューアーです。  

  * 標準対応画像フォーマット (bmp, jpg, gif, tiff, png, ico, WIC対応画像)
  * 圧縮ファイル対応 (zip, rar, 7z, lzh)
  * 多重圧縮ファイル対応
  * マウスジェスチャー対応
  * キーカスタマイズ、ジェスチャ設定可能
  * ドラッグによる移動、回転、拡縮
  * ルーペモード
  * 見開き表示モード
  * フルスクリーンモード
  * スライドショー機能
  * マルチスレッド、先読み対応
  * Webブラウザからの画像ドロップ
  * Susieプラグイン対応(UNICODEファイル名可)(※NeeViewS版のみ)

  詳細は以下のページを参照してください。
  
  * [NeeViewプロジェクト Wiki](https://bitbucket.org/neelabo/neeview/wiki/)


## 動作環境

  * Windows 7 SP1, Windows 8.1, Windows 10
  * .Net 4.6.2 以降が必要です。起動しない場合は [Microsoft ダウンロードセンター](https://www.microsoft.com/ja-jp/download/details.aspx?id=53345) から入手してインストールしてください。


## NeeView.exe と NeeViewS.exe

  NeeView.exe と NeeViewS.exe の２種類の実行ファイルがあります。  
  NeeView は通常版です。  
  NeeViewS はSusieプラグインに対応していますが、32bit動作になります。

  |    |64bitOS|32bitOS|Susie|
  |----|----|----|----|----
  |NeeView.exe|○|○|✕|
  |NeeViewS.exe|○※|○|○|※32bitで動作


## インストール・アンインストール方法

### Zip版

  * NeeView<VERSION/>.zip

  NeeView.exe と NeeViewS.exe 両方の実行ファイルが含まれています。
  設定ファイルは共通です。

  インストール不要です。Zipを展開後、そのまま `NeeView.exe` もしくは `NeeViewS.exe` を実行してください。  
  設定ファイル等ユーザーデータも同じ場所に保存されます。  

  アンインストールはファイルを削除するだけです。レジストリは使用していません。

### インストーラー版

  * NeeView<VERSION/>.msi
  * NeeViewS<VERSION/>.msi

  NeeView版とNeeViewS版でインストーラーが別れています。  
  同時にインストールすることは可能ですが、設定ファイルが共有されることにご注意ください。  

  実行するとインストールが開始されます。インストーラーの指示に従ってください。  
  設定ファイル等ユーザーデータは各ユーザのアプリデータフォルダーに保存されます。  
  このフォルダーは NeeView のメニューの「その他」＞「設定ファイルの場所を開く」で確認できます。  
  
  アンインストールはOSのアプリ管理機能からアンイントールします。  
  ただし、設定データ等のユーザデータはアンインストールだけでは消えませんのでご注意ください。
  手動で消すか、アンインストール前に NeeView の設定の「全般設定」の一番下にある「全データを削除する」(インストール版のみの機能)を実行してください。



## 連絡先

 バグや要望がありましたらこちらのブログのコメントにてご連絡ください。
 
  * [ヨクアルナニカ](https://yokuarunanika.blogspot.jp/)
 
 バグや要望はこちらでも受け付けております。`ErrorLog.txt`を添付する場合はこちらがお薦めです。
 
  * [NeeViewプロジェクト 課題投稿](https://bitbucket.org/neelabo/neeview/issues/new)
 
メールでの連絡先

  * [nee.laboratory@gmail.com](mailto:nee.laboratory@gmail.com)