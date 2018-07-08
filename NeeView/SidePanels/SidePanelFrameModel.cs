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

        private string _fontName = SystemFonts.MessageFontFamily.Source;
        private double _folderTreeFontSize = 12;
        private double _fontSize = 15.0;
        private bool _isTextWrapped;
        private double _noteOpacity = 0.5;

        #endregion

        #region Constructors

        public SidePanelFrameModel()
        {
            // initialize resource parameter
            SetFontFamilyResource(_fontName);
            SetFontSizeResource(_fontSize);
            SetFolderTreeFontSizeResource(_folderTreeFontSize);
            SetIsTextWrappedResource(_isTextWrapped);
            UpdateTextWrappedHeightResource();
            UpdateNoteOpacityResource();

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


        [PropertyMember("@ParamListItemFontName")]
        public string FontName
        {
            get
            {
                return _fontName;
            }
            set
            {
                value = value ?? SystemFonts.MessageFontFamily.Source;
                if (_fontName != value)
                {
                    try
                    {
                        _fontName = value;
                        SetFontFamilyResource(_fontName);
                        UpdateTextWrappedHeightResource();
                        RaisePropertyChanged();
                    }
                    catch
                    {
                        // nop.
                    }
                }
            }
        }


        [PropertyRange("@ParamListItemFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                value = Math.Max(1, value);
                if (_fontSize != value)
                {
                    _fontSize = value;
                    SetFontSizeResource(_fontSize);
                    UpdateTextWrappedHeightResource();
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamListItemFolderTreeFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FolderTreeFontSize
        {
            get { return _folderTreeFontSize; }
            set
            {
                value = Math.Max(1, value);
                if (_folderTreeFontSize != value)
                {
                    _folderTreeFontSize = value;
                    SetFolderTreeFontSizeResource(_folderTreeFontSize);
                    RaisePropertyChanged();
                }
            }
        }


        [PropertyMember("@ParamListItemIsTextWrapped", Tips = "@ParamListItemIsTextWrappedTips")]
        public bool IsTextWrapped
        {
            get { return _isTextWrapped; }
            set
            {
                if (_isTextWrapped != value)
                {
                    _isTextWrapped = value;
                    SetIsTextWrappedResource(_isTextWrapped);
                    UpdateTextWrappedHeightResource();
                    RaisePropertyChanged();
                }
            }
        }

        [PropertyRange("@ParamListItemNoteOpacity", 0.0, 1.0, Tips = "@ParamListItemNoteOpacityTips")]
        public double NoteOpacity
        {
            get { return _noteOpacity; }
            set
            {
                if (_noteOpacity != value)
                {
                    _noteOpacity = value;
                    UpdateNoteOpacityResource();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        // リソースにFontFamily適用
        private void SetFontFamilyResource(string fontName)
        {
            var fontFamily = fontName != null ? new FontFamily(fontName) : SystemFonts.MessageFontFamily;
            App.Current.Resources["PanelFontFamily"] = fontFamily;
        }

        // リソースにFontSize適用
        private void SetFontSizeResource(double fontSize)
        {
            App.Current.Resources["PanelFontSize"] = fontSize;
        }

        // リソースにFolderTreeFontSize適用
        private void SetFolderTreeFontSizeResource(double fontSize)
        {
            App.Current.Resources["FolderTreeFontSize"] = fontSize;
        }

        // リソースにTextWrapping適用
        private void SetIsTextWrappedResource(bool isWrapped)
        {
            App.Current.Resources["PanelTextWrapping"] = isWrapped ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        // リソースにNoteOpacity適用
        private void UpdateNoteOpacityResource()
        {
            App.Current.Resources["PanelNoteOpacity"] = _noteOpacity;
            App.Current.Resources["PanelNoteVisibility"] = _noteOpacity <= 0.0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // calc 2 line textbox height
        private void UpdateTextWrappedHeightResource()
        {
            if (_isTextWrapped)
            {
                // 実際にTextBlockを作成して計算する
                var textBlock = new TextBlock()
                {
                    Text = "AAA\nBBB",
                    FontSize = this.FontSize,
                };
                if (_fontName != null)
                {
                    textBlock.FontFamily = new FontFamily(_fontName);
                };
                var panel = new StackPanel();
                panel.Children.Add(textBlock);
                var area = new Size(256, 256);
                panel.Measure(area);
                panel.Arrange(new Rect(area));
                //panel.UpdateLayout();
                double height = (int)textBlock.ActualHeight + 1.0;

                App.Current.Resources["PanelTextWrappedHeight"] = height;
            }
            else
            {
                App.Current.Resources["PanelTextWrappedHeight"] = double.NaN;
            }
        }


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
            [DataMember, DefaultValue(true)]
            public bool IsSideBarVisible { get; set; }

            [DataMember]
            public bool IsManipulationBoundaryFeedbackEnabled { get; set; }

            [DataMember]
            public string FontName { get; set; }

            [DataMember, DefaultValue(15)]
            public double FontSize { get; set; }


            [DataMember, DefaultValue(12)]
            public double FolderTreeFontSize { get; set; }

            [DataMember]
            public bool IsTextWrapped { get; set; }

            [DataMember, DefaultValue(0.5)]
            public double NoteOpacity { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Left { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Right { get; set; }


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
            memento.FontName = this.FontName;
            memento.FontSize = this.FontSize;
            memento.FolderTreeFontSize = this.FolderTreeFontSize;
            memento.IsTextWrapped = this.IsTextWrapped;
            memento.NoteOpacity = this.NoteOpacity;
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
            this.FontName = memento.FontName;
            this.FontSize = memento.FontSize;
            this.FolderTreeFontSize = memento.FolderTreeFontSize;
            this.IsTextWrapped = memento.IsTextWrapped;
            this.NoteOpacity = memento.NoteOpacity;
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
