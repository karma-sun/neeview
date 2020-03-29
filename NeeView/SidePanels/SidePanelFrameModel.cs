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

        private bool _isVisibleLocked;
        private SidePanelGroup _left;
        private SidePanelGroup _right;
        private List<IPanel> _panels;
        private bool _isFlushPanelsConfigEnabled = true;

        #endregion

        #region Constructors

        public SidePanelFrameModel()
        {
            _left = new SidePanelGroup();
            _left.SelectedPanelChanged += (s, e) => SelectedPanelChanged?.Invoke(s, e);

            _right = new SidePanelGroup();
            _right.SelectedPanelChanged += (s, e) => SelectedPanelChanged?.Invoke(s, e);
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

        // サイドバー表示ロック。自動非表示にならないようにする
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }

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
            _panels = leftPanels.Concat(rightPanels).ToList();

            InitializePanelsInner();

            // Configに反映
            _left.Panels.CollectionChanged +=
                (s, e) => FlushPanelsConfig();

            _left.AddPropertyChanged(nameof(SidePanelGroup.SelectedPanel),
                (s, e) => Config.Current.Panels.LeftPanelSeleted = _left.SelectedPanel?.TypeCode);

            _left.AddPropertyChanged(nameof(SidePanelGroup.Width),
                (s, e) => Config.Current.Panels.LeftPanelWidth = _left.Width);

            _right.Panels.CollectionChanged +=
                (s, e) => FlushPanelsConfig();

            _right.AddPropertyChanged(nameof(SidePanelGroup.SelectedPanel),
                (s, e) => Config.Current.Panels.RightPanelSeleted = _right.SelectedPanel?.TypeCode);

            _right.AddPropertyChanged(nameof(SidePanelGroup.Width),
                (s, e) => Config.Current.Panels.RightPanelWidth = _right.Width);

            FlushPanelsConfig();

            // Configを監視
            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.PanelDocks), PanelsConfig_PanelDocksChanged);

            // 情報更新
            SelectedPanelChanged?.Invoke(this, null);
        }

        private void InitializePanelsInner()
        {
            var lefts = new List<IPanel>();
            var rights = new List<IPanel>();

            if (Config.Current.Panels.PanelDocks.Any())
            {
                lefts = Config.Current.Panels.PanelDocks
                    .Where(e => e.Value == PanelDock.Left)
                    .Select(e => _panels.FirstOrDefault(panel => panel.TypeCode == e.Key))
                    .Where(e => e != null)
                    .ToList();

                rights = Config.Current.Panels.PanelDocks
                    .Where(e => e.Value == PanelDock.Right)
                    .Select(e => _panels.FirstOrDefault(panel => panel.TypeCode == e.Key))
                    .Where(e => e != null)
                    .ToList();

                var rests = _panels.Except(lefts).Except(rights).ToList();

                // 未登録パネルを既定パネルに登録
                lefts = lefts.Concat(rests.Where(e => e.DefaultPlace == PanelPlace.Left)).ToList();
                rights = rights.Concat(rests.Where(e => e.DefaultPlace == PanelPlace.Right)).ToList();
            }
            else
            {
                lefts = _panels.Where(e => e.DefaultPlace == PanelPlace.Left).ToList();
                rights = _panels.Where(e => e.DefaultPlace == PanelPlace.Right).ToList();
            }

            _left.Initialize(lefts, Config.Current.Panels.LeftPanelSeleted, Config.Current.Panels.LeftPanelWidth);
            _right.Initialize(rights, Config.Current.Panels.RightPanelSeleted, Config.Current.Panels.RightPanelWidth);
        }

        private void PanelsConfig_PanelDocksChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // NOTE: 更新を抑制。あとでまとめて更新する
                _isFlushPanelsConfigEnabled = false;
                InitializePanelsInner();
            }
            finally
            {
                _isFlushPanelsConfigEnabled = true;
            }

            FlushPanelsConfig();
        }


        public void FlushPanelsConfig()
        {
            if (!_isFlushPanelsConfigEnabled) return;

            var dictionary = Config.Current.Panels.PanelDocks;

            dictionary.Clear();
            foreach (var panel in _left.Panels)
            {
                dictionary.Add(panel.TypeCode, PanelDock.Left);
            }
            foreach (var panel in _right.Panels)
            {
                dictionary.Add(panel.TypeCode, PanelDock.Right);
            }

            Config.Current.Panels.LeftPanelSeleted = _left.SelectedPanel?.TypeCode;
            Config.Current.Panels.RightPanelSeleted = _right.SelectedPanel?.TypeCode;
            Config.Current.Panels.LeftPanelWidth = _left.Width;
            Config.Current.Panels.RightPanelWidth = _right.Width;
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
            if (!Config.Current.Panels.IsManipulationBoundaryFeedbackEnabled)
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
                config.Panels.LeftPanelWidth = Left.Width;
                config.Panels.RightPanelSeleted = Right.SelectedPanelTypeCode;
                config.Panels.RightPanelWidth = Right.Width;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSideBarVisible = Config.Current.Panels.IsSideBarEnabled;
            memento.IsManipulationBoundaryFeedbackEnabled = Config.Current.Panels.IsManipulationBoundaryFeedbackEnabled;
            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();

            return memento;
        }

        #endregion
    }
}
