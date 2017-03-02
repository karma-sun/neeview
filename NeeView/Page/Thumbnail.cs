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
    /// <summary>
    /// サムネイル.
    /// Jpegで保持し、必要に応じてBitmapSourceを生成
    /// </summary>
    public class Thumbnail
    {
        /// <summary>
        /// 画像サイズ
        /// </summary>
        public static double Size { get; set; } = 256;

        /// <summary>
        /// 品質
        /// </summary>
        public static int Quality { get; set; } = 90;

        /// <summary>
        /// 有効判定
        /// </summary>
        internal bool IsValid => (_image != null);

        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        private byte[] _image;


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

            sw.Stop();
            Debug.WriteLine($"Jpeg: {_image.Length / 1024}KB, {sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// BitmapSource取得
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmap()
        {
            return IsValid ? DecodeFromJpeg(_image) : null;
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
    }
}
