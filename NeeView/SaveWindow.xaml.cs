using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// SaveWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SaveWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private Exporter _Exporter;

        #region Property: IsHintDoubleImage
        private bool _IsHintDoubleImage;
        public bool IsHintDoubleImage
        {
            get { return _IsHintDoubleImage; }
            set
            {
                _IsHintDoubleImage = value;
                OnPropertyChanged();
                UpdateExporter();
            }
        }
        #endregion

        public bool IsEnableDoubleImage => !IsHintClone && _Exporter.DoubleImage != null;

        #region Property: IsHintBackground
        private bool _IsHintBackground;
        public bool IsHintBackground
        {
            get { return _IsHintBackground; }
            set
            {
                _IsHintBackground = value;
                OnPropertyChanged();
                UpdateExporter();
            }
        }
        #endregion

        public bool IsEnableBackground => !IsHintClone;

        #region Property: IsHintClone
        private bool _IsHintClone;
        public bool IsHintClone
        {
            get { return _IsHintClone; }
            set
            {
                _IsHintClone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnableDoubleImage));
                OnPropertyChanged(nameof(IsEnableBackground));
                UpdateExporter();
            }
        }
        #endregion

        #region Property: Thumbnail
        private BitmapSource _Thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _Thumbnail; }
            set { _Thumbnail = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: ThumbnailTitle
        private string _ThumbnailTitle;
        public string ThumbnailTitle
        {
            get { return _ThumbnailTitle; }
            set { _ThumbnailTitle = value; OnPropertyChanged(); }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exporter"></param>
        public SaveWindow(Exporter exporter)
        {
            InitializeComponent();

            this.ButtonSave.Focus();

            _Exporter = exporter;
            IsHintClone = _Exporter.IsHintClone;

            this.DataContext = this;
        }

        /// <summary>
        /// エクスポーター設定を更新
        /// </summary>
        private void UpdateExporter()
        {
            _Exporter.IsHintClone = IsHintClone;
            _Exporter.ExportType = (!IsHintClone && IsHintDoubleImage) ? ExportType.Double : ExportType.Single;
            _Exporter.IsHintBackground = (!IsHintClone && IsHintBackground);

            _Exporter.UpdateBitmapSource();

            Thumbnail = _Exporter.BitmapSource;
            ThumbnailTitle = _Exporter.CurrentImage.Name;
        }

        // 決定ボタン
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = Exporter.IsEnableExportFolder ? Exporter.ExportFolder : null;
            dialog.OverwritePrompt = true;

            dialog.AddExtension = true;

            var defaultExt = _Exporter.CurrentImage.DefaultExtension; 
            dialog.DefaultExt = defaultExt;

            // 拡張子は小文字限定
            var fileName = LoosePath.ValidFileName(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(_Exporter.CurrentImage.Name), defaultExt));
            dialog.FileName = fileName;

            if (!IsHintClone)
            {
                var pngExt = new string[] { ".png" };
                var jpgExt = new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" };

                string filter = "PNG|*.png|JPEG|*.jpg;*.jpeg;*.jpe;*.jfif";

                // クローン保存できない時は標準でPNGにする
                if (!_Exporter.CanClone(false))
                {
                    fileName = System.IO.Path.ChangeExtension(fileName, ".png");
                    dialog.FileName = fileName;
                    defaultExt = ".png";
                    dialog.DefaultExt = defaultExt;
                }

                // filter
                if (pngExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 1;
                }
                else if (jpgExt.Contains(defaultExt))
                {
                    dialog.FilterIndex = 2;
                }
                else if (_Exporter.CanClone(false))
                {
                    filter += $"|{dialog.DefaultExt.ToUpper()}|*.{dialog.DefaultExt}";
                    dialog.FilterIndex = 3;
                }

                dialog.Filter = filter + "|全てのファイル|*.*";
            }

            if (dialog.ShowDialog(this) == true)
            {
                _Exporter.Path = dialog.FileName;
                this.DialogResult = true;
                this.Close();
            }
        }

        // キャンセルボタン
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
