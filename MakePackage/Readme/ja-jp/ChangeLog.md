## 更新履歴

----

### 39.5
(2022-08-11)

#### 修正
- アプリ終了時にエラーになる現象を軽減するためにSQLiteライブラリを以前のものに戻した
- 読み込み専用ショートカットを処理できない不具合修正
- サブフォルダーを読み込むブックのロードがキャンセルできない不具合修正
- 言語ファイル更新(zh-TW)

----

### 39.4
(2022-07-04)

#### 新着
- Windows11 のスナップレイアウトに対応

#### 修正
- ファイル追加時に本棚の除外フィルターが機能しない不具合修正
- 特定の操作によってサムネイル表示がとても重くなる不具合修正
- 最小化からフルスクリーンに復帰したときに画像の座標がずれる不具合修正
- フォルダーのサムネイルが更新されない不具合修正
- 「検索ボックスにフォーカスを移す」コマンドでフォーカスされないことがある不具合修正
- 前後のブックを開くときに「この場所ではサブフォルダーを読み込む」設定が機能しない不具合を修正
- サブフォルダーを読み込んでいるブックから巡回移動したときにサブフォルダーに移動してしまう不具合を修正
- パス指定ダイアログに不正なパスが渡されたときの不具合修正
- プレイリストをブックとして開くときにショートカットファイルが認識されない不具合修正
- 複数アーカイブファイルのショートカットをドラッグ＆ドロップしたときの不具合対応
- UNICODE名のショートカットを認識できなかった不具合修正
- 履歴リストから削除しても反映しないことがある不具合修正
- プレイリスト「現在のブックのみ表示」での「前の（次の）プレイリスト項目に移動」コマンドでエラーになる不具合修正
- リサイズフィルター適用時に明るさが変化することがある問題が解消しました
- スクリプト：Patch() の影響が残り続ける不具合を修正
- スクリプト：大きい配列での問題が解消しました
- 誤字修正

#### 変更
- 各種ライブラリ更新
- 言語ファイル更新(zh-TW)

----

### 39.3
(2021-07-17)

#### 新着

- 言語：中文(中国)に対応

#### 修正

- 最小化からフルスクリーン復帰時にタスクバーが表示されてしまう不具合修正
- タスクバーを自動的に隠す設定のときにウィンドウ最大化でタスクバーが表示されない不具合の改善
- 「前の履歴に戻る/進む」コマンドでエラーになる不具合修正
- フローティングパネルでリネームできない不具合修正
- クイックアクセスのリネーム時の初期選択範囲不具合修正
- フォルダーツリーのコンテキストメニューにショートカットキーが表示されない不具合修正
- "#"を含むパスにアプリを配置したときにテーマ読み込みに失敗する不具合修正

#### 変更

- 「ファイルをコピー」コマンドパラメーターに「テキスト コピー」設定を追加。クリップボードにコピーされるテキストの種類を選択します。

----

### 39.2
(2021-06-26)

#### 修正

- メインメニューがフォーカスを奪わないように修正
- 起動ヘルプダイアログのレイアウトが壊れる不具合修正
- チルトホイール１操作で１コマンドになるように修正 (設定 > コマンド > チルトホイール操作を１回に制限する)

----

### 39.1
(2021-06-20)

#### 修正

- 「スクロール＋前のページに戻る」コマンドのパラメーターを設定するとスクロールタイプが「斜めスクロール」に変化してしまう不具合修正
- リサイズフィルター適用時にぼやけることがある不具合修正
- マルチバイト文字や空白のあるパスにアプリを配置するとREADMEファイルが開けなくなる不具合修正

----

### 39.0
(2021-06-18)

#### 重要

##### ページマークをプレイリストに統合

- ページマークは廃止されました。これまでのページマークは「Pagemark」という名前のプレイリストとして引き継がれます。
- プレイリストパネルが新しく追加されました。
- プレイリストは複数作ることが可能で、切り替えて使用します。選択されているプレイリストをページマークのように扱えます。
- プレイリストパネルで管理されるプレイリストは専用のフォルダーに配置されたものに限られますが、既存のプレイリストファイルもこれまで通り使用可能です。
- ページマークではブック単位でのグループ分けでしたが、プレイリストではフォルダーや圧縮ファイル単位となります。

##### 外観の刷新

- ほぼすべてのUIコントロールが調整されました。
- テーマを増やしました。メニュー部のテーマーカラー設定は廃止されました。 (設定 > ウィンドウ > テーマ)
- カスタムテーマを作成することで自由に配色することが可能になりました。テーマファイルフォーマットは[こちら](https://bitbucket.org/neelabo/neeview/wiki/Theme)を参照してください。
- 設定ウィンドウにもテーマが適用されるようになりました。
- フォントの設定が全体的に見直されました (設定 > フォント)

##### 情報パネル刷新

- 多くのEXIF情報を表示するようにしました。
- 2ページ表示の場合に表示情報を切り替えられるようにしました。

#### 新着

- 言語：中文(台湾)に対応。(提供者に感謝！)
- 設定：使用するウェブブラウザー、テキストエディタの設定追加。(設定 > 全般)
- 設定：スクリプトやカスタムテーマをエクスポートデータに追加。
- コマンド： コマンドを複製できるようにした。設定のコマンドリストでコマンドを右クリックして「複製」で作成。パラメーターのあるコマンドのみ複製できます。
- コマンド：「無効な履歴を削除」追加。
- コマンド：チルトホイール対応。
- メインビュー：ホバースクロール実装。(メニュー > 画像 > ホバースクロール)
- メインビュー：余白設定を追加。(設定 > ウィンドウ > メインビューの余白)
- メインビュー：タッチ長押しでのルーペ対応。
- クイックアクセス：名前を変更できるようにした。クイックアクセスのプロパティから参照パスの変更も可能です。
- ナビゲーター：表示領域サムネイルを追加。(ナビゲーターパネルの詳細メニューから)
- ナビゲーター：ブック移動でも回転拡縮等を維持する設定追加。ナビゲータパネルのプッシュピンボタンのコンテキストメニューから変更する。
- ページスライダー：スライダー表示ON/OFFコマンド追加。 (メニュー > 表示 > スライダー)
- ページスライダー：スライダーのプレイリスト登録マーク表示ON/OFF設定追加。(設定 > スライダー)
- フィルムストリップ：プレイリスト登録マークを表示。(設定 > フィルムストリップ)
- フィルムストリップ：フィルムストリップにコンテキストメニュー実装。
- スクリプト：エラーレベルの設定追加。(設定 > スクリプト > 廃止されたメンバーアクセスのエラーレベル)
- スクリプト：スクリプトフォルダーの変更を監視するようにしました。
- スクリプト：スクリプトコマンド引数 nv.Args[] を追加。スクリプトコマンドのコマンドパラメーターで指定します。
- スクリプト：ページ切り替えイベント OnPageChanged を追加。
- スクリプト：ページ読み込み完了を待機する命令 nv.Book.Wait() 追加。
- スクリプト：nv.Environment追加
- 開発：多言語開発環境を整備。詳細は[こちら](https://bitbucket.org/neelabo/neeview/src/master/Language/Readme.md)を参照してください。

#### 修正

- 設定：拡張子設定でセミコロンを使用した際にデータがおかしくなる不具合修正。
- 設定：拡張子設定の初期化ボタンが機能しない不具合修正。
- 設定：設定の検索後にリストボックスが消失する不具合修正。
- その他：ページ記録が機能していない不具合修正。
- ウィンドウ：まれにサムネイル画像がポップアップされる不具合修整。
- ウィンドウ：コンテキストメニューを閉じた時にパネルも非表示になることがある不具合修正。
- ウィンドウ：特定のポップアップサムネイルの表示サイズがおかしくなる不具合修正。
- ウィンドウ：リストの複数選択挙動修正。
- メインビュー：RAWカメラ画像の回転で縦横比がおかしくなることがある不具合修正。
- 本棚：現在のブックを示すマークが表示されないことがある不具合修正。
- スクリプトコンソール：exitでアプリが不正終了する不具合修正。
- スクリプト：画像サイズが制限後の値になっていた不具合修正。
- スクリプト：ShowInputDialogのEnterキー入力がメインウィンドウに影響してしまう不具合修正。
- スクリプト：デフォルトのパス設定でのパスを取得できるようにした。

#### 変更

- 設定：初期状態でのファイル操作許可をOFFにしました。 (メニュー > オプション > ファイル操作許可)
- ネットワーク：ネットワークアクセス許可設定がOFFのとき、Webブラウザーでネット接続する場合には無効でなく確認ダイアログを表示するようにした。
- コマンド：N字スクロールをZ字スクロールにするコマンドパラメータ追加。
- コマンド：N字スクロールコマンドに改行単位での停止ができる設定追加。
- コマンド：外部アプリの作業ディレクトリ設定を追加。
- コマンド：外部アプリで複数ページを開くときに左ページから開くモード追加。
- コマンド：インポート、エクスポートコマンドにコマンドパラメータ追加。
- ブック：ページの並び順に登録順を追加。プレイリストの場合にのみ機能。それ以外では名前順として機能します。
- ウィンドウ：サイドパネルとメニューやスライダーの重なり部分の自動表示判定設定追加。 (設定 > パネル)
- ウィンドウ：自動表示判定のエリア幅を縦方向と横方向に分けた。 (設定 > パネル)
- ウィンドウ：メインウィンドウ全体のタブ移動を左上から右下になるように整備。
- メインビュー：アニメーションしないGIFを画像として処理するようにした。
- メインビュー：マウスドラッグ操作にパラメーターを追加。 (設定 > マウス操作)
- 本棚：「ホームの場所」で検索パスも有効にした。
- ページリスト：親移動で現在ブックを選択ページとして開くようにした。
- エフェクト：サイズ指定の機能拡張。
- ページスライダー：太さの設定追加。 (設定 > スライダー)
- ページスライダー：プレイリスト登録マーク表示デザインを変更。
- スクリプト：最初にスクリプトフォルダーを開いたときにフォルダーやサンプルを作成するように変更。

#### 削除

- コマンド：「タイトルバーの表示ON/OFF」コマンドを削除。
- パネル：補足テキストの不透明度設定廃止。カスタムテーマで設定可能。
- 本棚：詳細メニューの「プレイリストを保存」を削除。
- フィルムストリップ：「フィルムストリップの背景を表示する」設定廃止。ページスライダーの不透明度に連動。
- スクリプト：廃止されたメンバーがあります。詳細はスクリプトヘルプの「廃止されたメンバー」を参照してください。

----

これ以前の更新履歴は[こちら](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog)を参照してください。
