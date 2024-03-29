﻿using System;
using System.ComponentModel;
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
        private double _width;
        private bool _isAutoHide;
        private SidePanelFrame _model;


        public SidePanelFrameViewModel(SidePanelFrame model, LeftPanelViewModel left, RightPanelViewModel right)
        {
            if (model == null) return;

            _model = model;
            _model.VisibleAtOnceRequest += Model_VisibleAtOnceRequest;

            MainLayoutPanelManager = CustomLayoutPanelManager.Current;

            Left = left;
            Left.PropertyChanged += Left_PropertyChanged;

            Right = right;
            Right.PropertyChanged += Right_PropertyChanged;

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePanel),
                (s, e) => RaisePropertyChanged(nameof(Opacity)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity),
                (s, e) => RaisePropertyChanged(nameof(Opacity)));

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsSideBarEnabled),
                (s, e) => RaisePropertyChanged(nameof(IsSideBarVisible)));

            MainLayoutPanelManager.DragBegin +=
                (s, e) => DragBegin(this, null);
            MainLayoutPanelManager.DragEnd +=
                (s, e) => DragEnd(this, null);

            SidePanelIconDescriptor = new SidePanelIconDescriptor(this);
        }


        public event EventHandler PanelVisibilityChanged;


        public SidePanelIconDescriptor SidePanelIconDescriptor { get; }

        public bool IsSideBarVisible
        {
            get => Config.Current.Panels.IsSideBarEnabled;
            set => Config.Current.Panels.IsSideBarEnabled = value;
        }

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


        public double Opacity
        {
            get => MainWindowModel.Current.CanHidePanel ? Config.Current.Panels.Opacity : 1.0;
        }




        /// <summary>
        /// パネル表示リクエスト
        /// </summary>
        private void Model_VisibleAtOnceRequest(object sender, VisibleAtOnceRequestEventArgs e)
        {
            VisibleAtOnce(e.Key);
        }

        /// <summary>
        /// パネルを一度だけ表示
        /// </summary>
        public void VisibleAtOnce(string key)
        {
            if (Left.SelectedItemContains(key))
            {
                Left.VisibleOnce();
            }
            else if (Right.SelectedItemContains(key))
            {
                Right.VisibleOnce();
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

        public SidePanelFrame Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public SidePanelViewModel Left { get; private set; }

        public SidePanelViewModel Right { get; private set; }

        public App App => App.Current;

        public AutoHideConfig AutoHideConfig => Config.Current.AutoHide;



        public CustomLayoutPanelManager MainLayoutPanelManager { get; private set; }

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
