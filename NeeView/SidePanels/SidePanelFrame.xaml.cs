﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// SidePanel.xaml の相互作用ロジック
    /// </summary>
    public partial class SidePanelFrame : UserControl, INotifyPropertyChanged
    {
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
        /// reflesh properties
        /// </summary>
        public void Reflesh()
        {
            RaisePropertyChanged(null);
        }

        /// <summary>
        /// サイドバーの幅
        /// </summary>
        public double PanelIconGridWidth => 50.0;

        /// <summary>
        /// スプリッターの幅
        /// </summary>
        public double SplitterWidth => 6.0;


        /// <summary>
        /// パネル背景
        /// </summary>
        public Brush PanelBackground
        {
            get { return (Brush)GetValue(PanelBackgroundProperty); }
            set { SetValue(PanelBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanelBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelBackgroundProperty =
            DependencyProperty.Register("PanelBackground", typeof(Brush), typeof(SidePanelFrame), new PropertyMetadata(Brushes.DarkGray));


        /// <summary>
        /// アイコンリスト背景
        /// </summary>
        public Brush IconBackground
        {
            get { return (Brush)GetValue(IconBackgroundProperty); }
            set { SetValue(IconBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register("IconBackground", typeof(Brush), typeof(SidePanelFrame), new PropertyMetadata(Brushes.Gray));


        /// <summary>
        /// アイコン色
        /// </summary>
        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register("IconForeground", typeof(Brush), typeof(SidePanelFrame), new PropertyMetadata(null));


        /// <summary>
        /// PanelMargin property.
        /// </summary>
        public Thickness PanelMargin
        {
            get { return (Thickness)GetValue(PanelMarginProperty); }
            set { SetValue(PanelMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanelMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelMarginProperty =
            DependencyProperty.Register("PanelMargin", typeof(Thickness), typeof(SidePanelFrame), new PropertyMetadata(null));



        /// <summary>
        /// IsAutoHide property.
        /// </summary>
        public bool IsAutoHide
        {
            get { return (bool)GetValue(IsAutoHideProperty); }
            set { SetValue(IsAutoHideProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAutoHide.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAutoHideProperty =
            DependencyProperty.Register("IsAutoHide", typeof(bool), typeof(SidePanelFrame), new PropertyMetadata(false, IsAutoHide_Changed));

        //
        private static void IsAutoHide_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrame control)
            {
                control.UpdateAutoHide();
            }
        }


        /// <summary>
        /// このコントロールからマウス移動イベントを取得する
        /// </summary>
        public FrameworkElement MouseTarget
        {
            get { return (FrameworkElement)GetValue(MouseTargetProperty); }
            set { SetValue(MouseTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseTargetProperty =
            DependencyProperty.Register("MouseTarget", typeof(FrameworkElement), typeof(SidePanelFrame), new PropertyMetadata(null, MouseTargetPropertyChanged));

        //
        private static void MouseTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrame control)
            {
                if (control.MouseTarget != null)
                {
                    control.MouseTarget.MouseMove += control.Target_MouseMove;
                }
            }
        }

        /// <summary>
        /// Model property.
        /// TODO: このようにモデルをあとから設定するのはおかしい
        /// </summary>
        public SidePanelFrameModel Model
        {
            get { return (SidePanelFrameModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(SidePanelFrameModel), typeof(SidePanelFrame), new PropertyMetadata(null, ModelPropertyChanged));

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrame control)
            {
                control.InitializeViewModel(control.Model);
            }
        }

        /// <summary>
        /// VM property.
        /// </summary>
        public SidePanelFrameViewModel VM
        {
            get { return _vm; }
            private set { if (_vm != value) { _vm = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelFrameViewModel _vm;

        //
        private void InitializeViewModel(SidePanelFrameModel model)
        {
            VM = new SidePanelFrameViewModel(model, this.LeftIconList, this.RightIconList);
            if (VM.IsValid)
            {
                VM.PanelVisibilityChanged += (s, e) => UpdateCanvas();
                UpdateWidth();
                UpdateAutoHide();
            }
        }


        /// <summary>
        /// AutoHide 状態更新
        /// </summary>
        private void UpdateAutoHide()
        {
            if (VM.IsValid)
            {
                VM.IsAutoHide = IsAutoHide;
            }
        }


        /// <summary>
        /// コンストラクター
        /// </summary>
        public SidePanelFrame()
        {
            InitializeComponent();
            InitializeViewModel(this.Model);

            this.Root.DataContext = this;
        }



        /// <summary>
        /// マウスカーソル移動イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this.Root);
            var left = this.Viewport.TranslatePoint(new Point(0, 0), this.Root);
            var right = this.Viewport.TranslatePoint(new Point(this.Viewport.ActualWidth, 0), this.Root);

            _vm.UpdateVisibility(point, left, right);
        }



        /// <summary>
        /// 表示コンテンツ (未使用)
        /// </summary>
        public FrameworkElement ViewContent
        {
            get { return (FrameworkElement)GetValue(ViewContentProperty); }
            set { SetValue(ViewContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewContentProperty =
            DependencyProperty.Register("ViewContent", typeof(FrameworkElement), typeof(SidePanelFrame), new PropertyMetadata(null));



        /// <summary>
        /// CanvasWidth property.
        /// </summary>
        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanvasWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(SidePanelFrame), new PropertyMetadata(0.0));



        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanvasHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(SidePanelFrame), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasLeft property.
        /// </summary>
        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanvasLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasLeftProperty =
            DependencyProperty.Register("CanvasLeft", typeof(double), typeof(SidePanelFrame), new PropertyMetadata(0.0));



        /// <summary>
        /// CanvasTop property.
        /// </summary>
        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanvasTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasTopProperty =
            DependencyProperty.Register("CanvasTop", typeof(double), typeof(SidePanelFrame), new PropertyMetadata(0.0));


        /// <summary>
        /// 領域サイズ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWidth();
        }

        /// <summary>
        /// 領域幅更新。パネル幅制限に使用される
        /// </summary>
        private void UpdateWidth()
        {
            _vm.Width = Math.Max(this.Root.ActualWidth - (PanelIconGridWidth + SplitterWidth) * 2, 0);
            UpdateCanvas();
        }

        /// <summary>
        /// パネルコンテンツサイズ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvas();
        }

        /// <summary>
        /// コンテンツ表示領域サイズ更新
        /// </summary>
        private void UpdateCanvas()
        {
            if (!this.VM.IsValid || this.VM.IsAutoHide)
            {
                CanvasLeft = 0;
                CanvasTop = 0;
                CanvasWidth = this.Root.ActualWidth;
                CanvasHeight = this.Root.ActualHeight;
            }
            else
            {
                var point0 = this.Viewport.TranslatePoint(new Point(0, 0), this.Root);
                var point1 = this.Viewport.TranslatePoint(new Point(this.Viewport.ActualWidth, this.Viewport.ActualHeight), this.Root);

                var rect = new Rect(point0, point1);

                CanvasLeft = rect.Left;
                CanvasTop = rect.Top;
                CanvasWidth = rect.Width;
                CanvasHeight = rect.Height;
            }
        }
    }
}