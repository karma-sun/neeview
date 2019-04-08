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
    /// VideoControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaControlView : UserControl
    {
        private MediaControlViewModel _vm;

        public MediaControlView()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        public MediaControl Source
        {
            get { return (MediaControl)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MediaControl), typeof(MediaControlView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControlView control)
            {
                control.Initialize();
            }
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (Source == null) return;

            _vm = new MediaControlViewModel(Source);
            this.DataContext = _vm;
        }

        private void VideoSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.SetScrubbing(true);
        }

        private void VideoSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.SetScrubbing(false);
        }

        private void TimeTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _vm.ToggleTimeFormat();
        }

        private void Root_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _vm.MouseWheel(sender, e);
            e.Handled = true;
        }

        private void Volume_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _vm.MouseWheelVolume(sender, e);
            e.Handled = true;
        }

        #endregion

    }


}
