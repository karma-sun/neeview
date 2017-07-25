using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像情報
    /// </summary>
    public class PictureInfo
    {
        /// <summary>
        /// 画像サイズ
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Length { get; set; } = -1;

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime? LastWriteTime { get; set; }

        /// <summary>
        /// EXIF
        /// </summary>
        public BitmapExif Exif { get; set; }


        /// <summary>
        /// Archiver
        /// </summary>
        public string Archiver { get; set; }

        /// <summary>
        /// Decoder
        /// </summary>
        public string Decoder { get; set; }


        // 実際に読み込まないとわからないもの

        /// <summary>
        /// 基本色
        /// </summary>
        public Color Color { get; set; } = Colors.Black;

        /// <summary>
        /// ピクセル深度
        /// </summary>
        public int BitsPerPixel { get; set; }


        //
        public bool IsPixelInfoEnabled => BitsPerPixel > 0;

        //
        public void SetPixelInfo(BitmapSource bitmap)
        {
            // 基本色
            this.Color = bitmap.GetOneColor();

            // ピクセル深度
            this.BitsPerPixel = bitmap.GetSourceBitsPerPixel();
        }


        //
        public static PictureInfo Create(ArchiveEntry entry, Size size, BitmapMetadata metadata)
        {
            var info = new PictureInfo();
            info.Size = size;
            info.Length = entry.Length;
            info.LastWriteTime = entry.LastWriteTime;
            info.Exif = metadata != null ? new BitmapExif(metadata) : null;
            info.Archiver = entry.Archiver.ToString();

            return info;
        }
    }

    //
    public class PictureProfile
    {
        public static PictureProfile Current { get; set; }

        // メモリ節約用
        public Size Maximum { get; set; } = new Size(4096, 4096);

        // リサイズがあればこれは不要か。
        public Size Minimum { get; set; } = new Size(1, 1);

        public PictureProfile()
        {
            Current = this;
        }
    }

    //
    public class Picture : BindableBase
    {
        private PictureSourceBase _source;

        //
        public Picture(ArchiveEntry entry)
        {
            _source = PictureSourceFactory.Create(entry);
        }

        //
        public void Load()
        {
            var pictureFile = PictureLoaderManager.Current.Load(_source.ArchiveEntry);

            _source.RawData = pictureFile.Raw;
            _source.PictureInfo = pictureFile.PictureInfo;

            RaisePropertyChanged(nameof(PictureInfo));
        }

        //
        public async Task LoadAsync()
        {
            await Task.Run(() => Load());
        }

        //
        public PictureInfo PictureInfo => _source.PictureInfo;


        /// <summary>
        /// BitmapSource property.
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }


        private Size _size = Size.Empty;


        // TODO: OutOfMemory時のリトライ
        public BitmapSource CreateBitmap(Size size)
        {
            if (_source.PictureInfo == null)
            {
                Load();
            }

            if (_bitmapSource != null && size == _size) return _bitmapSource;

            size = _source.CreateFixedSize(size);
            if (_bitmapSource != null && size == _size) return _bitmapSource;

            this.BitmapSource = _source.CreateBitmap(size);
            _size = size;

            if (!_source.PictureInfo.IsPixelInfoEnabled)
            {
                try
                {
                    _source.PictureInfo.SetPixelInfo(_bitmapSource);
                    RaisePropertyChanged(nameof(PictureInfo));
                }
                catch (Exception)
                {
                    // この例外では停止させない
                }
            }

            return _bitmapSource;
        }

        //
        public async Task<BitmapSource> CreateBitmapAsync(Size size)
        {
            return await Task.Run(() => CreateBitmap(size));
        }


        //
        public void ClearBitmap()
        {
            this.BitmapSource = null;
            _size = Size.Empty;
        }


        private Size? _request;
        private bool _isBusy;
        private object _lock = new object();

        //
        public void RequestCreateBitmap(Size size)
        {
            lock (_lock)
            {
                _request = size;
            }

            if (!_isBusy)
            {
                _isBusy = true;
                Task.Run(() => CreateBitmapTask());
            }
        }

        //
        public void CreateBitmapTask()
        {
            try
            {
                while (_request != null)
                {
                    var size = (Size)_request;
                    lock (_lock)
                    {
                        _request = null;
                    }
                    CreateBitmap(size);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

    }


    /// <summary>
    /// 画像
    /// </summary>
    public abstract class PictureSourceBase
    {
        /// <summary>
        /// アーカイブエントリ
        /// </summary>
        public ArchiveEntry ArchiveEntry { get; private set; }

        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo PictureInfo { get; set; }

        /// <summary>
        /// ファイルデータ
        /// リサイズではこのメモリを元に画像を再生成する。
        /// nullの場合、ArchiveEntryから生成する。
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// 画像。保持する必要あるのか？
        /// </summary>
        public BitmapSource BitmapSource { get; set; }

        //
        public PictureSourceBase(ArchiveEntry entry)
        {
            this.ArchiveEntry = entry;
        }


        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns></returns>
        public Stream CreateStream()
        {
            if (this.RawData != null)
            {
                return new MemoryStream(this.RawData);
            }
            else
            {
                return ArchiveEntry.OpenEntry();
            }
        }


        //
        public abstract byte[] CreateRawData();

        //
        public abstract PictureInfo CreatePictureInfo();

        //
        protected PictureInfo CreateBasicPublicInfo(Size size, BitmapMetadata metadata)
        {
            var info = new PictureInfo();
            info.Size = size;
            info.Length = this.ArchiveEntry.Length;
            info.LastWriteTime = this.ArchiveEntry.LastWriteTime;
            info.Exif = metadata != null ? new BitmapExif(metadata) : null;
            info.Archiver = this.ArchiveEntry.Archiver.ToString();
            info.Decoder = ".Net BitmapImage";

            return info;
        }

        //
        public abstract Size CreateFixedSize(Size size);


        // 画像アスペクト比を保つ最大のサイズを返す
        protected Size UniformedSize(Size size)
        {
            var rateX = size.Width / this.PictureInfo.Size.Width;
            var rateY = size.Height / this.PictureInfo.Size.Height;

            if (rateX < rateY)
            {
                return new Size(size.Width, this.PictureInfo.Size.Height * rateX);
            }
            else
            {
                return new Size(this.PictureInfo.Size.Width * rateY, size.Height);
            }
        }

        //
        public BitmapSource CreateBitmap()
        {
            return CreateBitmap(Size.Empty);
        }

        //
        public abstract BitmapSource CreateBitmap(Size size);

    }


    public class PictureSource : PictureSourceBase
    {
        private PictureProfile _profile => PictureProfile.Current;

        // constructor
        public PictureSource(ArchiveEntry entry) : base(entry)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override byte[] CreateRawData()
        {
            using (var memoryStream = new MemoryStream())
            using (var stream = this.ArchiveEntry.OpenEntry())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 画像情報生成
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override PictureInfo CreatePictureInfo()
        {
            // 画像サイズとメタデータのみ読み込んで情報を生成する
            using (var stream = CreateStream())
            {
                var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                return CreateBasicPublicInfo(new Size(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight), (BitmapMetadata)bitmapFrame.Metadata);
            }
        }

        //
        public override Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty)
            {
                // 最大サイズを超えないようにする
                if (this.PictureInfo.Size.Width > _profile.Maximum.Width || this.PictureInfo.Size.Height > _profile.Maximum.Height)
                {
                    return UniformedSize(_profile.Maximum);
                }

                // TODO: 最小サイズは？
            }
            else
            {
                size = UniformedSize(new Size(size.Width.Clamp(_profile.Minimum.Width, _profile.Maximum.Width), size.Height.Clamp(_profile.Minimum.Height, _profile.Maximum.Height)));
                if (Math.Abs(size.Width - this.PictureInfo.Size.Width) < 1.0 && Math.Abs(size.Height - this.PictureInfo.Size.Height) < 1.0)
                {
                    Debug.WriteLine($"NearEqual !!");
                    size = Size.Empty;
                }
            }

            return size;
        }



        /// <summary>
        /// 指定サイズで画像データ生成
        /// </summary>
        /// <param name="size"></param>
        public override BitmapSource CreateBitmap(Size size)
        {
            if (size.IsEmpty)
            {
                // 最大座サイズを超える場合は正規化
                if (this.PictureInfo.Size.Width > _profile.Maximum.Width || this.PictureInfo.Size.Height > _profile.Maximum.Height)
                {
                    size = UniformedSize(_profile.Maximum);
                }
            }



            var factory = new DefaultBitmapFactory();
            using (var stream = CreateStream())
            {
                var sw = Stopwatch.StartNew();
                var bitmap = factory.Create(stream, size);
                Debug.WriteLine($"{ArchiveEntry.EntryLastName}: {size.ToInteger()}: {sw.ElapsedMilliseconds}ms");
                return bitmap;
            }

        }
    }

    public class PdfPageSource : PictureSourceBase
    {
        PdfArchiverProfile _profile => PdfArchiverProfile.Current;

        public PdfPageSource(ArchiveEntry entry) : base(entry)
        {
        }

        public override byte[] CreateRawData()
        {
            return null;
        }

        public override PictureInfo CreatePictureInfo()
        {
            var info = CreateBasicPublicInfo(new Size(0, 0), null);
            return info;
        }

        public override Size CreateFixedSize(Size size)
        {
            return size.IsEmpty
                ? _profile.RenderSize
                : new Size(
                    NVUtility.Clamp(size.Width, _profile.RenderSize.Width, _profile.RenderMaxSize.Width),
                    NVUtility.Clamp(size.Height, _profile.RenderSize.Height, _profile.RenderMaxSize.Height));
        }

        public override BitmapSource CreateBitmap(Size size)
        {
            var pdfArchiver = this.ArchiveEntry.Archiver as PdfArchiver;
            return pdfArchiver.CraeteBitmapSource(this.ArchiveEntry, size.IsEmpty ? _profile.RenderSize : size);
        }

    }


    public static class PictureSourceFactory
    {
        public static PictureSourceBase Create(ArchiveEntry entry)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return new PdfPageSource(entry);
            }
            else
            {
                return new PictureSource(entry);
            }
        }
    }

    /// <summary>
    /// 標準の画像生成処理
    /// </summary>
    public class DefaultBitmapFactory
    {
        //
        public BitmapImage Create(Stream stream, Size size)
        {
            try
            {
                return Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, size);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"DefaultBitmap: {e.Message}");
                stream.Seek(0, SeekOrigin.Begin);
                return Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad, size);
            }
        }

        //
        private BitmapImage Create(Stream stream, BitmapCreateOptions createOption, BitmapCacheOption cacheOption, Size size)
        {
            var bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.CreateOptions = createOption;
            bmpImage.CacheOption = cacheOption;
            bmpImage.StreamSource = stream;
            if (size != Size.Empty)
            {
                bmpImage.DecodePixelHeight = (int)size.Height;
                bmpImage.DecodePixelWidth = (int)size.Width;
            }
            bmpImage.EndInit();
            bmpImage.Freeze();

            return bmpImage;
        }

    }



    public static class SizeExtensions
    {
        public static Size ToInteger(this Size self)
        {
            return self.IsEmpty ? self : new Size((int)self.Width, (int)self.Height);
        }
    }

}
