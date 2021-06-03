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
    /// DragActionParameterControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DragActionParameterControl : UserControl
    {
        #region DependencyProperties

        public bool IsAny
        {
            get { return (bool)GetValue(IsAnyProperty); }
            private set { SetValue(IsAnyProperty, value); }
        }

        public static readonly DependencyProperty IsAnyProperty =
            DependencyProperty.Register("IsAny", typeof(bool), typeof(DragActionParameterControl), new PropertyMetadata(false));

        #endregion

        private DragActionParameterViewModel _vm;

        public DragActionParameterControl()
        {
            InitializeComponent();
        }

        public void Initialize(DragActionCollection commandMap, string key)
        {
            InitializeComponent();

            _vm = new DragActionParameterViewModel(commandMap, key);
            this.DataContext = _vm;

            this.IsAny = _vm.PropertyDocument != null;

            if (this.IsAny)
            {
                this.EmptyText.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MainPanel.Visibility = Visibility.Collapsed;
            }
        }


        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _vm?.Reset();
            this.Inspector.Refresh(); // TODO: MVVM的に更新されるようにする
        }
    }
}
