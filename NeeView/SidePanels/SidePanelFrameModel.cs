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
            _left.PropertyChanged += Left_PropertyChanged;

            _right = new SidePanelGroup();
            _right.PropertyChanged += Right_PropertyChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// パネル選択変更イベント.
        /// 非表示状態のパネルを表示させるために使用される.
        /// </summary>
        public event EventHandler SelectedPanelChanged;


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
        /// コンテンツ変更通知
        /// </summary>
        public void RaiseContentChanged()
        {
            ContentChanged?.Invoke(this, null);
        }


        /// <summary>
        /// 右パネルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Right, null);
                    break;
            }
        }

        /// <summary>
        /// 左パネルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Left, null);
                    break;
            }
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

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsSideBarVisible { get; set; }

            [DataMember]
            public bool IsManipulationBoundaryFeedbackEnabled { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Left { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Right { get; set; }


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


            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        /// <summary>
        /// Memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSideBarVisible = this.IsSideBarVisible;
            memento.IsManipulationBoundaryFeedbackEnabled = this.IsManipulationBoundaryFeedbackEnabled;
            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();

            return memento;
        }


        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
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

#pragma warning disable CS0612

        public void RestoreCompatible(Memento memento)
        {
            if (memento == null) return;

            // compatible before ver.32
            if (memento._Version < Config.GenerateProductVersionNumber(32, 0, 0))
            {
                SidePanelProfile.Current.FontName = memento.FontName;
                SidePanelProfile.Current.FontSize = memento.FontSize > 0.0 ? memento.FontSize : 15.0;
                SidePanelProfile.Current.FolderTreeFontSize = memento.FolderTreeFontSize > 0.0 ? memento.FolderTreeFontSize : 12.0;
                SidePanelProfile.Current.ContentItemIsTextWrapped = memento.IsTextWrapped;
                SidePanelProfile.Current.ContentItemNoteOpacity = memento.NoteOpacity;
                SidePanelProfile.Current.BannerItemIsTextWrapped = memento.IsTextWrapped;
                SidePanelProfile.Current.ThumbnailItemIsTextWrapped = memento.IsTextWrapped;

                SidePanelProfile.Current.ValidatePanelListItemProfile();
            }
        }

#pragma warning restore CS0612

        #endregion
    }
}
