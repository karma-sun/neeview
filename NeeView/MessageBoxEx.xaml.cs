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
        private MessageBoxParams _param;


        public MessageBoxEx(MessageBoxParams param)
        {
            _param = param;

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

            // メッセージボックスのアイコン
            switch (param.Icon)
            {
                case MessageBoxExImage.Warning:
                    this.IconImage.Source = App.Current.Resources["ic_warning_48px"] as ImageSource;
                    System.Media.SystemSounds.Exclamation.Play();
                    break;

                case MessageBoxExImage.Error:
                    this.IconImage.Source = App.Current.Resources["ic_error_48px"] as ImageSource;
                    System.Media.SystemSounds.Exclamation.Play();
                    break;

                case MessageBoxExImage.RecycleBin:
                    this.IconImage.Source = App.Current.Resources["ic_delete_48px"] as ImageSource;
                    break;

                case MessageBoxExImage.Information:
                    this.IconImage.Source = App.Current.Resources["ic_warning_48px"] as ImageSource;
                    break;

                case MessageBoxExImage.Question:
                    this.IconImage.Source = App.Current.Resources["ic_help_24px"] as ImageSource;
                    break;

                default:
                    this.IconImage.Visibility = Visibility.Collapsed;
                    break;
            }

            // Visual
            if (param.VisualContent != null)
            {
                this.VisualControl.Content = param.VisualContent;
                this.VisualControl.Margin = new Thickness(0, 0, 20, 0);
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



        public static bool? Show(Window owner, string text, string caption, MessageBoxButton button, MessageBoxExImage icon)
        {
            var param = new MessageBoxParams();
            param.MessageBoxText = text;
            param.Caption = caption;
            param.Button = button;
            param.Icon = icon;

            var dialog = new MessageBoxEx(param);
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            return result;
        }

        public static bool? Show(Window owner, string text, string caption, MessageBoxButton button)
        {
            return Show(owner, text, caption, button, MessageBoxExImage.None);
        }

        public static bool? Show(Window owner, string text, string caption)
        {
            return Show(owner, text, caption, MessageBoxButton.OK, MessageBoxExImage.None);
        }

        public static bool? Show(Window owner, string text)
        {
            return Show(owner, text, "通知", MessageBoxButton.OK, MessageBoxExImage.None);
        }


        public static bool? Show(string text, string caption, MessageBoxButton button, MessageBoxExImage icon)
        {
            var param = new MessageBoxParams();
            param.MessageBoxText = text;
            param.Caption = caption;
            param.Button = button;
            param.Icon = icon;

            var dialog = new MessageBoxEx(param);
            var result = dialog.ShowDialog();
            return result;
        }

        public static bool? Show(string text, string caption, MessageBoxButton button)
        {
            return Show(text, caption, button, MessageBoxExImage.None);
        }

        public static bool? Show(string text, string caption)
        {
            return Show(text, caption, MessageBoxButton.OK, MessageBoxExImage.None);
        }

        public static bool? Show(string text)
        {
            return Show(text, "", MessageBoxButton.OK, MessageBoxExImage.None);
        }
    }
}
