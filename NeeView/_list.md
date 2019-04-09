## App

App.config
App.Exception.cs
app.manifest
App.Memento.cs
App.Option.cs
App.xaml
App.xaml.cs
NeeView.csproj
NeeView.csproj.user
packages.config

## BokHistory

BookHistory.cs
BookHistoryCollection.cs
BookHistoryCommand.cs // 履歴操作

## System
Config.cs
ApplicationDisposer.cs // アプリ終了時にまとめてDisposeするサービス
AppDispatcher.cs // App.Current.Disppther ラッパー

## Controls
AutoFadeTriggerAction.cs // 自動フェイドアウト用のビヘイビア

## BookHub
BookAddress.cs
BookHub.cs
BookHubCommandEngine.cs
BookHubHistory.cs
BookLoadOption.cs // BookHubのLoad用オプションフラグ

## BookMemento 
BookMementoCollection.cs
BookMementoCollectionChangedArgs.cs
BookMementoFilter.cs // ページ設定をビットマスクで管理
BookMementoUnit.cs // BookMementoCollection用の単位
BookSetting.cs // 現在適用されているBookMementoの管理

## BookOperation
BookOperation.cs


## Command
CommandElement.cs
CommandParameters.cs
CommandTable.cs
CommandType.cs
BindingGenerator.cs // コマンドのメニューに対するバインド生成
RoutedCommandTable.cs


## ContentCanvas
ContentCanvas.cs // 表示ページをUIコントロールに変換する役目
ContentCanvasBrush.cs // コンテンツの全面、背景ブラシ
ContentRebuild.cs // リサイズによるコンテンツ再生成
ContentSizeCalcurator.cs // 表示モードに対応したコンテンツ表示サイズの計算
BackgroundStyle.cs // 背景の種類定義
BrushSource.cs // 背景カスタムブラシ
GridLine.cs // エフェクトのグリッドライン


## Sysmte
ContentDropReciever.cs // 外部からのデータドロップ受付
ExternalApplication.cs // 外部起動アプリ
FileIconCollection.cs // ファイルアイコンリソース管理
FileIO.cs // ファイル操作系
FileIOProfile.cs
MemoryControl.cs // GC
MultbootService.cs // 多重起動制御用管理
Language.cs // 言語の切り替えの管理
TrashBox.cs
RemoteCommandService.cs // プロセス間通信
SoundPlayerService.cs
Temporary.cs // 一時ファイル管理
Models.cs ... どうしよう？

## SlideShow
SlideShow.cs


## DelayAction
DelayAction.cs
DelayVisibility.cs

## MouseInput (Drag)
DragAction.cs
DragActionTable.cs
DragArea.cs
DragViewOrigin.cs

## Archiver
EntrySort.cs // ArchiveEntryの並び替え

## ExportPage
Exporter.cs
ExporterProfile.cs
SaveWindow.xaml // ページ保存時の設定ウィンドウ
SaveWindow.xaml.cs

## SidePanels/FileInfo
FileInfoItem.xaml // ファイル情報パネル用項目UI
FileInfoItem.xaml.cs

## SidePalens/Bookshelf/FolderList
FolderOrder.cs // フォルダーリストの並び順

## Help .. もっと良い名前にする
HtmlHelpUtility.cs // HTMLヘルプ生成用

## MouseInput
LongButtonDownMode.cs // 長押しボタンモード


## MainWindow
MainWindow.xaml
MainWindow.xaml.cs
MainWindowModel.cs
MainWindowViewModel.cs
MainWindowResourceDictionary.xaml
AnyKey.cs // キーが押されているか判定。ContentRebuildでのリサイズトリガに使用されている

## Menu
MenuTree.cs // コマンドからメニューコントロール生成。メインメニューの生成も。
MenuExtensions.cs // メニューコントロールのジェスチャーテキスト更新
ContextMenuSetting.cs // コンテキストメニュー

## Controls
MessageDialog.xaml // 汎用メッセージダイアログ
MessageDialog.xaml.cs



## InfoMessage
InfoMessage.cs // 表示メッセージ管理
NormalInfoMessage.cs
NormalInfoMessageView.xaml
NormalInfoMessageView.xaml.cs
NormalInfoMessageViewModel.cs
TinyInfoMessage.cs
TinyInfoMessageView.xaml
TinyInfoMessageView.xaml.cs
TinyInfoMessageViewModel.cs

## Book
BookProfile.cs
PageMode.cs // 1/2ページモード定義
PagePart.cs // 表示ページ情報。半分とか。
PagePosition.cs // ページ座標
PageRange.cs // ページ範囲
PageReadOrder.cs // ページ方向
PageSortMode.cs // ページ並び順
PageStretchMode.cs // 表示サイズモード


## Rename ファイル名とかの名前変更用
RenameControl.xaml
RenameControl.xaml.cs
RenameManager.xaml
RenameManager.xaml.cs




## SaveData
SaveData.cs
SaveDataBackup.cs
SaveDataSync.cs
UserSetting.cs
BackupSelectControl.xaml // インポート時のデータ選択UI
BackupSelectControl.xaml.cs


## SidePanels/Bookshelf/FolderList
SearchEnginecs.cs // 検索エンジン


## Controls
SliderTextBox.xaml // ページスライダー横の入力テキストボックス
SliderTextBox.xaml.cs


## Toast
Toast.cs
ToastService.cs


## VersionWindow
VersionWindow.xaml
VersionWindow.xaml.cs

## ViewContent
ViewPageCollection.cs // ViewContentSourceをまとめたもの。データ構造

## SidePanels/Pagemark
VirtualCollection.cs // 仮想パネル管理。ページマークパネルでの項目の実体化判定に利用


## Window
WindowCaptionButtons.xaml // 閉じるボタンとかのUI
WindowCaptionButtons.xaml.cs
WindowCaptionEmulator.cs // キャプションバーのドラッグとかのエミュレート
WindowTitle.cs // ウィンドウタイトル管理
WindowPlacement.cs // Window座標
WindowShape.cs // Windowスタイル
ThemeProfile.cs // Dark/Lightテーマ管理


## Obsolete (未使用)
WIndowHelper.cs // Window用。未使用？ 
Loupe.xaml // 別UIによる部分ルーペ。たぶん未使用
Loupe.xaml.cs

## System
WindowMessage.cs // WinProcのっとり。ドライブの検出とか？


## _NeeView

## _NeeView/Collections/Generic
LinkedDicionary.cs // 検索を高速化したLinkedListデータ構造。BookHistoryCollectionで使用されている

## _NeeView/Data
CommonExtensions.cs // 汎用SWAP。どこかに吸収させよう
IndexValue.cs // 番号と値のテーブル管理。設定用

## _NeeView/Text
SizeString.cs // "数値x数値" という文字列で数値を表す
StringCollection.cs // 文字コレクション。セミコロン区切りの文字列を配列に
HtmlParseUtility.cs // ドラッグデータ内のimgタグ検索 DragReciever
ReplaceString.cs // キーワード変換文字列操作。ウィンドウタイトルとか。



## Archiver
## Bitmap
## Book
## Bookamrk
## Controls
## Converters
## Development
## Effects
## InputGesture
## JobEngine
## MouseInput
## Obsolete
## OpenSourceControls
## Page
## Picture
## Print
## Resources
## Setting
## SidePanels
## Styles
## Susie
## Thumbnail
## TouchInput
## ViewContent
## _NeeLaboratory