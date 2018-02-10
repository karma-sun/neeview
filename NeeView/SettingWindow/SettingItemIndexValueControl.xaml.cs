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
    /// SettingItemIndexValue.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemIndexValueControl : UserControl
    {
        public IndexDoubleValue IndexValue
        {
            get { return (IndexDoubleValue)GetValue(IndexValueProperty); }
            set { SetValue(IndexValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IndexValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexValueProperty =
            DependencyProperty.Register("IndexValue", typeof(IndexDoubleValue), typeof(SettingItemIndexValueControl), new PropertyMetadata(null, IndexValue_Changed));

        private static void IndexValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemIndexValueControl control)
            {
                control.Root.DataContext = control.IndexValue;
            }
        }

        public SettingItemIndexValueControl()
        {
            InitializeComponent();
        }
    }
}
