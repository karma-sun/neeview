using System;
using System.Text;
using System.Threading.Tasks;
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
    /// InputTouchSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class InputTouchSettingControl : UserControl
    {
        private InputTouchSettingViewModel _vm;

        public InputTouchSettingControl()
        {
            InitializeComponent();
        }

        //
        public void Initialize(CommandTable.Memento memento, CommandType key)
        {
            InitializeComponent();

            this.GestureBox.PreviewMouseLeftButtonUp += GestureBox_PreviewMouseLeftButtonUp;

            _vm = new InputTouchSettingViewModel(memento, key, this.GestureBox);
            DataContext = _vm;
        }

        //
        public void Flush()
        {
            _vm?.Flush();
        }

        //
        private void GestureBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var width = this.GestureBox.ActualWidth;
            var pos = e.GetPosition(this.GestureBox);

            _vm.SetTouchGesture(pos, this.GestureBox.ActualWidth, this.GestureBox.ActualHeight);
        }
    }

}
