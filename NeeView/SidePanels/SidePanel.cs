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

            // ナビゲート
            this.NavigatePanel = new NavigatePanel(NavigateModel.Current);
            rightPanels.Add(this.NavigatePanel);

            // ブックマーク
            this.BookmarkPanel = new BookmarkPanel(BookmarkFolderList.Current);
            rightPanels.Add(this.BookmarkPanel);

            // ページマーク
            this.PagemarkPanel = new PagemarkPanel(PagemarkList.Current);
            rightPanels.Add(this.PagemarkPanel);

            // ページリストパネル。サイドパネル配置もしくは本棚配置
            PageListPlacementService.Current.Update();
            if (this.PageListPanel != null)
            {
                rightPanels.Add(this.PageListPanel);
            }

            // パネル群を登録
            this.InitializePanels(leftPanels, rightPanels);

            SelectedPanelChanged += SidePanel_SelectedPanelChanged;

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsDecoratePlace), SidePanelProfile_IsDecoratePlaceChanged);
        }


        // 各種類のパネルインスタンス
        public FolderPanel FolderListPanel { get; private set; }
        public HistoryPanel HistoryPanel { get; private set; }
        public FileInformationPanel FileInfoPanel { get; private set; }
        public ImageEffectPanel ImageEffectPanel { get; private set; }
        public NavigatePanel NavigatePanel { get; private set; }
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
            RaisePropertyChanged(nameof(IsVisibleNavigatePanel));
        }

        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return IsSelectedPanel(FileInfoPanel); }
            set { SetSelectedPanel(FileInfoPanel, value); RaisePanelPropertyChanged(); }
        }

        public void SetVisibleFileInfo(bool isVisible, bool flush)
        {
            SetSelectedPanel(FileInfoPanel, isVisible, flush);
            RaisePanelPropertyChanged();
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

        public void SetVisibleEffectInfo(bool isVisible, bool flush)
        {
            SetSelectedPanel(ImageEffectPanel, isVisible, flush);
            RaisePanelPropertyChanged();
        }

        public bool ToggleVisibleEffectInfo(bool byMenu)
        {
            ToggleSelectedPanel(ImageEffectPanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleEffectInfo;
        }



        // ナビゲートパネル情報表示ON/OFF
        public bool IsVisibleNavigatePanel
        {
            get { return IsSelectedPanel(NavigatePanel); }
            set { SetSelectedPanel(NavigatePanel, value); RaisePanelPropertyChanged(); }
        }

        public void SetVisibleNavigatePanel(bool isVisible, bool flush)
        {
            SetSelectedPanel(NavigatePanel, isVisible, flush);
            RaisePanelPropertyChanged();
        }

        public bool ToggleVisibleNavigatePanel(bool byMenu)
        {
            ToggleSelectedPanel(NavigatePanel, byMenu);
            RaisePanelPropertyChanged();
            return IsVisibleNavigatePanel;
        }

        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return IsSelectedPanel(FolderListPanel); }
            set { SetSelectedPanel(FolderListPanel, value); RaisePanelPropertyChanged(); }
        }

        public void SetVisibleFolderList(bool isVisible, bool flush)
        {
            SetSelectedPanel(FolderListPanel, isVisible, flush);
            RaisePanelPropertyChanged();
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
                    FolderPanelModel.Current.IsPageListVisible = value;
                }
                RaisePanelPropertyChanged();
            }
        }

        public void SetVisiblePageList(bool isVisible, bool flush)
        {
            if (PageListPanel != null)
            {
                SetSelectedPanel(PageListPanel, isVisible, flush);
            }
            else
            {
                FolderPanelModel.Current.IsPageListVisible = isVisible;
            }
            RaisePanelPropertyChanged();
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
                    PageListPlacementService.Current.Focus();
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
        public bool IsVisibleFolderTree
        {
            get { return Config.Current.Bookshelf.IsFolderTreeVisible && IsVisibleFolderList; }
            set { SetVisibleFolderTree(false, value); }
        }

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

            Config.Current.Bookshelf.IsFolderTreeVisible = isVisible;

            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookshelf.IsFolderTreeVisible;
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return IsSelectedPanel(HistoryPanel); }
            set { SetSelectedPanel(HistoryPanel, value); RaisePanelPropertyChanged(); }
        }

        public void SetVisibleHistoryList(bool isVisible, bool flush)
        {
            SetSelectedPanel(HistoryPanel, isVisible, flush);
            RaisePanelPropertyChanged();
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

        public void SetVisibleBookmarkList(bool isVisible, bool flush)
        {
            SetSelectedPanel(BookmarkPanel, isVisible, flush);
            RaisePanelPropertyChanged();
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

        public void SetVisiblePagemarkList(bool isVisible, bool flush)
        {
            SetSelectedPanel(PagemarkPanel, isVisible, flush);
            RaisePanelPropertyChanged();
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
