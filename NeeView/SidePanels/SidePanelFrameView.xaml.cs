using NeeView.Windows;
using NeeView.Windows.Media;
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
    /// SidePanelFrameView.xaml の相互作用ロジック
    /// </summary>
    public partial class SidePanelFrameView : UserControl, INotifyPropertyChanged
    {
        public static SidePanelFrameView Current { get; private set; }

        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        #region DependencyProperties

        /// <summary>
        /// パネル背景
        /// </summary>
        public Brush PanelBackground
        {
            get { return (Brush)GetValue(PanelBackgroundProperty); }
            set { SetValue(PanelBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PanelBackgroundProperty =
            DependencyProperty.Register("PanelBackground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(Brushes.DarkGray));


        /// <summary>
        /// アイコンリスト背景
        /// </summary>
        public Brush IconBackground
        {
            get { return (Brush)GetValue(IconBackgroundProperty); }
            set { SetValue(IconBackgroundProperty, value); }
        }

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register("IconBackground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(Brushes.Gray));


        /// <summary>
        /// アイコン色
        /// </summary>
        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register("IconForeground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(null));


        /// <summary>
        /// PanelMargin property.
        /// </summary>
        public Thickness PanelMargin
        {
            get { return (Thickness)GetValue(PanelMarginProperty); }
            set { SetValue(PanelMarginProperty, value); }
        }

        public static readonly DependencyProperty PanelMarginProperty =
            DependencyProperty.Register("PanelMargin", typeof(Thickness), typeof(SidePanelFrameView), new PropertyMetadata(null));


        /// <summary>
        /// IsAutoHide property.
        /// </summary>
        public bool IsAutoHide
        {
            get { return (bool)GetValue(IsAutoHideProperty); }
            set { SetValue(IsAutoHideProperty, value); }
        }

        public static readonly DependencyProperty IsAutoHideProperty =
            DependencyProperty.Register("IsAutoHide", typeof(bool), typeof(SidePanelFrameView), new PropertyMetadata(false, IsAutoHide_Changed));

        private static void IsAutoHide_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                control.UpdateAutoHide();
            }
        }

        /// <summary>
        /// マウスによる自動非表示の有効/無効
        /// アドレスバー等の手前のパネルにマウスがあるときは無効にすると言った使い方
        /// </summary>
        public bool IsAutoHideMouseEnabled
        {
            get { return (bool)GetValue(IsAutoHideMouseEnabledProperty); }
            set { SetValue(IsAutoHideMouseEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsAutoHideMouseEnabledProperty =
            DependencyProperty.Register("IsAutoHideMouseEnabled", typeof(bool), typeof(SidePanelFrameView), new PropertyMetadata(true));


        /// <summary>
        /// SidePanelFrameModel を Sourceとして指定する。
        /// 指定することで初めてViewModelが生成される
        /// </summary>
        public SidePanelFrame Source
        {
            get { return (SidePanelFrame)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(SidePanelFrame), typeof(SidePanelFrameView), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                control.InitializeViewModel(control.Source);
            }
        }


        /// <summary>
        /// CanvasWidth property.
        /// </summary>
        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasLeft property.
        /// </summary>
        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        public static readonly DependencyProperty CanvasLeftProperty =
            DependencyProperty.Register("CanvasLeft", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        /// <summary>
        /// CanvasTop property.
        /// </summary>
        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }

        public static readonly DependencyProperty CanvasTopProperty =
            DependencyProperty.Register("CanvasTop", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


        #endregion DependencyProperties


        /// <summary>
        /// コンストラクター
        /// </summary>
        public SidePanelFrameView()
        {
            Current = this;

            InitializeComponent();
            InitializeViewModel(this.Source);

            this.Root.DataContext = this;
        }


        /// <summary>
        /// サイドバーの幅
        /// </summary>
        public double PanelIconGridWidth => 50.0;

        /// <summary>
        /// スプリッターの幅
        /// </summary>
        public double SplitterWidth => 8.0;

        public SidePanelProfile Profile => SidePanelProfile.Current;

        private SidePanelFrameViewModel _vm;
        public SidePanelFrameViewModel VM
        {
            get { return _vm; }
            private set { if (_vm != value) { _vm = value; RaisePropertyChanged(); } }
        }


        private void InitializeViewModel(SidePanelFrame model)
        {
            if (model == null) return;

            var leftPanelViewModel = new LeftPanelViewModel(this.LeftIconList, MainLayoutPanelManager.Current.LeftDock, LeftPanelElementContains);
            var rightPanelViewModel = new RightPanelViewModel(this.RightIconList, MainLayoutPanelManager.Current.RightDock, RightPanelElementContains);
            this.VM = new SidePanelFrameViewModel(model, leftPanelViewModel, rightPanelViewModel);
            this.VM.PanelVisibilityChanged += (s, e) => UpdateCanvas();
            UpdateWidth();
            UpdateAutoHide();
        }

        /// <summary>
        /// 左パネルに含まれる要素判定
        /// </summary>
        private bool LeftPanelElementContains(DependencyObject element)
        {
            return VisualTreeUtility.HasParentElement(element, this.LeftPanel);
        }

        /// <summary>
        /// 右パネルに含まれる要素判定
        /// </summary>
        private bool RightPanelElementContains(DependencyObject element)
        {
            return VisualTreeUtility.HasParentElement(element, this.RightPanel);
        }

        /// <summary>
        /// AutoHide 更新
        /// </summary>
        private void UpdateAutoHide()
        {
            if (_vm == null) return;
            _vm.IsAutoHide = IsAutoHide;
        }

        /// <summary>
        /// 領域サイズ変更イベント処理
        /// </summary>
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWidth();
        }

        /// <summary>
        /// 領域幅更新。パネル幅制限に使用される
        /// </summary>
        private void UpdateWidth()
        {
            if (_vm == null) return;
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
            if (_vm == null || _vm.IsAutoHide)
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

        private void LeftIconGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).Focus();
        }

        private void LeftIconGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _vm.Left.Toggle();
        }

        private void RightIconGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).Focus();
        }

        private void RightIconGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _vm.Right.Toggle();
        }

        private void PanelIconItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DragStartBehavior_DragBegin(object sender, DragStartEventArgs e)
        {
            _vm.DragBegin(sender, e);
        }

        private void DragStartBehavior_DragEnd(object sender, EventArgs e)
        {
            _vm.DragEnd(sender, e);
        }

    }
}
