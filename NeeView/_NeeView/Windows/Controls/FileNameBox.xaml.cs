using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// FilenameBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FileNameBox : UserControl
    {
        private static string _defaultFileNote = "ファイルのパスを入力してください";
        private static string _defaultDirectoryNote = "フォルダーのパスを入力してください";

        //
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FileNameBox),
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
        public static readonly DependencyProperty DefaultDirectoryProperty =
            DependencyProperty.Register(
            "DefaultDirectory",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDefaultDirectoryChanged)));

        public string DefaultDirectory
        {
            get { return (string)GetValue(DefaultDirectoryProperty); }
            set { SetValue(DefaultDirectoryProperty, value); }
        }

        private static void OnDefaultDirectoryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        //
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(FileNameBox),
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
        public static readonly DependencyProperty IsDirectoryProperty =
            DependencyProperty.Register(
            "IsDirectory",
            typeof(bool),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDirectoryChanged)));

        public bool IsDirectory
        {
            get { return (bool)GetValue(IsDirectoryProperty); }
            set { SetValue(IsDirectoryProperty, value); }
        }

        private static void OnIsDirectoryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is FileNameBox control)
            {
                if (control.IsDirectory && control.Note == _defaultFileNote)
                {
                    control.Note = _defaultDirectoryNote;
                }
                else if(!control.IsDirectory && control.Note == _defaultDirectoryNote)
                {
                    control.Note = _defaultFileNote;
                }
            }
        }

        //
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTitleChanged)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        //
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
            "Filter",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFilterChanged)));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        private static void OnFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        //
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register(
            "Note",
            typeof(string),
            typeof(FileNameBox),
            new FrameworkPropertyMetadata(_defaultFileNote, new PropertyChangedCallback(OnNoteChanged)));

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        private static void OnNoteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        public class Wpf32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Wpf32Window(Window window)
            {
                this.Handle = new WindowInteropHelper(window).Handle;
            }
        }

        //
        public FileNameBox()
        {
            InitializeComponent();
        }

        //
        private void ButtonOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var owner = new Wpf32Window(Window.GetWindow(this));

            if (IsDirectory)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = Title ?? "フォルダー選択";
                dialog.SelectedPath = Text;

                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    dialog.SelectedPath = DefaultDirectory;
                }

                var result = dialog.ShowDialog(owner);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Text = dialog.SelectedPath;
                }
            }
            else
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Title = Title ?? "ファイル選択";
                dialog.FileName = Text;
                dialog.Filter = Filter;

                var result = dialog.ShowDialog(owner);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Text = dialog.FileName;
                }
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

            if (IsDirectory)
            {
                if (Directory.Exists(dropFiles[0]))
                {
                    Text = dropFiles[0];
                }
                else
                {
                    Text = Path.GetDirectoryName(dropFiles[0]);
                }
            }
            else
            {
                Text = dropFiles[0];
            }
        }
    }

}
