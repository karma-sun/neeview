// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// MessageBoxEx.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageBoxEx : Window
    {
        MessageBoxParams _Param;

        public MessageBoxEx(MessageBoxParams param)
        {
            _Param = param;

            InitializeComponent();

            this.Title = param.Caption;
            this.MessageBoxText.Text = param.MessageBoxText;

            switch (param.Button)
            {
                case MessageBoxButton.OK:
                    this.YesButton.Content = "OK";
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    this.YesButton.Content = "OK";
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Content = "Cancel";
                    break;
                case MessageBoxButton.YesNo:
                    this.YesButton.Content = "Yes";
                    this.NoButton.Content = "No";
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.YesButton.Content = "Yes";
                    this.NoButton.Content = "No";
                    this.CancelButton.Content = "Cancel";
                    break;
                default:
                    throw new NotSupportedException();
            }

            this.YesButton.Focus(); // Yesボタンにフォーカス

            // TODO: icon
            switch (param.Icon)
            {
                case MessageBoxImage.Exclamation:
                    System.Media.SystemSounds.Exclamation.Play();
                    break;
            }

            // Visual
            if (param.VisualContent != null)
            {
                this.VisualControl.Content = param.VisualContent;
                this.VisualControl.Margin = new Thickness(20, 10, 20, 10);
            }
                
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
