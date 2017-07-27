// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    //
    public class Picture : BindableBase
    {
        private ArchiveEntry _archiveEntry;

        //
        public Picture(ArchiveEntry entry)
        {
            _archiveEntry = entry;

            this.PictureInfo = new PictureInfo(entry);
        }

        //
        public PictureInfo PictureInfo { get; set; }

        // 未使用
        //private byte[] RawData { get; set; }

        /// <summary>
        /// BitmapSource property.
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }


        // Bitmapが同じサイズであるか判定
        private bool IsEqualBitmapSizeMaybe(Size size)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.0;
            return Math.Abs(size.Width - this.BitmapSource.PixelWidth) < margin && Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin;
        }


        // リサイズ
        public void Resize(Size size)
        {
            size = PictureFactory.Current.CreateFixedSize(_archiveEntry, size.IsEmpty ? this.PictureInfo.Size : size);
            if (IsEqualBitmapSizeMaybe(size)) return;

            // 規定サイズ判定
            if (size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            this.BitmapSource = PictureFactory.Current.CreateBitmapSource(_archiveEntry, size);
        }
    }

}
