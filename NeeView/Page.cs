using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace NeeView
{
    public abstract class Page : INotifyPropertyChanged
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

        #region Debug
        // 開発用メッセージ
        #region Property: Message
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); }
        }
        #endregion

        Action<string> _MessageFunc;

        protected void SetMessage(string text)
        {
            if (_MessageFunc != null)
            {
                _MessageFunc(text);
            }
            else
            {
                Message = text;
            }
        }
        #endregion

        public static event EventHandler ContentChanged;

        public abstract string Path { get; }
        public abstract string FullPath { get; }

        public DateTime UpdateTime { get; protected set; }

        public double Width { get; protected set; }
        public double Height { get; protected set; }

        public Color Color { get; protected set; }

        protected object _Content;
        public object Content
        {
            get { return _Content; }
            set
            {
                if (_Content == value) return;
                _Content = value;
                ContentChanged?.Invoke(this, null);
            }
        }

        public static bool IsEnableAnimatedGif { get; set; }


        protected static Dictionary<BitmapLoaderType, List<BitmapLoaderType>> _LoaderOrderListCollection;
        protected static List<BitmapLoaderType> LoaderOrderList { get { return _LoaderOrderListCollection[LoaderOrder]; } }
        public static BitmapLoaderType LoaderOrder { get; set; }

        private static void InitializeLoaderOrderList()
        {
            var orderSusie = new List<BitmapLoaderType>();
            orderSusie.Add(BitmapLoaderType.Susie);
            orderSusie.Add(BitmapLoaderType.Default);

            var orderDefault = new List<BitmapLoaderType>();
            orderDefault.Add(BitmapLoaderType.Default);
            orderDefault.Add(BitmapLoaderType.Susie);

            _LoaderOrderListCollection = new Dictionary<BitmapLoaderType, List<BitmapLoaderType>>();
            _LoaderOrderListCollection.Add(BitmapLoaderType.Susie, orderSusie);
            _LoaderOrderListCollection.Add(BitmapLoaderType.Default, orderDefault);

            LoaderOrder = BitmapLoaderType.Default;
        }


        protected abstract object OpenContent();

        private JobRequest _JobRequest;

        //
        static Page()
        {
            InitializeLoaderOrderList();
        }

        //
        public Page()
        {
        }

        //
        public void Open(QueueElementPriority priority = QueueElementPriority.Default)
        {
            if (_Content != null) return;

            if (_JobRequest != null && !_JobRequest.IsCancellationRequested)
            {
                Message = $"Open... ({priority})";
                _JobRequest.ChangePriority(priority);
                return;
            }

            Message = $"Open... ({priority})";
            _JobRequest = ModelContext.JobEngine.Add(OpenJob, OpenCancel, priority);
        }

        //
        private void OpenCancel()
        {
            Message = $"Canceled.";
        }

        //
        private void OpenJob(CancellationToken cancel)
        {
            var content = OpenContent();

            if (cancel.IsCancellationRequested)
            {
                Message = $"Canceled.";
                return;
            }

            Message = "Valid.";
            Content = content;
        }

        //
        public void Close()
        {
            Message = "Closing...";

            if (_JobRequest != null)
            {
                _JobRequest.Cancel();
                _JobRequest = null;
            }

            if (_Content != null)
            {
                _Content = null;
                GC.Collect();
            }

            Message = "Closed.";
        }


        public static void Swap(ref Page lhs, ref Page rhs)
        {
            Page temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }

    //
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alart,
    }


    public class FilePageContext
    {
        public FilePageIcon Icon { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
    }

    //
    public class FilePage : Page
    {
        string _Path;
        public override string Path { get { return _Path; } }

        string _Place;
        public override string FullPath { get { return LoosePath.Combine(_Place, _Path); } }

        FilePageIcon _Icon;

        public FilePage(ArchiveEntry entry, FilePageIcon icon, string place)
        {
            _Place = place;
            _Path = entry.Path;
            _Icon = icon;
            UpdateTime = entry.UpdateTime;
        }

        protected override object OpenContent()
        {
            Width = 320;
            Height = 320 * 1.25;
            Color = Colors.Black;

            return new FilePageContext()
            {
                Icon = _Icon,
                FileName = _Path
            };
        }
    }


    //
    public class BitmapPage : Page
    {
        string _Path;
        public override string Path { get { return _Path; } }

        string _Place;
        public override string FullPath { get { return LoosePath.Combine(_Place, _Path); } }

        Archiver _Archiver;

        public override string ToString()
        {
            return LoosePath.GetFileName(_Path);
        }

        public BitmapPage(ArchiveEntry entry, Archiver archiver, string place)
        {
            _Path = entry.Path;
            UpdateTime = entry.UpdateTime;
            _Archiver = archiver;
            _Place = place;

            //Setting();
        }

        //FileReaderType _FileReaderType;

        // 設定テスト
        //public void Setting()
        //{
        //    if (_Archiver != null)
        //    {
        //        _FileReaderType = FileReaderType.ZipArchiveEntry;
        //    }
        //    else
        //    {
        //        _FileReaderType = FileReaderType.File;
        //    }
        //}

        //
        private BitmapSource LoadBitmap(Stream stream)
        {
            Exception exception = null;

            foreach (var loaderType in LoaderOrderList)
            {
                stream.Seek(0, SeekOrigin.Begin);

                exception = null;

                try
                {
                    var bitmapLoader = BitmapLoaderManager.Create(loaderType);
                    var bmp = bitmapLoader.Load(stream, _Path);
                    if (bmp != null) return bmp;
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Debug.WriteLine($"bitmap load failed {_Path} by {loaderType}");
            }

            throw exception ?? new ApplicationException("画像の読み込みに失敗しました");
        }

        //
        private BitmapSource Load()
        {
            using (var stream = _Archiver.OpenEntry(_Path))
            {
                return LoadBitmap(stream);
            }
        }

        ExtractTempFile _TempFile;

        //
        protected override object OpenContent()
        {
#if DEBUG
            //Thread.Sleep(200);
#endif
            try
            {
                var bitmapSource = Load();
                if (bitmapSource == null) throw new ApplicationException("cannot load by BitmapImge.");
                Width = bitmapSource.PixelWidth;
                Height = bitmapSource.PixelHeight;
                Color = bitmapSource.GetOneColor();

                if (IsEnableAnimatedGif && LoosePath.GetExtension(_Path) == ".gif")
                {
                    _TempFile = _TempFile ?? new ExtractTempFile(_Path, _Archiver);
                    return new Uri(_TempFile.FileName);
                }
                else
                {
                    return bitmapSource;
                }
            }
            catch (Exception e)
            {
                SetMessage("Exception: " + e.Message);
                Width = 320;
                Height = 320 * 1.25;
                Color = Colors.Black;

                return new FilePageContext()
                {
                    Icon = FilePageIcon.Alart,
                    FileName = _Path,
                    Message = e.Message
                };
            }
        }
    }


    public class ExtractTempFile : IDisposable
    {
        public string FileName { get; private set; }
        private bool _IsTempFile;

        public ExtractTempFile(string entryName, Archiver archiver)
        {
            if (archiver is FolderFiles)
            {
                FileName = Path.Combine(archiver.FileName, entryName);
            }
            else
            {
                FileName = Temporary.CreateCountedTempFileName("img", ".gif");
                archiver.ExtractToFile(entryName, FileName);
                _IsTempFile = true;

                // テンポラリファイルはアーカイバの廃棄と同時に削除されるようにする
                archiver.TrashBox.Add(this);
            }
        }

        public void Dispose()
        {
            if (_IsTempFile && FileName != null)
            {
                File.Delete(FileName);
                FileName = null;
            }
        }
    }


}
