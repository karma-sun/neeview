using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame ViewModel
    /// </summary>
    public class SidePanelFrameViewModel : BindableBase
    {
        public SidePanelFrameViewModel(SidePanelFrameModel model, ItemsControl leftItemsControl, ItemsControl rightItemsControl)
        {
            if (model == null) return;

            _model = model;

            Left = new SidePanelViewModel(_model.Left, leftItemsControl);
            Left.PropertyChanged += Left_PropertyChanged;
            Left.PanelDroped += Left_PanelDroped;

            Right = new SidePanelViewModel(_model.Right, rightItemsControl);
            Right.PropertyChanged += Right_PropertyChanged;
            Right.PanelDroped += Right_PanelDroped;

            Config.Current.Layout.Panels.AddPropertyChanged(nameof(PanelsConfig.IsSideBarEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsSideBarVisible));
            });
        }
        

        public event EventHandler PanelVisibilityChanged;


        public bool IsSideBarVisible
        {
            get => Config.Current.Layout.Panels.IsSideBarEnabled;
            set => Config.Current.Layout.Panels.IsSideBarEnabled = value;
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = Math.Max(value, Left.Width + Right.Width);
                    UpdateLeftMaxWidth();
                    UpdateRightMaxWidth();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 左パネルの最大幅更新
        /// </summary>
        private void UpdateLeftMaxWidth()
        {
            Left.MaxWidth = _width - Right.Width;
        }

        /// <summary>
        /// 右パネルの最大幅更新
        /// </summary>
        private void UpdateRightMaxWidth()
        {
            Right.MaxWidth = _width - Left.Width;
        }
        
        private bool _isAutoHide;
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set
            {
                if (_isAutoHide != value)
                {
                    _isAutoHide = value;
                    this.Left.IsAutoHide = value;
                    this.Right.IsAutoHide = value;
                    RaisePropertyChanged();
                    PanelVisibilityChanged?.Invoke(this, null);
                }
            }
        }

        private SidePanelFrameModel _model;
        public SidePanelFrameModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public SidePanelViewModel Left { get; private set; }

        public SidePanelViewModel Right { get; private set; }

        public App App => App.Current;

        public AutoHideConfig AutoHideConfig => Config.Current.Layout.AutoHide;



        /// <summary>
        /// ドラッグ開始イベント処理.
        /// 強制的にパネル表示させる
        /// </summary>
        public void DragBegin(object sender, DragStartEventArgs e)
        {
            Left.IsDragged = true;
            Right.IsDragged = true;
        }

        /// <summary>
        /// ドラッグ終了イベント処理
        /// </summary>
        public void DragEnd(object sender, EventArgs e)
        {
            Left.IsDragged = false;
            Right.IsDragged = false;
        }

        /// <summary>
        /// 左パネルへのパネル移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_PanelDroped(object sender, PanelDropedEventArgs e)
        {
            if (Right.Panel.Panels.Contains(e.Panel))
            {
                Right.Panel.Remove(e.Panel);
            }

            Left.Panel.Add(e.Panel, e.Index);
        }

        /// <summary>
        /// 右パネルへのパネル移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_PanelDroped(object sender, PanelDropedEventArgs e)
        {
            if (Left.Panel.Panels.Contains(e.Panel))
            {
                Left.Panel.Remove(e.Panel);
            }

            Right.Panel.Add(e.Panel, e.Index);
        }

        /// <summary>
        /// 右パネルのプロパティ変更イベント処理
        /// </summary>
        private void Right_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.Width):
                    UpdateLeftMaxWidth();
                    break;
                case nameof(Right.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, null);
                    break;
            }
        }

        /// <summary>
        /// 左パネルのプロパティ変更イベント処理
        /// </summary>
        private void Left_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.Width):
                    UpdateRightMaxWidth();
                    break;
                case nameof(Left.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, null);
                    break;
            }
        }
    }
}
