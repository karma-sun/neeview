// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// NeeView用 サイドパネル管理
    /// </summary>
    public class SidePanels : SidePanelFrameModel
    {
        private List<IPanel> _panels;

        // 各種類のパネルインスタンス
        public FolderListPanel FolderListPanel { get; private set; }
        public HistoryPanel HistoryPanel { get; private set; }
        public FileInfoPanel FileInfoPanel { get; private set; }
        public ImageEffectPanel ImageEffectPanel { get; private set; }
        public BookmarkPanel BookmarkPanel { get; private set; }
        public PagemarkPanel PagemarkPanel { get; private set; }


        /// <summary>
        /// サイドパネル初期化
        /// TODO: 生成順。モデルはビュー生成の前に準備されているべき
        /// </summary>
        /// <param name="control"></param>
        public SidePanels()
        {
            _panels = new List<IPanel>();

            // フォルダーリスト
            this.FolderListPanel = new FolderListPanel();
            _panels.Add(this.FolderListPanel);
            //this.PageList.Initialize(this);

            // 履歴
            this.HistoryPanel = new HistoryPanel();
            _panels.Add(this.HistoryPanel);

            // ファイル情報
            this.FileInfoPanel = new FileInfoPanel();
            _panels.Add(this.FileInfoPanel);

            // エフェクト
            this.ImageEffectPanel = new ImageEffectPanel();
            _panels.Add(this.ImageEffectPanel);

            // ブックマーク
            this.BookmarkPanel = new BookmarkPanel();
            _panels.Add(this.BookmarkPanel);

            // ページマーク
            this.PagemarkPanel = new PagemarkPanel();
            _panels.Add(this.PagemarkPanel);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="vm"></param>
        public void Initialize(MainWindowVM vm)
        {
            // フォルダーリスト
            this.FolderListPanel.Initialize(vm);
            this.FolderListPanel.SetPlace(ModelContext.BookHistory.LastFolder ?? vm.BookHub.GetFixedHome(), null, false); // ##

            // 履歴
            this.HistoryPanel.Initialize(vm);

            // ファイル情報
            this.FileInfoPanel.Initialize(vm);

            // エフェクト
            this.ImageEffectPanel.Initialize(vm);

            // ブックマーク
            this.BookmarkPanel.Initialize(vm);

            // ページマーク
            this.PagemarkPanel.Initialize(vm);
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


        #region Memento

        /// <summary>
        /// 標準Memento生成
        /// </summary>
        /// <returns></returns>
        public Memento CreateDefaultMemento()
        {
            var memento = new Memento();
            memento.Right.PanelTypeCodes = new List<string>() { nameof(FileInfoPanel), nameof(ImageEffectPanel) };
            return memento;
        }

        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            this.Restore(memento, _panels);
        }

        #endregion
    }
}
