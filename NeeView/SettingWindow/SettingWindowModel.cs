using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
                new SettingPage("外部連携", null),
                new SettingPage("ページ設定", null),
                new SettingPage("履歴設定", null),
                new SettingPage("スライドショー", null),
                new SettingPage("コマンド", null),
                new SettingPage("メニュー", null),
                new SettingPage("詳細設定", null),
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
                    new SettingItemIndexValue(PropertyMemberElement.Create(DragTransform.Current, nameof(DragTransform.AngleFrequency)), new AngleFrequency()),
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
                    new SettingItemGroup(null,
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
    }


}
