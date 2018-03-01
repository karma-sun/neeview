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
    /// SettingItemSubControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemSubControl : UserControl
    {
        // TODO: ひとまずチェックボックにのみ対応している。

        public SettingItemSubControl()
        {
            InitializeComponent();
        }

        public SettingItemSubControl(string header, string tips, object content, bool isContentStretch)
        {
            InitializeComponent();

            this.ContentValue.Content = content;

            if (!string.IsNullOrWhiteSpace(tips))
            {
                this.ToolTip = tips;
            }

            if (!isContentStretch)
            {
                this.ContentValue.HorizontalAlignment = HorizontalAlignment.Left;
                this.ContentValue.Width = 300;
            }
        }
    }
}
