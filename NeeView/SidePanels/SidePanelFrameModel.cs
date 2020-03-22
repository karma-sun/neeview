using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame Model
    /// 左右のパネルを管理
    /// </summary>
    public class SidePanelFrameModel : BindableBase
    {
        #region Fields

        private bool _IsSideBarVisible = true;
        private bool _isVisibleLocked;
        private SidePanelGroup _left;
        private SidePanelGroup _right;

        #endregion

        #region Constructors

        public SidePanelFrameModel()
        {
            _left = new SidePanelGroup();
            _left.SelectedPanelChanged += (s, e) => SelectedPanelChanged(s, e);

            _right = new SidePanelGroup();
            _right.SelectedPanelChanged += (s, e) => SelectedPanelChanged(s, e);
        }

        #endregion

        #region Events

        /// <summary>
        /// パネル選択変更イベント.
        /// 非表示状態のパネルを表示させるために使用される.
        /// </summary>
        public event EventHandler<SelectedPanelChangedEventArgs> SelectedPanelChanged;

        /// <summary>
        /// パネル内容更新イベント.
        /// 自動非表示時間のリセットに使用される.
        /// </summary>
        public event EventHandler ContentChanged;

        #endregion

        #region Properties

        // サイドバー表示フラグ
        public bool IsSideBarVisible
        {
            get { return _IsSideBarVisible; }
            set { if (_IsSideBarVisible != value) { _IsSideBarVisible = value; RaisePropertyChanged(); } }
        }

        // サイドバー表示ロック。自動非表示にならないようにする
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }


        [PropertyMember("@ParamSidePanelIsManipulationBoundaryFeedbackEnabled")]
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; }

        // Left Panel
        public SidePanelGroup Left
        {
            get { return _left; }
            set { if (_left != value) { _left = value; RaisePropertyChanged(); } }
        }

        // Right Panel
        public SidePanelGroup Right
        {
            get { return _right; }
            set { if (_right != value) { _right = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        /// <summary>
        /// パネル登録
        /// </summary>
        /// <param name="leftPanels"></param>
        /// <param name="rightPanels"></param>
        public void InitializePanels(List<IPanel> leftPanels, List<IPanel> rightPanels)
        {
            leftPanels.ForEach(e => _left.Panels.Add(e));
            rightPanels.ForEach(e => _right.Panels.Add(e));
        }

        /// <summary>
        /// パネルの追加
        /// </summary>
        public void Attach(IPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            if (this.Left.Contains(panel) || this.Right.Contains(panel))
            {
                return;
            }

            if (panel.DefaultPlace == PanelPlace.Left)
            {
                this.Left.Panels.Add(panel);
            }
            else
            {
                this.Right.Panels.Add(panel);
            }
        }

        /// <summary>
        /// パネルの削除
        /// </summary>
        public void Detach(IPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            if (!this.Left.Contains(panel) && !this.Right.Contains(panel))
            {
                return;
            }

            SetSelectedPanel(panel, false);

            if (this.Left.Contains(panel))
            {
                this.Left.Panels.Remove(panel);
            }
            else
            {
                this.Right.Panels.Remove(panel);
            }
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
            if (panel == null)
            {
                return;
            }

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

            if (IsSelectedPanel(panel))
            {
                panel.Focus();
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


        /// <summary>
        /// コンテンツ変更通知
        /// </summary>
        public void RaiseContentChanged()
        {
            ContentChanged?.Invoke(this, null);
        }


        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!this.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }

        public void Refresh()
        {
            this.Left.Refresh();
            this.Right.Refresh();
        }

        public void CloseAllPanels()
        {
            this.Left.ToggleSelectedPanel(null, true);
            this.Right.ToggleSelectedPanel(null, true);
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
                // TODO: Left, Right の処理は？
                // Left.Restore(Config.Current.Layout.SidePanel.Left)
                // Right.Restore(Config.Current.Layout.SidePanel.Right) とか
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSideBarVisible = this.IsSideBarVisible;
            memento.IsManipulationBoundaryFeedbackEnabled = this.IsManipulationBoundaryFeedbackEnabled;
            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // パネル収集
            var panels = _left.Panels.Concat(_right.Panels).ToList();
            _left.Panels.Clear();
            _right.Panels.Clear();

            // memento反映
            this.IsSideBarVisible = memento.IsSideBarVisible;
            this.IsManipulationBoundaryFeedbackEnabled = memento.IsManipulationBoundaryFeedbackEnabled;
            _left.Restore(memento.Left, panels);
            _right.Restore(memento.Right, panels);

            // 未登録パネルを既定パネルに登録
            foreach (var panel in panels.Where(e => !_left.Panels.Contains(e) && !_right.Panels.Contains(e)))
            {
                (panel.DefaultPlace == PanelPlace.Right ? _right : _left).Panels.Add(panel);
            }

            // 情報更新
            SelectedPanelChanged?.Invoke(this, null);
        }

        #endregion
    }
}
