using NeeLaboratory.ComponentModel;
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
using System.Windows.Input;

namespace NeeView
{
    public class VisibleAtOnceRequestEventArgs : EventArgs
    {
        public VisibleAtOnceRequestEventArgs(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
    }

    /// <summary>
    /// NeeView用 サイドパネル管理
    /// </summary>
    public class SidePanelFrame : BindableBase
    {
        static SidePanelFrame() => Current = new SidePanelFrame();
        public static SidePanelFrame Current { get; }


        private bool _isVisibleLocked;


        private SidePanelFrame()
        {
            CustomLayoutPanelManager.Current.CollectionChanged += (s, e) => RaisePanelPropertyChanged();
        }



        // パネル表示要求
        public event EventHandler<VisibleAtOnceRequestEventArgs> VisibleAtOnceRequest;


        // サイドバー表示ロック。自動非表示にならないようにする
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        public void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!Config.Current.Panels.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }


        #region Panels Visibility

        private void RaisePanelPropertyChanged()
        {
            RaisePropertyChanged(nameof(IsVisibleFolderList));
            RaisePropertyChanged(nameof(IsVisibleHistoryList));
            RaisePropertyChanged(nameof(IsVisibleBookmarkList));
            RaisePropertyChanged(nameof(IsVisiblePageList));
            RaisePropertyChanged(nameof(IsVisibleFileInfo));
            RaisePropertyChanged(nameof(IsVisibleEffectInfo));
            RaisePropertyChanged(nameof(IsVisibleNavigator));
            RaisePropertyChanged(nameof(IsVisiblePlaylist));
        }


        public void VisibleAtOnce(string key)
        {
            VisibleAtOnceRequest?.Invoke(this, new VisibleAtOnceRequestEventArgs(key));
        }


        private bool IsVisiblePanel(string key)
        {
            return CustomLayoutPanelManager.Current.IsPanelSelected(key);
        }

        private void SetVisiblePanel(string key, bool isVisible)
        {
            CustomLayoutPanelManager.Current.SelectPanel(key, isVisible);
            RaisePanelPropertyChanged();
        }

        private bool ToggleVisiblePanel(string key, bool byMenu)
        {
            bool isVisible = !CustomLayoutPanelManager.Current.IsPanelSelected(key) || (!byMenu && !CustomLayoutPanelManager.Current.IsPanelVisible(key));
            SetVisiblePanel(key, isVisible);
            return isVisible;
        }



        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return IsVisiblePanel(nameof(FileInformationPanel)); }
            set { SetVisiblePanel(nameof(FileInformationPanel), value); }
        }

        // TODO: flushって？
        public void SetVisibleFileInfo(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(FileInformationPanel), isVisible);
        }

        public bool ToggleVisibleFileInfo(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(FileInformationPanel), byMenu);
        }



        // エフェクト情報表示ON/OFF
        public bool IsVisibleEffectInfo
        {
            get { return IsVisiblePanel(nameof(ImageEffectPanel)); }
            set { SetVisiblePanel(nameof(ImageEffectPanel), value); }
        }

        public void SetVisibleEffectInfo(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(ImageEffectPanel), isVisible);
        }

        public bool ToggleVisibleEffectInfo(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(ImageEffectPanel), byMenu);
        }



        // ナビゲートパネル情報表示ON/OFF
        public bool IsVisibleNavigator
        {
            get { return IsVisiblePanel(nameof(NavigatePanel)); }
            set { SetVisiblePanel(nameof(NavigatePanel), value); }
        }

        public void SetVisibleNavigator(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(NavigatePanel), isVisible);
        }

        public bool ToggleVisibleNavigator(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(NavigatePanel), byMenu);
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return IsVisiblePanel(nameof(FolderPanel)); }
            set { SetVisiblePanel(nameof(FolderPanel), value); }
        }

        public void SetVisibleFolderList(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(FolderPanel), isVisible);
        }

        public bool ToggleVisibleFolderList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(FolderPanel), byMenu);
        }


        // ページリスト
        public bool IsVisiblePageList
        {
            get { return IsVisiblePanel(nameof(PageListPanel)); }
            set { SetVisiblePanel(nameof(PageListPanel), value); }
        }

        public void SetVisiblePageList(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(PageListPanel), isVisible);
        }

        public bool ToggleVisiblePageList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(PageListPanel), byMenu);
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return IsVisiblePanel(nameof(HistoryPanel)); }
            set { SetVisiblePanel(nameof(HistoryPanel), value); }
        }

        public void SetVisibleHistoryList(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(HistoryPanel), isVisible);
        }

        public bool ToggleVisibleHistoryList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(HistoryPanel), byMenu);
        }


        // ブックマークリスト表示ON/OFF
        public bool IsVisibleBookmarkList
        {
            get { return IsVisiblePanel(nameof(BookmarkPanel)); }
            set { SetVisiblePanel(nameof(BookmarkPanel), value); }
        }

        public void SetVisibleBookmarkList(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(BookmarkPanel), isVisible);
        }

        public bool ToggleVisibleBookmarkList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(BookmarkPanel), byMenu);
        }

        // プレイリスト表示ON/OFF
        public bool IsVisiblePlaylist
        {
            get { return IsVisiblePanel(nameof(PlaylistPanel)); }
            set { SetVisiblePanel(nameof(PlaylistPanel), value); }
        }

        public void SetVisiblePlaylist(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(PlaylistPanel), isVisible);
        }

        public bool ToggleVisiblePlaylist(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(PlaylistPanel), byMenu);
        }

        #endregion Panels Visibility

        #region Bookshelf parts

        /// <summary>
        /// 本棚のブックマークグループを表示
        /// </summary>
        public bool FocusBookshelfBookmarkList(bool byMenu)
        {
            // フォルダーツリーは「ブックマークリスト」を選択した状態にする
            BookshelfFolderTreeModel.Current.SelectRootBookmarkFolder();
            BookshelfFolderList.Current.RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.Refresh);

            // フォルダーリスト選択
            SetVisiblePanel(nameof(FolderPanel), true);
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
        public bool IsVisibleBookshelfSearchBox => BookshelfFolderList.Current.IsFolderSearchBoxVisible && IsVisibleFolderList;

        /// <summary>
        /// 検索ボックスにフォーカスを移す
        /// </summary>
        public void FocusBookshelfSearchBox(bool byMenu)
        {
            SetVisibleFolderList(true, true);
            BookshelfFolderList.Current.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// フォルダーツリー表示状態
        /// </summary>
        public bool IsVisibleBookshelfFolderTree
        {
            get { return Config.Current.Bookshelf.IsFolderTreeVisible && IsVisibleFolderList; }
            set { SetVisibleBookshelfFolderTree(false, value); }
        }

        /// <summary>
        /// フォルダーツリー表示状態切替
        /// </summary>
        public bool ToggleVisibleBookshelfFolderTree(bool byMenu)
        {
            return SetVisibleBookshelfFolderTree(byMenu, !IsVisibleBookshelfFolderTree || !IsVisiblePanel(nameof(FolderPanel)));
        }

        /// <summary>
        /// フォルダーツリー表示状設定
        /// </summary>
        public bool SetVisibleBookshelfFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(nameof(FolderPanel))}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                BookshelfFolderTreeModel.Current.FocusAtOnce();
            }

            Config.Current.Bookshelf.IsFolderTreeVisible = isVisible;

            SetVisiblePanel(nameof(FolderPanel), true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookshelf.IsFolderTreeVisible;
        }

        #endregion Bookshelf parts

        #region Bookmark parts


        /// <summary>
        /// フォルダーツリー表示状設定
        /// </summary>
        public bool SetVisibleBookmarkFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(nameof(BookmarkPanel))}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                BookmarkFolderTreeModel.Current.FocusAtOnce();
            }

            Config.Current.Bookmark.IsFolderTreeVisible = isVisible;

            SetVisiblePanel(nameof(BookmarkPanel), true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookmark.IsFolderTreeVisible;
        }

        #endregion



        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsSideBarVisible { get; set; }

            [DataMember]
            public bool IsManipulationBoundaryFeedbackEnabled { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Left { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Right { get; set; }

            #region Obsolete
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public string FontName { get; set; } // ver 32.0

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public double FontSize { get; set; } // ver 32.0

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public double FolderTreeFontSize { get; set; } // ver 32.0

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsTextWrapped { get; set; } // ver 32.0

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public double NoteOpacity { get; set; } // ver 32.0
            #endregion


            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                config.Panels.IsSideBarEnabled = IsSideBarVisible;
                config.Panels.IsManipulationBoundaryFeedbackEnabled = IsManipulationBoundaryFeedbackEnabled;
                config.Panels.PanelDocks = new Dictionary<string, PanelDock>();
                foreach (var panelTypeCode in Left.PanelTypeCodes)
                {
                    config.Panels.PanelDocks.Add(panelTypeCode, PanelDock.Left);
                }
                foreach (var panelTypeCode in Right.PanelTypeCodes)
                {
                    config.Panels.PanelDocks.Add(panelTypeCode, PanelDock.Right);
                }
                config.Panels.LeftPanelSeleted = Left.SelectedPanelTypeCode;
                config.Panels.RightPanelSeleted = Right.SelectedPanelTypeCode;
                config.Panels.LeftPanelWidth = Left.Width;
                config.Panels.RightPanelWidth = Right.Width;
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
            }
        }

        #endregion
    }
}
