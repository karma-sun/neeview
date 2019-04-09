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
    /// ExpandedMark.xaml の相互作用ロジック
    /// </summary>
    public partial class ExpandedMark : UserControl
    {
        public ExpandedMark()
        {
            InitializeComponent();

            IsEnabledChanged += (s, e) => Refresh();
        }


        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ExpandedMark), new PropertyMetadata(false, IsExpanded_Changed));

        private static void IsExpanded_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExpandedMark control)
            {
                control.Refresh();
            }
        }

        public void Refresh()
        {
            this.Expanded.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
            this.NotExpanded.Visibility = IsExpanded ? Visibility.Collapsed : Visibility.Visible;
            this.Root.Visibility = IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
