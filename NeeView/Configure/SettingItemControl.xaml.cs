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
    /// SettingItemControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemControl : UserControl
    {
        public SettingItemControl()
        {
            InitializeComponent();
        }

        public SettingItemControl(string header, string tips, object content, bool isContentStretch)
        {
            InitializeComponent();
            this.Header.Text = header;
            this.ToolTip = tips;
            if (isContentStretch)
            {
                this.ContentStretch.Content = content;
            }
            else
            {
                this.ContentValue.Content = content;
            }
        }
    }
}
