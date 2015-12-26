/*
Copyright (c) 2015 Mitsuhiro Ito (nee)

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FilenameBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FilenameBox : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FilenameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }


        //
        public static readonly DependencyProperty OpenFileDialogProperty =
            DependencyProperty.Register(
            "OpenFileDialog",
            typeof(OpenFileDialog),
            typeof(FilenameBox),
            new FrameworkPropertyMetadata(new OpenFileDialog(), new PropertyChangedCallback(OnOpenFileDialogChanged)));

        public OpenFileDialog OpenFileDialog
        {
            get { return (OpenFileDialog)GetValue(OpenFileDialogProperty); }
            set { SetValue(OpenFileDialogProperty, value); }
        }

        private static void OnOpenFileDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }


        //
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(FilenameBox),
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
        public FilenameBox()
        {
            InitializeComponent();
        }

        private void ButtonOpenDialog_Click(object sender, RoutedEventArgs e)
        {
#if false
            var dialog = OpenFileDialog;

            //dialog.InitialDirectory = VM.ProjectFolder;
            //dialog.Title = "プロジェクトファイルの読み込み";
            //dialog.DefaultExt = "*.png";
            //dialog.Filter = "PNG File|*.png";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                Text = dialog.FileName;
            }
#else
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "フォルダ選択";
            dialog.SelectedPath = Text;

            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Text = dialog.SelectedPath;
            }
#endif
        }

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


        private void PathTextBox_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;
            Text = dropFiles[0];
        }
    }


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