using System;
using System.Collections.Generic;
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
        public static SidePanel Current { get; private set; }

        // フォーカス初期化要求
        // TODO: イベント名は原因であって期待する結果ではよくない
        public event EventHandler ResetFocus;

        // 各種類のパネルインスタンス
        public FolderPanel FolderListPanel { get; private set; }
        public HistoryPanel HistoryPanel { get; private set; }
        public FileInformationPanel FileInfoPanel { get; private set; }
        public ImageEffectPanel ImageEffectPanel { get; private set; }
        public PagemarkPanel PagemarkPanel { get; private set; }

        //
        private Models _models;

        /// <summary>
        /// サイドパネル初期化
        /// TODO: 生成順。モデルはビュー生成の前に準備されているべき
        /// </summary>
        /// <param name="control"></param>
        public SidePanel(Models models)
        {
            Current = this;

            _models = models;

            var leftPanels = new List<IPanel>();
            var rightPanels = new List<IPanel>();

            // フォルダーリスト
            this.FolderListPanel = new FolderPanel(models.FolderPanelModel, models.FolderList, models.PageList);
            leftPanels.Add(this.FolderListPanel);

            // 履歴
            this.HistoryPanel = new HistoryPanel(models.HistoryList);
            leftPanels.Add(this.HistoryPanel);

            // ファイル情報
            this.FileInfoPanel = new FileInformationPanel(models.FileInformation);
            rightPanels.Add(this.FileInfoPanel);

            // エフェクト
            this.ImageEffectPanel = new ImageEffectPanel(models.ImageEffect, models.ImageFilter);
            rightPanels.Add(this.ImageEffectPanel);

            // ページマーク
            this.PagemarkPanel = new PagemarkPanel(models.PagemarkList);
            leftPanels.Add(this.PagemarkPanel);

            // パネル群を登録
            this.InitializePanels(leftPanels, rightPanels);

            //
            SelectedPanelChanged += (s, e) => RaisePanelPropertyChanged();
        }


        /// <summary>
        /// 指定したパネルが表示されているか判定
        /// </summary>
        /// <returns></returns>
        public bool IsVisiblePanel(IPanel panel)
        {
            return this.Left.IsVisiblePanel(panel) || this.Right.IsVisiblePanel(panel);
        }

        /// <summary>
        /// 指定したパネルが選択されているか判定
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public bool IsSelectedPanel(IPanel panel)
        {
            return this.Left.SelectedPanel == panel || this.Right.SelectedPanel == panel;
        }

        /// <summary>
        /// パネル選択状態を設定
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <param name="isSelected">選択</param>
        public void SetSelectedPanel(IPanel panel, bool isSelected)
        {
            if (this.Left.Contains(panel))
            {
                this.Left.SetSelectedPanel(panel, isSelected);
            }
            if (this.Right.Contains(panel))
            {
                this.Right.SetSelectedPanel(panel, isSelected);
            }
        }

        /// <summary>
        /// パネル選択状態をトグル。
        /// 非表示状態の場合は切り替えよりも表示させることを優先する
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <param name="force">表示状態にかかわらず切り替える</param>
        public void ToggleSelectedPanel(IPanel panel, bool force)
        {
            if (this.Left.Contains(panel))
            {
                this.Left.ToggleSelectedPanel(panel, force);
            }
            if (this.Right.Contains(panel))
            {
                this.Right.ToggleSelectedPanel(panel, force);
            }
        }

        /// <summary>
        /// パネル表示トグル
        /// </summary>
        /// <param name="code"></param>
        public void ToggleVisiblePanel(IPanel panel)
        {
            this.Left.Toggle(panel);
            this.Right.Toggle(panel);
        }



        #region Panels Visibility

        //
        private void RaisePanelPropertyChanged()
        {
            RaisePropertyChanged(nameof(IsVisibleFolderList));
            RaisePropertyChanged(nameof(IsVisibleHistoryList));
            RaisePropertyChanged(nameof(IsVisiblePagemarkList));
            RaisePropertyChanged(nameof(IsVisiblePageListMenu));
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
            ResetFocus?.Invoke(this, null);
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
            ResetFocus?.Invoke(this, null);
            return IsVisibleEffectInfo;
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return IsSelectedPanel(FolderListPanel); }
            set { SetSelectedPanel(FolderListPanel, value); RaisePanelPropertyChanged(); }
        }

        //
        public bool ToggleVisibleFolderList(bool byMenu)
        {
            ToggleSelectedPanel(FolderListPanel, byMenu);
            RaisePanelPropertyChanged();
            if (!IsVisibleFolderList)
            {
                ResetFocus?.Invoke(this, null);
            }
            return IsVisibleFolderList;
        }

        //
        public bool IsVisiblePageListMenu => _models.FolderPanelModel.IsPageListVisible && IsVisibleFolderList;

        //
        public bool ToggleVisiblePageList(bool byMenu)
        {
            var model = _models.FolderPanelModel;

            if (byMenu || !model.IsPageListVisible || IsVisiblePanel(FolderListPanel))
            {
                model.IsPageListVisible = !IsVisiblePageListMenu;
            }
            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();

            if (model.IsPageListVisible)
            {
                _models.PageList.FocusAtOnce = true;
            }

            return model.IsPageListVisible;
        }


        //
        public bool FocusBookmarkList(bool byMenu)
        {
            var model = _models.FolderPanelModel;
            if (_models.FolderList.Place.Scheme != QueryScheme.Bookmark)
            {
                _models.FolderList.RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.UpdateHistory);
            }

            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();
            if (!IsVisibleFolderList)
            {
                ResetFocus?.Invoke(this, null);
            }
            return IsVisibleFolderList;
        }


        /// <summary>
        /// 検索ボックス表示状態
        /// </summary>
        public bool IsVisibleFolderSearchBox => _models.FolderList.IsFolderSearchBoxVisible && IsVisibleFolderList;


        //
        public void FocusFolderSearchBox(bool byMenu)
        {
            _models.FolderList.RaiseSearchBoxFocus();
        }

        /// <summary>
        /// フォルダーツリー表示状態
        /// </summary>
        public bool IsVisibleFolderTree => _models.FolderList.IsFolderTreeVisible && IsVisibleFolderList;

        /// <summary>
        /// フォルダーツリー表示状態切替
        /// </summary>
        public bool ToggleVisibleFolderTree(bool byMenu)
        {
            return SetVisibleFolderTree(byMenu, !IsVisibleFolderTree);
        }

        public bool SetVisibleFolderTree(bool byMenu, bool isVisible)
        {
            var model = _models.FolderList;

            if (byMenu || !model.IsFolderTreeVisible || IsVisiblePanel(FolderListPanel))
            {
                model.IsFolderTreeVisible = isVisible;
            }
            SetSelectedPanel(FolderListPanel, true);
            RaisePanelPropertyChanged();

            // フォーカス要求
            if (!byMenu && model.IsFolderTreeVisible)
            {
                model.RaiseFolderTreeFocus();
            }

            return model.IsFolderTreeVisible;
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return IsSelectedPanel(HistoryPanel); }
            set { SetSelectedPanel(HistoryPanel, value); RaisePanelPropertyChanged(); }
        }

        //
        public bool ToggleVisibleHistoryList(bool byMenu)
        {
            ToggleSelectedPanel(HistoryPanel, byMenu);
            RaisePanelPropertyChanged();
            if (!IsVisibleHistoryList)
            {
                ResetFocus?.Invoke(this, null);
            }
            return IsVisibleHistoryList;
        }

        // ページマークリスト表示ON/OFF
        public bool IsVisiblePagemarkList
        {
            get { return IsSelectedPanel(PagemarkPanel); }
            set { SetSelectedPanel(PagemarkPanel, value); RaisePanelPropertyChanged(); }
        }

        //
        public bool ToggleVisiblePagemarkList(bool byMenu)
        {
            ToggleSelectedPanel(PagemarkPanel, byMenu);
            RaisePanelPropertyChanged();
            if (!IsVisiblePagemarkList)
            {
                ResetFocus?.Invoke(this, null);
            }
            return IsVisiblePagemarkList;
        }

        #endregion
    }
}
