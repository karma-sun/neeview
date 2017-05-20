using System;
using System.Collections.Generic;
using System.Linq;
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
    /// PageSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSlider : UserControl
    {
        // TODO: これは仮です
        public MainWindowVM ViewModel
        {
            get { return (MainWindowVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainWindowVM), typeof(PageSlider), new PropertyMetadata(null, ViewMdoel_Changed));

        private static void ViewMdoel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageSlider control)
            {
                control.Initialize();
            }
        }




        public UIElement FocusTo
        {
            get { return (UIElement)GetValue(FocusToProperty); }
            set { SetValue(FocusToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusTo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusToProperty =
            DependencyProperty.Register("FocusTo", typeof(UIElement), typeof(PageSlider), new PropertyMetadata(null));




        // TODO: これは仮です
        private MainWindowVM _VM;


        /// <summary>
        /// constructor
        /// </summary>
        public PageSlider()
        {
            InitializeComponent();

        }

        public void Initialize()
        {
            // 仮
            _VM = this.ViewModel;
            this.Root.DataContext = _VM;

            // マーカー初期化
            this.PageMarkers.Initialize(_VM.BookHub);
        }

        //
        public void OnIsSliderDirectionReversedChanged()
        {
            // Retrieve the Track from the Slider control
            var track = this.Slider.Template.FindName("PART_Track", this.Slider) as System.Windows.Controls.Primitives.Track;
            // Force it to rerender
            track.InvalidateVisual();

            this.PageMarkers.IsSliderDirectionReversed = _VM.IsSliderDirectionReversed;
        }



        /// <summary>
        /// スライダーエリアでのマウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = MouseInputHelper.DeltaCount(e);

            for (int i = 0; i < turn; ++i)
            {
                if (e.Delta < 0)
                {
                    _VM.BookHub.NextPage();
                }
                else
                {
                    _VM.BookHub.PrevPage();
                }
            }
        }

        // スライダーに乗ったら表示開始
        private void PageSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            // nop.
        }
        //
        private void PageSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // nop.
        }

        private void PageSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_VM.CanSliderLinkedThumbnailList)
            {
                BookOperation.Current.SetIndex(BookOperation.Current.Index);
            }
        }

        private void PageSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // nop.
        }

        private void PageSliderTextBox_ValueChanged(object sender, EventArgs e)
        {
            BookOperation.Current.SetIndex(BookOperation.Current.Index);
        }


        // テキストボックス入力時に単キーのショートカットを無効にする
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカット無効
            KeyExGesture.AllowSingleKey = false;
            //e.Handled = true;
        }

    }
}
