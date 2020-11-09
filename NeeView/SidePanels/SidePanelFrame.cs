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
    /// <summary>
    /// NeeView用 サイドパネル管理
    /// </summary>
    public class SidePanelFrame : BindableBase
    {
        static SidePanelFrame() => Current = new SidePanelFrame();
        public static SidePanelFrame Current { get; }


        private bool _isVisibleLocked;
        private MainLayoutPanelManager _layoutPanelManager => MainLayoutPanelManager.Current; // ##


        private SidePanelFrame()
        {
        }

        /// <summary>
        /// パネル内容更新イベント.
        /// 自動非表示時間のリセットに使用される.
        /// </summary>
        public event EventHandler<SidePanelContentChangedEventArgs> ContentChanged;


        // サイドバー表示ロック。自動非表示にならないようにする
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// コンテンツ変更通知
        /// </summary>
        public void RaiseContentChanged(string key)
        {
            ContentChanged?.Invoke(this, new SidePanelContentChangedEventArgs(key));
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
            RaisePropertyChanged(nameof(IsVisiblePagemarkList));
            RaisePropertyChanged(nameof(IsVisiblePageList));
            RaisePropertyChanged(nameof(IsVisibleFileInfo));
            RaisePropertyChanged(nameof(IsVisibleEffectInfo));
            RaisePropertyChanged(nameof(IsVisibleNavigator));
        }


        private bool IsVisiblePanel(string key)
        {
            return _layoutPanelManager.IsPanelSelected(key);
        }

        private void SetVisiblePanel(string key, bool isVisible)
        {
            _layoutPanelManager.SelectPanel(key, isVisible);
            RaisePanelPropertyChanged();
        }

        private bool ToggleVisiblePanel(string key, bool byMenu)
        {
            bool isVisible = !_layoutPanelManager.IsPanelSelected(key) || (!byMenu && !_layoutPanelManager.IsPanelVisible(key));
            SetVisiblePanel(key, isVisible);

            if (isVisible)
            {
                RaiseContentChanged(key);
            }

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


        #region Bookshelf parts

        // TODO: これ使ってる？
        public bool FocusBookmarkList(bool byMenu)
        {
            // フォルダーツリーは「ブックマークリスト」を選択した状態にする
            BookshelfFolderTreeModel.Current.SelectRootBookmarkFolder();
            BookshelfFolderList.Current.RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.Refresh);

            // フォルダーリスト選択
            ////SetSelectedPanel(FolderListPanel, true);
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
        public bool IsVisibleFolderSearchBox => BookshelfFolderList.Current.IsFolderSearchBoxVisible && IsVisibleFolderList;


        public void FocusFolderSearchBox(bool byMenu)
        {
            ////SetSelectedPanel(FolderListPanel, true);

            SetVisibleFolderList(true, true);
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
            return SetVisibleFolderTree(byMenu, !IsVisibleFolderTree || !IsVisiblePanel(nameof(FolderPanel)));
        }

        public bool SetVisibleFolderTree(bool byMenu, bool isVisible)
        {
            Debug.WriteLine($"{isVisible}, {IsVisiblePanel(nameof(FolderPanel))}");

            // フォーカス要求。表示前に要求する
            if (!byMenu && isVisible)
            {
                BookshelfFolderTreeModel.Current.FocusAtOnce();
            }

            Config.Current.Bookshelf.IsFolderTreeVisible = isVisible;

            ////SetSelectedPanel(FolderListPanel, true);
            SetVisiblePanel(nameof(FolderPanel), true);
            RaisePanelPropertyChanged();

            return Config.Current.Bookshelf.IsFolderTreeVisible;
        }

        #endregion Bookshelf parts



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


        // ページマークリスト表示ON/OFF
        public bool IsVisiblePagemarkList
        {
            get { return IsVisiblePanel(nameof(PagemarkPanel)); }
            set { SetVisiblePanel(nameof(PagemarkPanel), value); }
        }

        public void SetVisiblePagemarkList(bool isVisible, bool flush)
        {
            SetVisiblePanel(nameof(PagemarkPanel), isVisible);
        }

        public bool ToggleVisiblePagemarkList(bool byMenu)
        {
            return ToggleVisiblePanel(nameof(PagemarkPanel), byMenu);
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
                config.Panels.PanelDocks.Clear();
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

        [Obsolete]
        public Memento CreateMemento()
        {
            var memento = new Memento();
            return memento;
        }

        #endregion
    }
}
