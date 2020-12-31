## 更新履歴

### 38.0
(2021-01-??)

#### 新着

- サイドパネルのドッキングパネル化。パネルをドラッグして接続させることができます。
- サイドパネルのフローティング対応。パネルアイコンもしくはパネルタイトルを右クリックして「フローティング」を実行すると、そのパネルがサブウィンドウ化されます。
- メインビューウィンドウ実装。メインビューを独立したウィンドウにします。(表示 > メインビューウィンドウ)
- 表示コンテンツサイズにウィンドウサイズをあわせる「ウィンドウサイズ調整」コマンド追加。
- 自動非表示モード設定追加。フルスクリーン以外のウィンドウ状態でも自動非表示を機能させることができます。(設定 > パネル > 自動非表示モード)
- AeroSnap座標の復元設定追加。 (設定 > 起動設定 > AeroSnapの座標を復元)
- ページ表示数に依存したスライダー移動の設定追加。 (設定 > ページスライダー > 変更量を表示ページ数に同期)
- WIC情報取得ON/OFF設定追加。 (設定 > 対応形式 > WIC情報を使用する)

#### 修正

- プリンターによっては印刷ダイアログを開こうとするとエラーになる不具合対応。
- WICの状態によっては起動できないことがある不具合修正。
- 除外フォルダー設定をすべて削除すると起動できなくなる不具合修正。
- 履歴のスタイル変更でサムネイルが更新されない不具合修正。
- フィルムストリップと表示が一致しないことがある不具合修正。
- メインメニューのショートカットが表示されないことがある不具合修正。
- ネットワークパスでアーカイブ内フォルダーが開けない不具合修正。
- 設定のインポートでブックマークの更新が終わらないことがある不具合修正。
- ページ間隔設定と回転でのストレッチ適用不具合修正。
- 回転後にページ移動したときのスケール維持の不具合修正。
- 動画のパスに「#」が含まれると再生できない不具合修正。
- 横長ページ分割時のページ送り不具合修正。
- ブックページをダブルクリックして開いた時にページが進んでしまう現象抑制。
- MP3等、映像のないメディアが再生できないことがある不具合改善。
- ショートカットキー名称の修正。

#### 変更

- サイドパネルグリップの透明化。
- テキストボックス以外でのIMEを無効化。
- バックアップファイルの生成を１起動に付き１回に制限。
- ストアアプリ版のデータ保存フォルダーを NeeLaboratory\NeeView.a から NeeLavoratory-NeeView に移動。アンインストールでデータが削除されないことがある問題解決のため。
- 開いたファイルの上位フォルダーが変更できなくなる問題解決のため、カレントディレクトリを常にexeと同じ場所になるようにしました。
- 自然順ソートの漢字の並び順を音読み順に変更。
- スクリプトが有効の場合のみ既定のスクリプトフォルダーを生成するようにした。 既定でないフォルダーが指定されている場合は生成しません。
- 設定読み込みエラーダイアログに詳細メッセージを追加、アプリ終了ボタンを追加。
- NeeView切り替え順番を起動順に変更。
- ページ設定の「ページ位置」に最終ページならば初期化する選択を追加。
- 「表示」メニューの順番調整。
- 「ファイル情報」を「情報」に変更。
- 各種ライブラリ更新。

#### 削除

- 「フルスクリーンのときにタスクバー領域を覆わない」設定削除。自動非表示モードで代用。
- 「ページリストを本棚に配置」設定削除。ドッキングパネルで代用。

#### スクリプト

- 修正：コマンドパラメーター変更が保存されない不具合修正。
- 修正：nv.Command.ToggleVisible*.Execute(true) でフォーカスが移動しない不具合修正。
- 修正：起動スクリプトで本棚にフォーカスが移動しない不具合修正。
- 新規：スクリプトファイルのドックコメントで既定のショートカットを指定できるようにした。
- 新規：nv.ShowInputDialog() 命令の追加。文字列入力ダイアログです。
- 新規：sleep() 命令の追加。指定された時間スクリプト処理を停止します。
- 新規：「スクリプト中断」コマンド追加。sleepを使用しているスクリプトの動作を停止させます。
- 新規：nv.Bookshelf等、各パネルアクセサの追加。本棚等のパネル単位のアクセサを追加しました。選択項目等の取得、設定ができます。
- 変更：スクリプトコンソール出力でオブジェクトの内容を出力するようにした。
- 変更：nv.Book のページアクセサ取得をメソッドからプロパティに変更。
    - nv.Book.Page(int) -> nv.Book.Pages\[int\] インデックスは0スタートになります。
    - nv.Book.ViewPage(int) -> nv.Book.ViewPages\[int\]
    - Pages[] ではページサイズ(Width,Height)は取得できません。 ViewPages[] では取得できます。
- nv.Config
    - 新規：nv.Config.Image.Standard.UseWicInformation
    - 新規：nv.Config.MainView.IsFloating
    - 新規：nv.Config.MainView.IsHideTitleBar
    - 新規：nv.Config.MainView.IsTopmost
    - 新規：nv.Config.MenuBar.IsHideMenuInAutoHideMode
    - 新規：nv.Config.Slider.IsSyncPageMode
    - 新規：nv.Config.System.IsInputMethodEnabled
    - 新規：nv.Config.Window.IsAutoHideInFullScreen
    - 新規：nv.Config.Window.IsAutoHideInNormal
    - 新規：nv.Config.Window.IsAutoHidInMaximized
    - 新規：nv.Config.Window.IsRestoreAeroSnapPlacement
    - 変更：nv.Config.Bookmark.IsSelected → nv.Bookmark.IsSelected
    - 変更：nv.Config.Bookmark.IsVisible → nv.Bookmark.IsVisible
    - 変更：nv.Config.Bookshelf.IsSelected → nv.Bookshelf.IsSelected
    - 変更：nv.Config.Bookshelf.IsVisible → nv.Bookshelf.IsVisible
    - 変更：nv.Config.Effect.IsSelected → nv.Effect.IsSelected
    - 変更：nv.Config.Effect.IsVisible → nv.Effect.IsVisible
    - 変更：nv.Config.History.IsSelected → nv.History.IsSelected
    - 変更：nv.Config.History.IsVisible → nv.History.IsVisible
    - 変更：nv.Config.Information.IsSelected → nv.Information.IsSelected
    - 変更：nv.Config.Information.IsVisible → nv.Information.IsVisible
    - 変更：nv.Config.PageList.IsSelected → nv.PageList.IsSelected
    - 変更：nv.Config.PageList.IsVisible → nv.PageList.IsVisible
    - 変更：nv.Config.Pagemark.IsSelected → nv.Pagemark.IsSelected
    - 変更：nv.Config.Pagemark.IsVisible → nv.Pagemark.Visible
    - 変更：nv.Config.Panels.IsHidePanelInFullscreen → nv.Config.Panels.IsHidePanelInAutoHideMode
    - 変更：nv.Config.Slider.IsHidePageSliderInFullscreen → nv.Config.Slider.IsHidePageSliderInAutoHideMode
    - 削除：nv.Config.Bookshelf.IsPageListDocked → x
    - 削除：nv.Config.Bookshelf.IsPageListVisible → x
    - 削除：nv.Config.Window.IsFullScreenWithTaskBar → x
- nv.Command
    - 新規：ToggleMainViewFloating
    - 新規：StretchWindow
    - 新規：CancelScript
    - 変更：FocusPrevAppCommand → FocusPrevApp
    - 変更：FocusNextAppCommand → FocusNextApp
    - 変更：TogglePermitFileCommand → TogglePermitFile
    - 削除：TogglePageListPlacement → x

----

これ以前の更新履歴は[こちら](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog)を参照してください。
