﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Thumbnail の BitmapSource化
    /// </summary>
    public class ThumbnailBitmap : BindableBase
    {
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set { if (_imageSource != value) { _imageSource = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Thumbnail property.
        /// </summary>
        private Thumbnail _thumbnail;
        public Thumbnail Thumbnail
        {
            get { return _thumbnail; }
            set { Set(value); }
        }

        /// <summary>
        /// Thumbnail設定
        /// </summary>
        /// <param name="thumbnail"></param>
        public void Set(Thumbnail thumbnail)
        {
            if (_thumbnail == thumbnail) return;

            if (_thumbnail != null)
            {
                _thumbnail.Changed -= Thumbnail_Changed;
                _thumbnail = null;
            }

            _thumbnail = thumbnail;

            if (_thumbnail != null)
            {
                UpdateBitmapSourceAsync();
                _thumbnail.Changed += Thumbnail_Changed;
            }
            else
            {
                ImageSource = null;
            }

            RaisePropertyChanged(nameof(Thumbnail));
        }

        /// <summary>
        /// Thumbnail開放
        /// </summary>
        public void Reset()
        {
            Set(null);
        }

        /// <summary>
        /// Thumbnail変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumbnail_Changed(object sender, EventArgs e)
        {
            UpdateBitmapSourceAsync();
        }

        /// <summary>
        /// BitmapSource更新
        /// </summary>
        private async void UpdateBitmapSourceAsync()
        {
            if (Thumbnail != null && Thumbnail.IsValid)
            {
                // BitmapSource生成 (非同期)
                ImageSource = await Task.Run(() => Thumbnail?.ImageSource);
            }
        }
    }
}
