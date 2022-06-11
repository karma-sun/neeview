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
    public enum FileDialogType
    {
        OpenFile,
        SaveFile,
        Directory,
    }

    /// <summary>
    /// FilenameBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FileNameBox : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        private static string _defaultFileNote = Properties.Resources.FileNameBox_File_Message;
        private static string _defaultDirectoryNote = Properties.Resources.FileNameBox_Directory_Message;

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
        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(FileNameBox), new PropertyMetadata(null));

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
        public FileDialogType FileDialogType
        {
            get { return (FileDialogType)GetValue(FileDialogTypeProperty); }
            set { SetValue(FileDialogTypeProperty, value); }
        }

        public static readonly DependencyProperty FileDialogTypeProperty =
            DependencyProperty.Register("FileDialogType", typeof(FileDialogType), typeof(FileNameBox), new PropertyMetadata(FileDialogType.OpenFile, OnFileDialogTypeChanged));

        private static void OnFileDialogTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileNameBox control)
            {
                control.RaisePropertyChanged(nameof(EmptyMessage));
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
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNoteChanged)));

        public string Note
        {
            get { return (string)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        private static void OnNoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileNameBox control)
            {
                control.RaisePropertyChanged(nameof(EmptyMessage));
            }
        }


        public FileNameBox()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }


        public string EmptyMessage
        {
            get => Note ?? (FileDialogType == FileDialogType.Directory ? Properties.Resources.FileNameBox_Directory_Message : Properties.Resources.FileNameBox_File_Message);
        }


        private void ButtonOpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var path = Text ?? "";
            var owner = new FileIO.Win32Window(Window.GetWindow(this));

            // check path chars
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            var invalidCharsIndex = path.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                path = "";
            }

            if (FileDialogType == FileDialogType.Directory)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = Title ?? Properties.Resources.FileNameBox_SelectDirectory;
                dialog.SelectedPath = path;

                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    dialog.SelectedPath = DefaultDirectory;
                }

                var result = dialog.ShowDialog(owner);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                }
            }
            else if (FileDialogType == FileDialogType.SaveFile)
            {
                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Title = Title ?? Properties.Resources.FileNameBox_SelectFile;
                dialog.InitialDirectory = string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path);
                dialog.FileName = string.IsNullOrWhiteSpace(path) ? DefaultText : Path.GetFileName(path);
                dialog.Filter = Filter;
                dialog.OverwritePrompt = false;
                dialog.CreatePrompt = false;

                var result = dialog.ShowDialog(owner);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }
            else
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Title = Title ?? Properties.Resources.FileNameBox_SelectFile;
                dialog.InitialDirectory = string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path);
                dialog.FileName = string.IsNullOrWhiteSpace(path) ? DefaultText : Path.GetFileName(path);
                dialog.Filter = Filter;

                var result = dialog.ShowDialog(owner);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }

            Text = path;
        }

        //
        private void PathTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        //
        private void PathTextBox_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;

            if (FileDialogType == FileDialogType.Directory)
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
