// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using NeeView.Windows;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame ViewModel
    /// </summary>
    public class SidePanelFrameViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PanelVisibility Changed
        /// </summary>
        public event EventHandler PanelVisibilityChanged;

        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// IsSideVarVisible property.
        /// </summary>
        public bool IsSideBarVisible
        {
            get { return _model != null ? _model.IsSideBarVisible : true; }
            set { if (_model.IsSideBarVisible != value) { _model.IsSideBarVisible = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Width property.
        /// </summary>
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

        //
        private double _width;

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


        /// <summary>
        /// IsAutoHide property.
        /// </summary>
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

        private bool _isAutoHide;




        /// <summary>
        /// Model property.
        /// </summary>
        public SidePanelFrameModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelFrameModel _model;


        /// <summary>
        /// LeftPanel
        /// </summary>
        public LeftPanelViewModel Left { get; private set; }

        /// <summary>
        /// RightPanel
        /// </summary>
        public RightPanelViewModel Right { get; private set; }

        /// <summary>
        /// ドラッグ開始設定
        /// </summary>
        public DragStartDescription DragStartDescription { get; private set; }


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="model"></param>
        public SidePanelFrameViewModel(SidePanelFrameModel model, ItemsControl leftItemsControl, ItemsControl rightItemsControl)
        {
            if (model == null) return;

            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;
            _model.ContentChanged  += (s, e) => ResetDelayHide();

            Left = new LeftPanelViewModel(_model.Left, leftItemsControl);
            Left.PropertyChanged += Left_PropertyChanged;
            Left.PanelDroped += Left_PanelDroped;

            Right = new RightPanelViewModel(_model.Right, rightItemsControl);
            Right.PropertyChanged += Right_PropertyChanged;
            Right.PanelDroped += Right_PanelDroped;

            DragStartDescription = new DragStartDescription();
            DragStartDescription.DragStart += DragStartDescription_DragStart;
            DragStartDescription.DragEnd += DragStartDescription_DragEnd;
        }

        /// <summary>
        /// モデルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.IsSideBarVisible):
                    RaisePropertyChanged(nameof(IsSideBarVisible));
                    break;
                case nameof(Model.IsVisibleLocked):
                    this.Left.IsVisibleLocked = Model.IsVisibleLocked;
                    this.Right.IsVisibleLocked = Model.IsVisibleLocked;
                    break;
            }
        }

        /// <summary>
        /// ドラッグ開始イベント処理.
        /// 強制的にパネル表示させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragStartDescription_DragStart(object sender, EventArgs e)
        {
            Left.IsDragged = true;
            Right.IsDragged = true;
        }

        /// <summary>
        /// ドラッグ終了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragStartDescription_DragEnd(object sender, EventArgs e)
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 表示状態更新.
        /// 自動表示/非表示の処理
        /// </summary>
        /// <param name="point">カーソル位置</param>
        /// <param name="left">左パネル右端</param>
        /// <param name="right">右パネル左端</param>
        internal void UpdateVisibility(Point point, Point left, Point right)
        {
            Left?.UpdateVisibility(point, left);
            Right?.UpdateVisibility(point, right);
        }


        /// <summary>
        /// 自動非表示時間リセット
        /// </summary>
        private void ResetDelayHide()
        {
            this.Left?.ResetDelayHide();
            this.Right?.ResetDelayHide();
        }

    }
}
