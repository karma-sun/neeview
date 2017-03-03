// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public class Thumbnail : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        public static double Size { get; set; } = 256;

        /// <summary>
        /// 品質
        /// </summary>
        public static int Quality { get; set; } = 80;

        /// <summary>
        /// 有効判定
        /// </summary>
        internal bool IsValid => (_image != null);

        /// <summary>
        /// 変更イベント
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        private byte[] _image;

        /// <summary>
        /// View用Bitmapプロパティ
        /// </summary>
        public BitmapSource BitmapSource => CreateBitmap();

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="source"></param>
        internal void Initialize(BitmapSource source)
        {
            if (IsValid) return;

            var sw = new Stopwatch();
            sw.Start();

            var bitmapSource = Utility.NVGraphics.CreateThumbnail(source, new Size(Size, Size));
            _image = EncodeToJpeg(bitmapSource);
            //_image = EncodeToPng(bitmapSource);

            sw.Stop();
            Debug.WriteLine($"Jpeg: {_image.Length / 1024}KB, {sw.ElapsedMilliseconds}ms");
            //Debug.WriteLine($"Png: {_image.Length / 1024}KB, {sw.ElapsedMilliseconds}ms");

            Changed?.Invoke(this, null);
            RaisePropertyChanged(nameof(BitmapSource));
        }

        /// <summary>
        /// BitmapSource取得
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmap()
        {
            return IsValid ? DecodeFromJpeg(_image) : null;
            //return IsValid ? DecodeFromPng(_image) : null;
        }

        /// <summary>
        /// BitmapSource to Jpeg
        /// </summary>
        private byte[] EncodeToJpeg(BitmapSource source)
        {
            using (var stream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = Quality;
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
        /// BitmapSource to Png
        /// </summary>
        private byte[] EncodeToPng(BitmapSource source)
        {
            using (var stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.QualityLevel = Quality;
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);

                stream.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Png to BitmapSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private BitmapSource DecodeFromPng(byte[] image)
        {
            using (var stream = new MemoryStream(image, false))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
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
        }
    }
}
