using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    // TODO: VMとの関係
    // TODO: ページの区分の見直し。
    // 全部実装できるまでひとまずここに全て定義する

    /// <summary>
    /// 設定画面 Model
    /// </summary>
    public class SettingWindowModel : BindableBase
    {
        private List<SettingPage> _pages;

        //
        public SettingWindowModel()
        {
            Initialize();
        }

        //
        public List<SettingPage> Pages
        {
            get { return _pages; }
        }

        //
        private void Initialize()
        {
            _pages = new List<SettingPage>()
            {
                new SettingPage("全般", CreateGeneralPage()),
                new SettingPage("表示設定", CreteViewPage()),
                new SettingPage("画像操作", CreteViewControlPage()),
                new SettingPage("外部連携", CreateExternalPage()),
                new SettingPage("ページ設定", CreatePageSettingPage()),
                new SettingPage("履歴設定", CreateHistoryPage()),
                new SettingPage("スライドショー", CreateSlideshowPage()),
                new SettingPage("Susie", CreateSusiePage()),
                new SettingPage("コマンド", CreateCommandPage()),
                new SettingPage("メニュー", CreateMenuPage()),
                new SettingPage("詳細設定", CreateDetailPage()),
            };

            _pages.First().IsSelected = true;
        }


        #region 全般

        //
        private List<SettingItem> CreateGeneralPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("基本",
                    new SettingItemProperty(PropertyMemberElement.Create(ArchiverManager.Current, nameof(ArchiverManager.IsEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsArchiveRecursive)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(new Binding(nameof(ArchiverManager.IsEnabled)) {Source = ArchiverManager.Current})
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.Memento.IsEnableAnimatedGif))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.Memento.IsEnableNoSupportFile)))),

                new SettingItemSection("起動設定",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsMultiBootEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveWindowPlacement))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsSaveFullScreen))),
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.IsOpenLastBook))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsAutoPlaySlideShow)))),

                new SettingItemSection("メモリ関連",
                    new SettingItemProperty(PropertyMemberElement.Create(MemoryControl.Current, nameof(MemoryControl.IsAutoGC))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.PreLoadMode)))),

                new SettingItemSection("ページ送り",
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsPrioritizePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsMultiplePageMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current, nameof(BookOperation.PageEndAction)))),

                 new SettingItemSection("その他",
                    new SettingItemButton("サムネイルキャッシュを", "削除する",  RemoveCache),
                    new SettingItemButton("全データを", new WarningText("削除する"), RemoveAllData)
                    {
                        Tips = "ユーザデータを削除し、アプリケーションを終了します。\nアンインストール前に履歴等を完全に削除したい場合に使用します",
                        Visibility = new VisibilityPropertyValue(IsVisibleRemoveDataButton() ? Visibility.Visible : Visibility.Collapsed)
                    }),
            };
        }

        /// <summary>
        /// データ削除ボタンを表示する？
        /// </summary>
        private bool IsVisibleRemoveDataButton() => Config.Current.IsUseLocalApplicationDataFolder && !Config.Current.IsAppxPackage;

        /// <summary>
        /// RemoveCache command.
        /// </summary>
        private RelayCommand _RemoveCache;
        public RelayCommand RemoveCache
        {
            get { return _RemoveCache = _RemoveCache ?? new RelayCommand(RemoveCache_Executed); }
        }

        private void RemoveCache_Executed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// RemoveAllData command.
        /// </summary>
        private RelayCommand _RemoveAllData;
        public RelayCommand RemoveAllData
        {
            get { return _RemoveAllData = _RemoveAllData ?? new RelayCommand(RemoveAllData_Executed); }
        }

        private void RemoveAllData_Executed()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 表示設定

        private List<SettingItem> CreteViewPage()
        {
            string windowTitleFormatTips = $@"フォーマットの説明

$Book -- 開いているブック名
$Page -- 現在ページ番号
$PageMax-- 最大ページ番号
$ViewScale-- ビュー操作による表示倍率(%)
$FullName[LR]-- パスを含むファイル名
$Name[LR]-- ファイル名
$Size[LR]-- ファイルサイズ(ex. 100×100)
$SizeEx[LR]-- ファイルサイズ + ピクセルビット数(ex. 100×100×24)
$Scale[LR]-- 画像の表示倍率(%)

""◯◯◯[LR]"" は、1ページ用、2ページ用で変数名が変わることを示します
例えば $Name は1ページ用、 $NameL は２ページ左用、 $NameR は2ページ右用になります
$Name は2ページ表示時には主となるページ(ページ番号の小さい方)になります";

            return new List<SettingItem>
            {
                new SettingItemSection("テーマ",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.PanelColor)))),

                new SettingItemSection("背景",
                    new SettingItemButton("カスタム背景", "設定", EditCustomBackground)),

                new SettingItemSection("通知表示",
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NoticeShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.CommandShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.GestureShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.NowLoadingShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(InfoMessage.Current, nameof(InfoMessage.ViewTransformShowMessageStyle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsOriginalScaleShowMessage)))),

                 new SettingItemSection("ウィンドウタイトル",
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat1))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(WindowTitle.Current, nameof(WindowTitle.WindowTitleFormat2))) {IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsVisibleWindowTitle)))) {Tips = windowTitleFormatTips },

                 new SettingItemSection("スライダー",
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderDirection))),
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.SliderIndexLayout)))),


                 new SettingItemSection("サムネイルリスト",
                    new SettingItemProperty(PropertyMemberElement.Create(PageSlider.Current, nameof(PageSlider.IsSliderLinkedThumbnailList))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailNumber))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsVisibleThumbnailPlate))),
                    new SettingItemProperty(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.IsSelectedCenter))),
                    new SettingItemThumbnailSize(PropertyMemberElement.Create(ThumbnailList.Current, nameof(ThumbnailList.ThumbnailSize)))),

                new SettingItemSection("パネル",
                    new SettingItemProperty(PropertyMemberElement.Create(MainWindowModel.Current, nameof(MainWindowModel.IsHidePanelInFullscreen)))),

                new SettingItemSection("フォルダーリスト",
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleBookmarkMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.IsVisibleHistoryMark))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.FolderIconLayout))),
                    new SettingItemProperty(PropertyMemberElement.Create(FolderList.Current, nameof(FolderList.Home))) {IsStretch = true}),

                new SettingItemSection("ファイル情報",
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleFilePath))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsUseExifDateTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleBitsPerPixel))),
                    new SettingItemProperty(PropertyMemberElement.Create(FileInformation.Current, nameof(FileInformation.IsVisibleLoader)))),

                new SettingItemSection("エフェクト",
                    new SettingItemProperty(PropertyMemberElement.Create(ImageEffect.Current, nameof(ImageEffect.IsHsvMode)))),

                new SettingItemSection("2ページ表示",
                    new SettingItemProperty(PropertyMemberElement.Create(ContentCanvas.Current, nameof(ContentCanvas.ContentsSpace))) { IsStretch = true }),
            };
        }

        /// <summary>
        /// EditCustomBackground command.
        /// </summary>
        public RelayCommand EditCustomBackground
        {
            get { return _EditCustomBackground = _EditCustomBackground ?? new RelayCommand(EditCustomBackground_Executed); }
        }

        //
        private RelayCommand _EditCustomBackground;

        //
        private void EditCustomBackground_Executed()
        {
            var dialog = new BackgroundSettingWindow(ContentCanvasBrush.Current.CustomBackground.Clone());
            dialog.Owner = SettingWindowEx.Current;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                ContentCanvasBrush.Current.CustomBackground = dialog.Result;
            }
        }

        #endregion

        #region 画像操作

        private List<SettingItem> CreteViewControlPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("ビュー操作",
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.IsLimitMove))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsViewStartPositionCenter))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsControlCenterImage))),
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.AngleFrequency)), new AngleFrequency()),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepScale))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepAngle))),
                    new SettingItemProperty(PropertyMemberElement.Create(DragTransformControl.Current, nameof(DragTransformControl.IsKeepFlip)))),

                new SettingItemSection("マウスドラッグ操作",
                    new SettingItemMouseDrag()),

                new SettingItemSection("マウス長押し操作",
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongLeftButtonDownMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Normal, nameof(MouseInputNormal.LongLeftButtonDownTime))) {IsStretch = true}),

                new SettingItemSection("ルーペ",
                    new SettingItemProperty(PropertyMemberElement.Create(LoupeTransform.Current, nameof(LoupeTransform.IsVisibleLoupeInfo))),
                    new SettingItemProperty(PropertyMemberElement.Create(MouseInput.Current.Loupe, nameof(MouseInputLoupe.IsLoupeCenter)))),

                new SettingItemSection("タッチ操作",
                    new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current, nameof(TouchInput.IsEnabled))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.DragAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Normal, nameof(TouchInputNormal.HoldAction))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsAngleEnabled))),
                        new SettingItemProperty(PropertyMemberElement.Create(TouchInput.Current.Drag.Manipulation, nameof(TouchDragManipulation.IsScaleEnabled))))
                    {
                        IsEnabled = new IsEnabledPropertyValue(TouchInput.Current, nameof(TouchInput.IsEnabled)),
                    }),
            };
        }

        #endregion

        #region 外部連携
        private List<SettingItem> CreateExternalPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("外部アプリ設定",  "「外部アプリで起動」コマンドで使用するアプリを指定します",
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Command))) { IsStretch = true },
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Parameter))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Normal, true),
                    },
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Protocol))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Protocol, true),
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveOption)))),

                new SettingItemSection("クリップボードへのファイルコピー",
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveOption)))),

                new SettingItemSection("ブラウザからのドラッグ&ドロップ", "Webブラウザからの画像ドロップに関連する設定です",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.DownloadPath))) { IsStretch = true }),

                new SettingItemSection("ファイル保存", "画像を保存するコマンドの設定です",
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.IsEnableExportFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.QualityLevel))) { IsStretch = true }),
            };
        }
        #endregion

        #region ページ設定
        private List<SettingItem> CreatePageSettingPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("履歴、ブックマークから復元するページ設定項目", "開いたことがあるブックの場合、前回の情報から設定の復元をします。\n復元しない項目は、既定のページ設定もしくは直前の設定が使用されます",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.Page))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.PageMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.BookReadOrder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedDividePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedWidePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleFirstPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsSupportedSingleLastPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.IsRecursiveFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.HistoryMementoFilter, nameof(BookMementoFilter.SortMode)))),

                new SettingItemSection("既定のページ設定の使用",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current, nameof(BookSetting.IsUseBookMementoDefault)))),

                new SettingItemSection("既定のページ設定", "「ページ設定の初期化」で使用される設定です",
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.PageMode))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.BookReadOrder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedDividePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedWidePage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedSingleFirstPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsSupportedSingleLastPage))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.IsRecursiveFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookSetting.Current.BookMementoDefault, nameof(Book.Memento.SortMode)))),

                new SettingItemSection("サブフォルダー読み込み問い合わせ",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsConfirmRecursive)))),

                new SettingItemSection("サブフォルダー読み込み自動判定",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsAutoRecursive))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsAutoRecursiveWithAllFiles)))
                    {
                        IsEnabled = new IsEnabledPropertyValue(BookHub.Current, nameof(BookHub.IsAutoRecursive))
                    }),

                new SettingItemSection("PDF設定",
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.RenderSize)))),

            };
        }
        #endregion

        #region 履歴設定
        private List<SettingItem> CreateHistoryPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("履歴保存数制限",
                    new SettingItemIndexValue<int>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSize)), new HistoryLimitSize()),
                    new SettingItemIndexValue<TimeSpan>(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.LimitSpan)), new HistoryLimitSpan())),

                new SettingItemSection("履歴削除",
                    new SettingItemGroup(
                        new SettingItemButton("履歴を", "削除する",  RemoveHistory))),

                new SettingItemSection("保存設定",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsInnerArchiveHistoryEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookHub.Current, nameof(BookHub.IsUncHistoryEnabled)))),

                new SettingItemSection("フォルダーリスト",
                    new SettingItemProperty(PropertyMemberElement.Create(BookHistory.Current, nameof(BookHistory.IsKeepFolderStatus)))),
            };
        }

        /// <summary>
        /// RemoveHistory command.
        /// </summary>
        public RelayCommand RemoveHistory
        {
            get { return _RemoveHistory = _RemoveHistory ?? new RelayCommand(RemoveHistory_Executed); }
        }

        //
        private RelayCommand _RemoveHistory;

        //
        private void RemoveHistory_Executed()
        {
            BookHistory.Current.Clear();

            // TODO:
            //var dialog = new MessageDialog("", "履歴を削除しました");
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }
        #endregion

        #region スライドショー

        private List<SettingItem> CreateSlideshowPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("設定",
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsSlideShowByLoop))),
                    new SettingItemProperty(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.IsCancelSlideByMouseMove))),
                    new SettingItemIndexValue<double>(PropertyMemberElement.Create(SlideShow.Current, nameof(SlideShow.SlideShowInterval)), new SlideShowInterval())),
            };
        }

        #endregion

        #region Susie

        private List<SettingItem> CreateSusiePage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("全般",
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsEnableSusie))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.SusiePluginPath))),
                    new SettingItemSusiePlugin(),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieImage))),
                    new SettingItemProperty(PropertyMemberElement.Create(SusieContext.Current, nameof(SusieContext.IsFirstOrderSusieArchive)))),
            };
        }

        #endregion

        #region コマンド
        private List<SettingItem> CreateCommandPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("コマンド",
                    new SettingItemCommand()),
            };
        }
        #endregion

        #region コンテキストメニュー
        private List<SettingItem> CreateMenuPage()
        {
            return new List<SettingItem>
            {
                new SettingItemSection("コンテキストメニュー",
                    new SettingItemContextMenu()),
            };
        }
        #endregion

        #region 詳細設定
        private List<SettingItem> CreateDetailPage()
        {
            // TODO: Memento編集しなくなるので廃止予定
            return new List<SettingItem>
            {
                new SettingItemSection("詳細設定",
                    new SettingItemPreference()),
            };
        }
        #endregion
    }
}
