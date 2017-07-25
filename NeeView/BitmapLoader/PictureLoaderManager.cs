// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PictureFile
    {
        public byte[] Raw { get; set; }
        public PictureInfo PictureInfo { get; set; }

        public BitmapSource BitmapSource { get; set; }
    }

    /// <summary>
    /// 画像ローダーのインターフェイス
    /// </summary>
    public interface IPictureFileLoader
    {
        bool IsEnabled { get; }

        PictureFile Load(ArchiveEntry entry);

        ////BitmapContentSource Load(Stream stream, ArchiveEntry entry);
        ////BitmapContentSource LoadFromFile(string fileName, ArchiveEntry entry);
    }




    /// <summary>
    /// BitmapLoader管理
    /// </summary>
    public class PictureLoaderManager : IPictureFileLoader
    {
        public static PictureLoaderManager Current { get; private set; }

        public bool IsEnabled => true;

        // サポート拡張子
        private Dictionary<BitmapLoaderType, string[]> _supprtedFileTypes = new Dictionary<BitmapLoaderType, string[]>()
        {
            [BitmapLoaderType.Default] = new string[] { ".bmp", ".dib", ".jpg", ".jpeg", ".jpe", ".jfif", ".gif", ".tif", ".tiff", ".png", ".ico", },
            [BitmapLoaderType.Susie] = new string[] { },
        };

        // ローダー優先順位
        private Dictionary<BitmapLoaderType, List<BitmapLoaderType>> _orderList = new Dictionary<BitmapLoaderType, List<BitmapLoaderType>>()
        {
            [BitmapLoaderType.Default] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Default,
                BitmapLoaderType.Susie,
            },
            [BitmapLoaderType.Susie] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Susie,
                BitmapLoaderType.Default,
            },
        };

        // ローダー優先順位の種類
        public BitmapLoaderType OrderType { set; get; } = BitmapLoaderType.Default;

        // ローダー優先リストを取得
        public List<BitmapLoaderType> OrderList
        {
            get { return _orderList[OrderType]; }
        }


        // コンストラクタ
        public PictureLoaderManager()
        {
            Current = this;
            UpdateDefaultSupprtedFileTypes();
        }


        // サポートしているローダーがあるか判定
        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != BitmapLoaderType.None;
        }

        // 除外パス判定
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => BookProfile.Current.Excludes.Contains(e));
        }

        // サポートしているローダーの種類を取得
        public BitmapLoaderType GetSupportedType(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in _orderList[OrderType])
            {
                if (type == BitmapLoaderType.Susie && !SusieBitmapLoader.IsEnable)
                {
                    continue;
                }

                if (_supprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return BitmapLoaderType.None;
        }

        // デフォルトローダーのサポート拡張子を更新
        public void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            foreach (var pair in DefaultBitmapLoader.GetExtensions())
            {
                list.AddRange(pair.Value.Split(','));
            }

            _supprtedFileTypes[BitmapLoaderType.Default] = list.ToArray();
        }


        // Susieローダーのサポート拡張子を更新
        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.INPlgunList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            _supprtedFileTypes[BitmapLoaderType.Susie] = list.Distinct().ToArray();
        }


        // ローダー作成
        public static IPictureFileLoader CreateLoader(BitmapLoaderType type)
        {
            switch (type)
            {
                case BitmapLoaderType.Default:
                    return new DefaultPictureLoader();

                case BitmapLoaderType.Susie:
                    return new SusiePictureLoader();

                default:
                    throw new ArgumentException("no support BitmapLoaderType.", nameof(type));
            }
        }

        //
        public PictureFile Load(ArchiveEntry entry)
        {
            var exceptions = new List<Exception>();

            foreach (var loaderType in this.OrderList)
            {
                try
                {
                    var bitmapLoader = PictureLoaderManager.CreateLoader(loaderType);
                    if (!bitmapLoader.IsEnabled) continue;

                    var bmp = bitmapLoader.Load(entry);

                    if (bmp != null)
                    {
                        //if (bmp.Info != null) bmp.Info.Archiver = entry.Archiver.ToString();
                        return bmp;
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.Message}\nat '{entry.EntryName}' by {loaderType}");
                    exceptions.Add(e);
                }
            }

            if (!exceptions.Any()) exceptions.Add(new IOException("画像の読み込みに失敗しました。"));

            throw new BitmapLoaderException(exceptions);
        }
    }

    //
    public class DefaultPictureLoader : IPictureFileLoader
    {
        public bool IsEnabled => true;

        //
        public PictureFile Load(ArchiveEntry entry)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var stream = entry.OpenEntry())
                {
                    // TODO: rawData switch
                    ////stream.CopyTo(memoryStream);
                    ////memoryStream.Seek(0, SeekOrigin.Begin);

                    // メタ情報取得
                    var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                    var pictureInfo = PictureInfo.Create(entry, new Size(bitmapFrame.PixelWidth, bitmapFrame.PixelHeight), (BitmapMetadata)bitmapFrame.Metadata);
                    pictureInfo.Decoder = ".Net BitmapImage";

                    var pictureFile = new PictureFile();
                    pictureFile.PictureInfo = pictureInfo;

                    // raw data
                    ////pictureFile.Raw = memoryStream.ToArray();

                    // bitmap
                    stream.Seek(0, SeekOrigin.Begin);
                    // TODO: bitmap LoadFlag
                    // TODO: LimitedSize
                    pictureFile.BitmapSource = DefaultBitmapFactory.Create(stream, Size.Empty);

                    // TODO: thumbnail
                    // TODO: thumbnail LoadFlag

                    return pictureFile;
                }
            }
        }


    }


    /// <summary>
    /// Susie画像ローダー
    /// </summary>
    public class SusiePictureLoader : IPictureFileLoader
    {
        public static bool IsEnable { get; set; } = true; // ## いかんなあ、このstatic

        private Susie.SusiePlugin _susiePlugin;

        // 有効判定
        public bool IsEnabled => IsEnable;

        //
        public PictureFile Load(ArchiveEntry entry)
        {
            if (entry.IsFileSystem)
            {
                return Load(entry.GetFileSystemPath(), entry);
            }
            else
            {
                using (var stream = entry.OpenEntry())
                {
                    return Load(stream, entry);
                }
            }
        }

        // Bitmap読み込み(stream)
        private PictureFile Load(Stream stream, ArchiveEntry entry)
        {
            if (!IsEnable) return null;

            byte[] buff;
            if (stream is MemoryStream)
            {
                buff = ((MemoryStream)stream).GetBuffer();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.GetBuffer();
                }
            }

            // TODO: PNGで取得できるように
            var bmp = SusieContext.Current.Susie?.GetPicture(entry.EntryName, buff, true, out _susiePlugin);
            var bmpSource = DefaultBitmapFactory.Create(bmp); // ファイル名は識別用
            if (bmpSource == null)
            {
                throw new SusieIOException();
            }

            var file = new PictureFile();
            file.PictureInfo = PictureInfo.Create(entry, new Size(bmpSource.PixelWidth, bmpSource.PixelHeight), null);
            file.PictureInfo.Decoder = _susiePlugin?.ToString();

            file.BitmapSource = bmpSource;

#if false
            // TODO: Susieから直接PNGで取得できるように ... 重いだけ？不要かもしれん
            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                encoder.Save(ms);

                using (var input = new MemoryStream(bmp))
                {
                    var bitmap = new System.Drawing.Bitmap(input);
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }

                file.Raw = ms.ToArray();
            }
#endif

            return file;
        }


        // Bitmap読み込み(ファイル版)
        private PictureFile Load(string fileName, ArchiveEntry entry)
        {
            if (!IsEnable) return null;

            var bmpSource = DefaultBitmapFactory.Create(SusieContext.Current.Susie?.GetPictureFromFile(fileName, true, out _susiePlugin));
            if (bmpSource == null)
            {
                throw new SusieIOException();
            }

            var file = new PictureFile();
            file.PictureInfo = PictureInfo.Create(entry, new Size(bmpSource.PixelWidth, bmpSource.PixelHeight), null);
            file.PictureInfo.Decoder = _susiePlugin?.ToString();

            // TODO: Susieから直接PNGで取得できるように
            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                encoder.Save(ms);
                file.Raw = ms.ToArray();
            }

            return file;
        }
    }

}
