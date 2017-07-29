// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// サムネイル.
    /// Jpegで保持し、必要に応じてBitmapSourceを生成
    /// </summary>
    public class Thumbnail : BindableBase, IDisposable
    {
        /// <summary>
        /// 有効判定
        /// </summary>
        internal bool IsValid => (_image != null);

        /// <summary>
        /// 変更イベント
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// 参照イベント
        /// </summary>
        public event EventHandler Touched;


        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        private byte[] _image;
        public byte[] Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    if (Image != null)
                    {
                        Changed?.Invoke(this, null);
                        Touched?.Invoke(this, null);
                        RaisePropertyChanged(nameof(BitmapSource));
                        RaisePropertyChanged(nameof(IsUniqueImage));
                    }
                }
            }
        }

        /// <summary>
        /// ユニークイメージ？
        /// </summary>
        public bool IsUniqueImage => _image != null && _image != _emptyImage;


        /// <summary>
        /// View用Bitmapプロパティ
        /// </summary>
        public BitmapSource BitmapSource => CreateBitmap();

        /// <summary>
        /// 寿命間利用シリアルナンバー
        /// </summary>
        public int LifeSerial { get; set; }

        /// <summary>
        /// キャッシュ使用
        /// </summary>
        public bool IsSupprtedCache { get; set; }

        /// <summary>
        /// キャシュ用ヘッダ
        /// </summary>
        public ThumbnailCacheHeader _header { get; set; }


        /// <summary>
        /// キャッシュを使用してサムネイル生成を試みる
        /// </summary>
        internal void Initialize(ArchiveEntry entry, string appendix)
        {
            if (IsValid || !IsSupprtedCache) return;

            _header = new ThumbnailCacheHeader(entry.FullName, entry.Length, entry.LastWriteTime, appendix);
            var image = ThumbnailCache.Current.Load(_header);

            Image = image;
        }

        /// <summary>
        /// 画像データから初期化
        /// </summary>
        /// <param name="source"></param>
        internal void Initialize(byte[] image)
        {
            if (IsValid) return;

            Image = image ?? _emptyImage;
        }


        /// <summary>
        /// image無効
        /// </summary>
        public void Clear()
        {
            // 通知は不要なので直接パラメータ変更
            _image = null;
        }

        /// <summary>
        /// Touch
        /// </summary>
        public void Touch()
        {
            Touched?.Invoke(this, null);
        }


        /// <summary>
        /// BitmapSource取得
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmap()
        {
            if (IsValid)
            {
                Touched?.Invoke(this, null);
                if (_image == _emptyImage)
                {
                    return EmptyBitmapSource;
                }
                else
                {
                    return DecodeFromJpeg(_image);
                }
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// BitmapSource to Jpeg
        /// </summary>
        private byte[] EncodeToJpeg(BitmapSource source)
        {
            using (var stream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = ThumbnailProfile.Current.Quality;
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);

                stream.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Jpeg to BitmapSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private BitmapSource DecodeFromJpeg(byte[] image)
        {
            using (var stream = new MemoryStream(image, false))
            {
                JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var bitmap = decoder.Frames[0];
                bitmap.Freeze();
                return bitmap;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _image = null;
            Changed = null;
            Touched = null;
            ResetPropertyChanged();
        }



        /// <summary>
        /// Empty Image Key
        /// </summary>
        public static byte[] _emptyImage = System.Text.Encoding.ASCII.GetBytes("EMPTY!");

        /// <summary>
        /// EmptyBitmapSource property.
        /// </summary>
        private static BitmapSource _emptyBitmapSource;
        public static BitmapSource EmptyBitmapSource
        {
            get
            {
                if (_emptyBitmapSource == null)
                {
                    Uri resourceUri = new Uri("/Resources/Empty.png", UriKind.Relative);
                    _emptyBitmapSource = new BitmapImage(resourceUri);
                }
                return _emptyBitmapSource;
            }
        }
    }
}
