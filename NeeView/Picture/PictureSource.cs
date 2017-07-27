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
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
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
                size = UniformedSize(new Size(Math.Min(size.Width, _profile.Maximum.Width), Math.Min(size.Height, _profile.Maximum.Height)));
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

            // んー
            var bitmapFactory = new BitmapFactory();

            //var factory = new DefaultBitmapFactory();
            using (var stream = CreateStream())
            {
                var sw = Stopwatch.StartNew();
                var bitmap = bitmapFactory.Create(stream, size);
                Debug.WriteLine($"{ArchiveEntry.EntryLastName}: {size.Truncate()}: {sw.ElapsedMilliseconds}ms");
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

}
