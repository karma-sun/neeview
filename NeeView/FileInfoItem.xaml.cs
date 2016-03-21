// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
    /// FileInfoItem.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInfoItem : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(FileInfoItem), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FileInfoItem), new PropertyMetadata(null));


        // copy to clipboard command
        public static readonly ICommand ClipboardCopyCommand = new RoutedCommand("ClipboardCopyCommand", typeof(FileInfoItem));

        // copy to clipboard command execute
        private void ClipboardCopyCommand_Executed(object source, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(Text);
        }


        /// <summary>
        /// constructor
        /// </summary>
        public FileInfoItem()
        {
            InitializeComponent();

            this.CopyMenu.CommandBindings.Add(new CommandBinding(ClipboardCopyCommand, ClipboardCopyCommand_Executed));
        }
    }

}
