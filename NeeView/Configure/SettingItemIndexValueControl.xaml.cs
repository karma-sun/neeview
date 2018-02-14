using NeeView.Windows.Property;
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
        public IIndexValue IndexValue
        {
            get { return (IIndexValue)GetValue(IndexValueProperty); }
            set { SetValue(IndexValueProperty, value); }
        }

        public static readonly DependencyProperty IndexValueProperty =
            DependencyProperty.Register("IndexValue", typeof(IIndexValue), typeof(SettingItemIndexValueControl), new PropertyMetadata(null));


        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(SettingItemIndexValueControl), new PropertyMetadata(false));


        public SettingItemIndexValueControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }
    }
}
