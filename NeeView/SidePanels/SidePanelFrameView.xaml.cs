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
        public double SplitterWidth => 8.0;


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
            DependencyProperty.Register("PanelBackground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(Brushes.DarkGray));


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
            DependencyProperty.Register("IconBackground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(Brushes.Gray));


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
            DependencyProperty.Register("IconForeground", typeof(Brush), typeof(SidePanelFrameView), new PropertyMetadata(null));


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
            DependencyProperty.Register("PanelMargin", typeof(Thickness), typeof(SidePanelFrameView), new PropertyMetadata(null));



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
            DependencyProperty.Register("IsAutoHide", typeof(bool), typeof(SidePanelFrameView), new PropertyMetadata(false, IsAutoHide_Changed));

        //
        private static void IsAutoHide_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
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
            DependencyProperty.Register("MouseTarget", typeof(FrameworkElement), typeof(SidePanelFrameView), new PropertyMetadata(null, MouseTargetPropertyChanged));

        //
        private static void MouseTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                if (control.MouseTarget != null)
                {
                    control.MouseTarget.MouseMove += control.Target_MouseMove;
                }
            }
        }

        /// <summary>
        /// SidePanelFrameModel を Sourceとして指定する。
        /// 指定することで初めてViewModelが生成される
        /// </summary>
        public SidePanelFrameModel Source
        {
            get { return (SidePanelFrameModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(SidePanelFrameModel), typeof(SidePanelFrameView), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelFrameView control)
            {
                control.InitializeViewModel(control.Source);
            }
        }


        //
        public SidePanelProfile Profile => SidePanelProfile.Current;


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
            if (model == null) return;

            this.VM = new SidePanelFrameViewModel(model, this.LeftIconList, this.RightIconList);
            this.VM.PanelVisibilityChanged += (s, e) => UpdateCanvas();
            UpdateWidth();
            UpdateAutoHide();
        }


        /// <summary>
        /// AutoHide 状態更新
        /// </summary>
        private void UpdateAutoHide()
        {
            if (_vm == null) return;
            _vm.IsAutoHide = IsAutoHide;
        }


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
        /// マウスカーソル移動イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateVisibility(e.GetPosition(this.Root));
        }

        /// <summary>
        /// パネル表示更新
        /// </summary>
        public void UpdateVisibility()
        {
            UpdateVisibility(Mouse.GetPosition(this.Root));
        }

        /// <summary>
        /// パネル表示更新
        /// </summary>
        /// <param name="point">マウス座標</param>
        private void UpdateVisibility(Point point)
        {
            if (_vm == null) return;

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
            DependencyProperty.Register("ViewContent", typeof(FrameworkElement), typeof(SidePanelFrameView), new PropertyMetadata(null));



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
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));



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
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


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
            DependencyProperty.Register("CanvasLeft", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));



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
            DependencyProperty.Register("CanvasTop", typeof(double), typeof(SidePanelFrameView), new PropertyMetadata(0.0));


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

        //
        private void LeftPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _vm.Left.ResetDelayHide();
        }

        //
        private void RightPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _vm.Right.ResetDelayHide();
        }

        private void LeftIconGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _vm.Left.Panel.Toggle();
        }

        private void RightIconGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _vm.Right.Panel.Toggle();
        }

        private void PanelIconItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
