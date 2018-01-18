// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// FileInformation : ViewModel
    /// </summary>
    public class FileInformationViewModel : BindableBase
    {
        /// <summary>
        /// Model property.
        /// </summary>
        public FileInformation Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private FileInformation _model;



        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public FileInformationViewModel(FileInformation model)
        {
            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;
        }


        //
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case nameof(_model.ViewContent):
                case nameof(_model.IsUseExifDateTime):
                case nameof(_model.IsVisibleBitsPerPixel):
                case nameof(_model.IsVisibleLoader):
                    _isDarty = true;
                    Update();
                    break;
            }
        }

        /// <summary>
        /// 表示状態。
        /// 非表示での情報更新はデータクリアを行う
        /// 表示状態になったときにデータを再構築する
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { if (_isVisible != value) { _isVisible = value; RaisePropertyChanged(); Update(); } }
        }

        private bool _isVisible;



        /// <summary>
        /// Thumbnail Bitmap
        /// </summary>
        public ThumbnailBitmap ThumbnailBitmap { get; set; } = new ThumbnailBitmap();


        /// <summary>
        /// ImageSize property.
        /// </summary>
        private string _ImageSize;
        public string ImageSize
        {
            get { return _ImageSize; }
            set { if (_ImageSize != value) { _ImageSize = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// FileSize property.
        /// </summary>
        private string _FileSize;
        public string FileSize
        {
            get { return _FileSize; }
            set { if (_FileSize != value) { _FileSize = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ShotInfo property.
        /// </summary>
        private string _ShotInfo;
        public string ShotInfo
        {
            get { return _ShotInfo; }
            set { if (_ShotInfo != value) { _ShotInfo = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ISOSpeedRatings property.
        /// </summary>
        private string _ISOSpeedRatings;
        public string ISOSpeedRatings
        {
            get { return _ISOSpeedRatings; }
            set { if (_ISOSpeedRatings != value) { _ISOSpeedRatings = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// CameraModel property.
        /// </summary>
        private string _CameraModel;
        public string CameraModel
        {
            get { return _CameraModel; }
            set { if (_CameraModel != value) { _CameraModel = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// LastWriteTime property.
        /// </summary>
        private string _LastWriteTime;
        public string LastWriteTime
        {
            get { return _LastWriteTime; }
            set { if (_LastWriteTime != value) { _LastWriteTime = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// LoaderVisibility property.
        /// </summary>
        private Visibility _LoaderVisibility = Visibility.Collapsed;
        public Visibility LoaderVisibility
        {
            get { return _LoaderVisibility; }
            set { if (_LoaderVisibility != value) { _LoaderVisibility = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// Archiver property.
        /// </summary>
        private string _Archiver;
        public string Archiver
        {
            get { return _Archiver; }
            set { if (_Archiver != value) { _Archiver = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Decoder property.
        /// </summary>
        private string _Decoder;
        public string Decoder
        {
            get { return _Decoder; }
            set { if (_Decoder != value) { _Decoder = value; RaisePropertyChanged(); } }
        }


        //
        private bool _isDarty;


        // コンテンツの切り替わりで内容更新
        public void Update()
        {
            if (!_isDarty) return;

            LoaderVisibility = _model.IsVisibleLoader ? Visibility.Visible : Visibility.Collapsed;

            var bitmapContent = IsVisible ? _model.ViewContent?.Content as BitmapContent : null;

            var info = bitmapContent?.Picture?.PictureInfo;
            if (info != null)
            {
                //Debug.WriteLine($"FileInfo: {_model.ViewContent?.FileName}");

                _isDarty = false;

                // サムネイル設定
                ThumbnailBitmap.Set(bitmapContent.Thumbnail);

                // 画像サイズ表示
                ImageSize = $"{info.OriginalSize.Width} x {info.OriginalSize.Height}" + (info.IsLimited ? "*" : "") + (_model.IsVisibleBitsPerPixel ? $" ({info.BitsPerPixel}bit)" : "");

                // ファイルサイズ表示
                FileSize = info.Length > 0 ? string.Format("{0:#,0} KB", info.Length > 0 ? (info.Length + 1023) / 1024 : 0) : null;

                // EXIF
                var exif = info.Exif;

                // EXIF: ShotInfo
                ShotInfo = exif?.ShotInfo;

                // EXIF: ISO Speed Raging
                ISOSpeedRatings = exif != null && exif.ISOSpeedRatings > 0 ? exif.ISOSpeedRatings.ToString() : null;

                // EXIF: Model
                CameraModel = exif?.Model;

                // 更新日
                DateTime? lastWriteTime = (_model.IsUseExifDateTime && exif?.LastWriteTime != null)
                    ? exif.LastWriteTime
                    : info.LastWriteTime;
                LastWriteTime = lastWriteTime?.ToString("yyyy年M月d日 dddd H:mm");

                // アーカイバー
                Archiver = info.Archiver;

                // デコーダ
                Decoder = (bitmapContent is AnimatedContent) ? "MediaPlayer" : info.Decoder;
            }
            else
            {
                //Debug.WriteLine($"FileInfo: null");

                ThumbnailBitmap.Reset();
                ImageSize = null;
                FileSize = null;
                ShotInfo = null;
                ISOSpeedRatings = null;
                CameraModel = null;
                LastWriteTime = null;
                Archiver = null;
                Decoder = null;
            }
        }

        /// <summary>
        /// OpenPlace command.
        /// </summary>
        private RelayCommand _OpenPlace;
        public RelayCommand OpenPlace
        {
            get { return _OpenPlace = _OpenPlace ?? new RelayCommand(OpenPlace_Executed); }
        }

        private void OpenPlace_Executed()
        {
            if (_model.ViewContent != null)
            {
                var place = _model.ViewContent.Page?.GetFolderOpenPlace();
                if (!string.IsNullOrWhiteSpace(place))
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }

    }
}
