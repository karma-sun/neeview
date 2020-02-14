using NeeView.Effects;
using NeeView.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// NeeView用 サイドパネル管理
    /// </summary>
    public class SidePanel : SidePanelFrameModel
    {
        // NOTE: Initialize()必須
        static SidePanel() => Current = new SidePanel();
        public static SidePanel Current { get; }

        private SidePanel()
        {
        }

        public void Initialize()
        {
            Interop.NVFpReset();

            var leftPanels = new List<IPanel>();
            var rightPanels = new List<IPanel>();

            // フォルダーリスト
            this.FolderListPanel = new FolderPanel(FolderPanelModel.Current, BookshelfFolderList.Current, PageList.Current);
            leftPanels.Add(this.FolderListPanel);

            // 履歴
            this.HistoryPanel = new HistoryPanel(HistoryList.Current);
            leftPanels.Add(this.HistoryPanel);

            // ファイル情報
            this.FileInfoPanel = new FileInformationPanel(FileInformation.Current);
            rightPanels.Add(this.FileInfoPanel);

            // エフェクト
            this.ImageEffectPanel = new ImageEffectPanel(ImageEffect.Current, ImageFilter.Current);
            rightPanels.Add(this.ImageEffectPanel);

            // ブックマーク
            this.BookmarkPanel = new BookmarkPanel(BookmarkFolderList.Current);
            rightPanels.Add(this.BookmarkPanel);

            // ページマーク
            this.PagemarkPanel = new PagemarkPanel(PagemarkList.Current);
            rightPanels.Add(this.PagemarkPanel);

            // パネル群を登録
            this.InitializePanels(leftPanels, rightPanels);

            // ページリストパネルの更新
            PageListPlacementService.Current.Update();

            SelectedPanelChanged += SidePanel_SelectedPanelChanged;

            SidePanelProfile.Current.AddPropertyChanged(nameof(SidePanelProfile.IsDecoratePlace), SidePanelProfile_IsDecoratePlaceChanged);
        }


        // 各種類のパネルインスタンス
        public FolderPanel FolderListPanel { get; private set; }
        public HistoryPanel HistoryPanel { get; private set; }
        public FileInformationPanel FileInfoPanel { get; private set; }
        public ImageEffectPanel ImageEffectPanel { get; private set; }
        public PagemarkPanel PagemarkPanel { get; private set; }
        public PageListPanel PageListPanel { get; private set; }
        public BookmarkPanel BookmarkPanel { get; private set; }


        private void SidePanelProfile_IsDecoratePlaceChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// パネル選択変更時の処理
        /// </summary>
        private void SidePanel_SelectedPanelChanged(object sender, SelectedPanelChangedEventArgs e)
        {
            RaisePanelPropertyChanged();
        }

        /// <summary>
        /// ページリストパネルの追加
        /// </summary>
        public void AttachPageListPanel(PageListPanel panel)
        {
            Attach(panel);
            PageListPanel = panel;
        }

        /// <summary>
        /// ページリストパネルを削除
        /// </summary>
        public void DetachPageListPanel()
        {
            if (PageListPanel == null)
            {
                return;
            }

            // 配置位置を記憶
            PageListPanel.DefaultPlace = this.Left.Contains(PageListPanel) ? PanelPlace.Left : PanelPlace.Right;

            Detach(PageListPanel);
            PageListPanel = null;
        }



        #region Panels Visibility

        //
        private void RaisePanelPropertyChanged()
        {
            RaisePropertyChanged(nameof(IsVisibleFolderList));
            RaisePropertyChanged(nameof(IsVisibleHistoryList));
            RaisePropertyChanged(nameof(IsVisibleBookmarkList));
            RaisePropertyChanged(nameof(IsVisiblePagemarkList));
            RaisePropertyChanged(nameof(IsVisiblePageList));
            RaisePropertyChanged(nameof(IsVisibleFileInfo));
            RaisePropertyChanged(nameof(IsVisibleEffectInfo));
        }

        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return IsSelectedPanel(FileInfoPanel); }
            set { SetSelectedPanel(FileInfoPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisibleFileInfo(bool byMenu)
        {
            ToggleSelectedPanel(FileInfoPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleFileInfo;
        }


        // エフェクト情報表示ON/OFF
        public bool IsVisibleEffectInfo
        {
            get { return IsSelectedPanel(ImageEffectPanel); }
            set { SetSelectedPanel(ImageEffectPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisibleEffectInfo(bool byMenu)
        {
            ToggleSelectedPanel(ImageEffectPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleEffectInfo;
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return IsSelectedPanel(FolderListPanel); }
            set { SetSelectedPanel(FolderListPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisibleFolderList(bool byMenu)
        {
            ToggleSelectedPanel(FolderListPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleFolderList;
        }


        // ページリスト
        public bool IsVisiblePageList
        {
            get
            {
                return PageListPanel != null
                    ? IsSelectedPanel(PageListPanel)
                    : FolderPanelModel.Current.IsPageListVisible && IsVisibleFolderList;
            }
            set
            {
                if (PageListPanel != null)
                {
                    SetSelectedPanel(PageListPanel, value);
                }
                else
                {
                    FolderPanelModel.Current.IsPageListVisible = true;
                }
                RaisePanelPropertyChanged();
            }
        }

        public bool ToggleVisiblePageList(bool byMenu)
        {
            // パネル
            if (PageListPanel != null)
            {
                ToggleSelectedPanel(PageListPanel, byMenu);
                RaisePanelPropertyChanged();
                return IsVisiblePageList;
            }

            // 本棚の一部
            else
            {
                var model = FolderPanelModel.Current;

                if (byMenu || !model.IsPageListVisible || IsVisiblePanel(FolderListPanel))
                {
                    model.IsPageListVisible = !IsVisiblePageList;
                }
                SetSelectedPanel(FolderListPanel, true);
                RaisePanelPropertyChanged();

                if (model.IsPageListVisible)
                {
                    PageList.Current.FocusAtOnce();
                }

                return model.IsPageListVisible;
            }
        }

        public bool FocusBookmarkList(bool byMenu)
        {
            // フォルダーツリーは「ブックマークリスト」を選択した状態にする
            BookshelfFolderTreeModel.Current.SelectRootBookmarkFolder();
            BookshelfFolderList.Current.RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.Refresh);

            // フォルダーリスト選択
            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();

            // フォルダーリストにフォーカスをあわせる
            if (!byMenu && IsVisibleFolderList)
            {
                BookshelfFolderList.Current.FocusAtOnce();
            }

            return IsVisibleFolderList;
        }

        /// <summary>
        /// 検索ボックス表示状態
        /// </summary>
        public bool IsVisibleFolderSearchBox => BookshelfFolderList.Current.IsFolderSearchBoxVisible && IsVisibleFolderList;


        //
        public void FocusFolderSearchBox(bool byMenu)
        {
            SetSelectedPanel(FolderListPanel, true);

            BookshelfFolderList.Current.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// フォルダーツリー表示状態
        /// </summary>
        public bool IsVisibleFolderTree => BookshelfFolderList.Current.IsFolderTreeVisible && IsVisibleFolderList;

        /// <summary>
        /// フォルダーツリー表示状態切替
        /// </summary>
        public bool ToggleVisibleFolderTree(bool byMenu)
        {
            return SetVisibleFolderTree(byMenu, !IsVisibleFolderTree || !IsVisiblePanel(FolderListPanel));
        }

        public bool SetVisibleFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(FolderListPanel)}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                BookshelfFolderTreeModel.Current.FocusAtOnce();
            }

            BookshelfFolderList.Current.IsFolderTreeVisible = isVisible;

            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();

            return BookshelfFolderList.Current.IsFolderTreeVisible;
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return IsSelectedPanel(HistoryPanel); }
            set { SetSelectedPanel(HistoryPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisibleHistoryList(bool byMenu)
        {
            ToggleSelectedPanel(HistoryPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleHistoryList;
        }

        // ブックマークリスト表示ON/OFF
        public bool IsVisibleBookmarkList
        {
            get { return IsSelectedPanel(BookmarkPanel); }
            set { SetSelectedPanel(BookmarkPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisibleBookmarkList(bool byMenu)
        {
            ToggleSelectedPanel(BookmarkPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleBookmarkList;
        }

        // ページマークリスト表示ON/OFF
        public bool IsVisiblePagemarkList
        {
            get { return IsSelectedPanel(PagemarkPanel); }
            set { SetSelectedPanel(PagemarkPanel, value); RaisePanelPropertyChanged(); }
        }

        public bool ToggleVisiblePagemarkList(bool byMenu)
        {
            ToggleSelectedPanel(PagemarkPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisiblePagemarkList;
        }

        #endregion
    }
}
