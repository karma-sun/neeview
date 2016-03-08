// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace NeeView
{
    /// <summary>
    /// FilenameBox.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryNameBox : UserControl
    {
        // Text
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(DirectoryNameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }


        // DefaultDirectory
        public static readonly DependencyProperty DefaultDirectoryProperty =
            DependencyProperty.Register(
            "DefaultDirectory",
            typeof(string),
            typeof(DirectoryNameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDefaultDirectoryChanged)));

        public string DefaultDirectory
        {
            get { return (string)GetValue(DefaultDirectoryProperty); }
            set { SetValue(DefaultDirectoryProperty, value); }
        }

        private static void OnDefaultDirectoryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }


        // IsValid
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(DirectoryNameBox),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsValidChanged)));

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

        private static void OnIsValidChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }



        //
        public DirectoryNameBox()
        {
            InitializeComponent();
        }

        //
        private void ButtonOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "フォルダ選択";
            dialog.SelectedPath = Text;

            if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                dialog.SelectedPath = DefaultDirectory;
            }

            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Text = dialog.SelectedPath;
            }
        }

        //
        private void PathTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        //
        private void PathTextBox_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;

            if (Directory.Exists(dropFiles[0]))
            {
                Text = dropFiles[0];
            }
            else
            {
                Text = Path.GetDirectoryName(dropFiles[0]);
            }
        }
    }

    //
    [System.Windows.Data.ValueConversion(typeof(bool), typeof(Visibility))]
    public class NotBoolToVisiblityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}