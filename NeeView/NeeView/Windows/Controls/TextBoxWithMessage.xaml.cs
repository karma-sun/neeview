using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// TextBoxWithMessage.xaml の相互作用ロジック
    /// </summary>
    public partial class TextBoxWithMessage : UserControl
    {
        public TextBoxWithMessage()
        {
            InitializeComponent();
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBoxWithMessage), new PropertyMetadata(null));


        public string EmptyMessage
        {
            get { return (string)GetValue(EmptyMessageProperty); }
            set { SetValue(EmptyMessageProperty, value); }
        }

        public static readonly DependencyProperty EmptyMessageProperty =
            DependencyProperty.Register("EmptyMessage", typeof(string), typeof(TextBoxWithMessage), new PropertyMetadata(null));
    }
}
