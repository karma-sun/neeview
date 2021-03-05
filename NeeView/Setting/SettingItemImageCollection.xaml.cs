using NeeView.Text;
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

namespace NeeView.Setting
{
    /// <summary>
    /// SettingItemImageCollection.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemImageCollection : UserControl
    {
        private bool _isStoreHelpEnabled = true;

        public SettingItemImageCollection()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public SettingItemImageCollection(double height, bool isStoreHelpEnabled) : this()
        {
            this.ExtensionsView.Height = height;
            _isStoreHelpEnabled = isStoreHelpEnabled;
        }

        #region Dependency Properties

        public StringCollection Collection
        {
            get { return (StringCollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(StringCollection), typeof(SettingItemImageCollection), new PropertyMetadata(null, CollectionPropertyChanged));

        private static void CollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingItemImageCollection control)
            {
                control.Refresh();
            }
        }

        #endregion

        public Config Config => Config.Current;

        private void Refresh()
        {
            if (Collection == null) return;
        }


        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ExternalProcess.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
