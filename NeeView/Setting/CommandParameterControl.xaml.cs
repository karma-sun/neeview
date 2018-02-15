using System;
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
    /// CommandParameterControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandParameterControl : UserControl
    {
        #region DependencyProperties

        public bool IsAny
        {
            get { return (bool)GetValue(IsAnyProperty); }
            private set { SetValue(IsAnyProperty, value); }
        }

        public static readonly DependencyProperty IsAnyProperty =
            DependencyProperty.Register("IsAny", typeof(bool), typeof(CommandParameterControl), new PropertyMetadata(false));

        #endregion

        private CommandParameterViewModel _vm;

        public CommandParameterControl()
        {
            InitializeComponent();
        }

        public void Initialize(CommandTable.Memento memento, CommandType key)
        {
            InitializeComponent();

            _vm = new CommandParameterViewModel(memento, key);
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

        public void Flush()
        {
            _vm?.Flush();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _vm?.Reset();
            this.Inspector.Reflesh(); // TODO: MVVM的に更新されるようにする
        }
    }
}
