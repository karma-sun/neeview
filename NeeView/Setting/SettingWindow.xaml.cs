using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        /// <summary>
        /// このウィンドウが存在する間だけ設定されるインスタンス
        /// </summary>
        public static SettingWindow Current { get; private set; }

        public SettingWindowViewModel _vm;

        public SettingWindow()
        {
            InitializeComponent();
        }

        public SettingWindow(SettingWindowModel model)
        {
            InitializeComponent();

            Current = this;
            this.Closed += (s, e) => Current = null;

            _vm = new SettingWindowViewModel(model);
            this.DataContext = _vm;
        }

        /// <summary>
        /// 設定画面を閉じる時にデータ保存するフラグ
        /// </summary>
        public bool AllowSave { get; set; } = true;

        //
        private void SettingWindow_Closed(object sender, EventArgs e)
        {
            if (this.AllowSave)
            {
                WindowShape.Current.CreateSnapMemento();
                SaveData.Current.SaveAll();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.PageContent.Focus();
        }
    }

    public class BooleanToSwitchStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "オン" : "オフ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
