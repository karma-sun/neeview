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

namespace NeeView.Configure
{
    /// <summary>
    /// MouseGestureSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseGestureSettingControl : UserControl
    {
        private MouseGestureSettingViewModel _vm;

        public MouseGestureSettingControl()
        {
            InitializeComponent();
        }

        //
        public void Initialize(CommandTable.Memento memento, CommandType key)
        {
            _vm = new MouseGestureSettingViewModel(memento, key, this.GestureBox);
            this.DataContext = _vm;
        }

        //
        public void Flush()
        {
            _vm?.Flush();
        }
    }
}
