// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Bitmap生成モード
    /// </summary>
    public enum BitmapCreateMode
    {
        /// <summary>
        /// システム標準処理
        /// </summary>
        Default,

        /// <summary>
        /// MagicScalerでの処理
        /// </summary>
        HighQuality,
    }

    /// <summary>
    /// Bitmap出力画像フォーマット
    /// </summary>
    public enum BitmapImageFormat
    {
        Jpeg,
        Png,
    }

    /// <summary>
    /// Bitmap生成
    /// </summary>
    public class BitmapFactory
    {
        // 標準
        private DefaultBitmapFactory _default = new DefaultBitmapFactory();

        // MagicScaler
        private MagicScalerBitmapFactory _magicScaler = new MagicScalerBitmapFactory();

        //
        public MagicScalerBitmapFactory MagicScaler => _magicScaler;

        //
        public BitmapImage Create(Stream stream, BitmapInfo info, Size size, BitmapCreateSetting setting)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    return _magicScaler.Create(stream, info, size, setting.ProcessImageSettings);
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            return _default.Create(stream, info, size);
        }


        //
        public void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            // by MagicScaler
            if (!size.IsEmpty && setting.Mode == BitmapCreateMode.HighQuality)
            {
                try
                {
                    _magicScaler.CreateImage(stream, info, outStream, size, format, quality, setting.ProcessImageSettings);
                    return;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MagicScaler Failed:" + ex.Message);
                }
            }

            // by Default
            _default.CreateImage(stream, info, outStream, size, format, quality);
        }
    }

}
