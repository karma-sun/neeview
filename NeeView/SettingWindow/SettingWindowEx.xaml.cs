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
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// SettingWindowEx.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindowEx : Window
    {
        /// <summary>
        /// このウィンドウが存在する間だけ設定されるインスタンス
        /// </summary>
        public static SettingWindowEx Current { get; private set; }

        public SettingWindowViewModel _vm;

        public SettingWindowEx()
        {
            InitializeComponent();
        }

        public SettingWindowEx(SettingWindowModel model)
        {
            InitializeComponent();

            Current = this;
            this.Closed += (s, e) => Current = null;

            _vm = new SettingWindowViewModel(model);
            this.DataContext = _vm;
        }
    }
}
