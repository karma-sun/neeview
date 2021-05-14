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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ProgressRing.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressRing : UserControl
    {
        public ProgressRing()
        {
            InitializeComponent();

            this.Root.Visibility = Visibility.Collapsed;
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, IsActivePropertyChanged));

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressRing control)
            {
                control.UpdateAnimationState();
            }
        }

        private void UpdateAnimationState()
        {
            if (IsActive)
            {
                this.Root.Visibility = Visibility.Visible;

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
#if false
                var aniRotate = new DoubleAnimation();
                aniRotate.By = 45;
                aniRotate.Duration = TimeSpan.FromSeconds(0.25);
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
#endif

                this.Root.Visibility = Visibility.Collapsed;

                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }
    }
}
