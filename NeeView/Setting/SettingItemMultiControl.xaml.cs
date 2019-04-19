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
    /// object content.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemMultiControl : UserControl
    {
        public SettingItemMultiControl()
        {
            InitializeComponent();
        }

        public SettingItemMultiControl(string header, string tips, object content1, object content2)
        {
            InitializeComponent();

            this.Header.Text = header;
            this.ContentValue1.Content = content1;
            this.ContentValue2.Content = content2;

            if (!string.IsNullOrWhiteSpace(tips))
            {
                this.Note.Text = tips;
                this.Note.Visibility = Visibility.Visible;
            }
        }
    }
}
