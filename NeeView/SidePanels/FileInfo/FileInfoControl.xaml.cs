// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ファイル情報ペイン設定
    /// </summary>
    [DataContract]
    public class FileInfoSetting
    {
        [DataMember]
        public bool IsUseExifDateTime { get; set; }

        [DataMember]
        public bool IsVisibleBitsPerPixel { get; set; }

        [DataMember]
        public bool IsVisibleLoader { get; set; }

        [DataMember]
        public Dock Dock { get; set; }

        //
        private void Constructor()
        {
            IsUseExifDateTime = true;
            Dock = Dock.Right;
        }

        public FileInfoSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
        public FileInfoSetting Clone()
        {
            return (FileInfoSetting)MemberwiseClone();
        }
    }


    /// <summary>
    /// FileInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInfoControl : UserControl, INotifyPropertyChanged
    {
        public FileInfoControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Setting property.
        /// </summary>
        private FileInfoSetting _Setting;
        public FileInfoSetting Setting
        {
            get { return _Setting; }
            set { if (_Setting != value) { _Setting = value; _isDarty = true; Update(); RaisePropertyChanged(); } }
        }


        /// <summary>
        /// ViewContent property.
        /// </summary>
        private ViewContent _viewContent;
        public ViewContent ViewContent
        {
            get { return _viewContent; }
            set { if (_viewContent != value) { _viewContent = value; _isDarty = true;  Update(); RaisePropertyChanged(); } }
        }

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
        /// Model property.
        /// </summary>
        private string _Model;
        public string Model
        {
            get { return _Model; }
            set { if (_Model != value) { _Model = value; RaisePropertyChanged(); } }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Root_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update();
        }

        //
        private bool _isDarty;


        // コンテンツの切り替わりで内容更新
        public void Update()
        {
            if (!_isDarty) return;

            if (Setting != null)
            {
                LoaderVisibility = Setting.IsVisibleLoader ? Visibility.Visible : Visibility.Collapsed;
            }

            var bitmapContent = IsVisible ? _viewContent?.Content as BitmapContent : null;

            if (bitmapContent?.BitmapInfo != null)
            {
                _isDarty = false;

                // サムネイル設定
                ThumbnailBitmap.Set(bitmapContent.Thumbnail);

                // 画像サイズ表示
                ImageSize = $"{bitmapContent.Size.Width} x {bitmapContent.Size.Height}" + (Setting.IsVisibleBitsPerPixel ? $" ({bitmapContent.BitmapInfo.BitsPerPixel}bit)" : "");

                // ファイルサイズ表示
                FileSize = bitmapContent.BitmapInfo.Length >= 0 ? string.Format("{0:#,0} KB", bitmapContent.BitmapInfo.Length > 0 ? (bitmapContent.BitmapInfo.Length + 1023) / 1024 : 0) : null;

                // EXIF
                var exif = bitmapContent.BitmapInfo.Exif;

                // EXIF: ShotInfo
                ShotInfo = exif?.ShotInfo;

                // EXIF: ISO Speed Raging
                ISOSpeedRatings = exif != null && exif.ISOSpeedRatings > 0 ? exif.ISOSpeedRatings.ToString() : null;

                // EXIF: Model
                Model = exif?.Model;

                // 更新日
                DateTime? lastWriteTime = (Setting.IsUseExifDateTime && exif?.LastWriteTime != null)
                    ? exif.LastWriteTime
                    : bitmapContent.BitmapInfo.LastWriteTime;
                LastWriteTime = lastWriteTime?.ToString("yyyy年M月d日 dddd H:mm");

                // アーカイバー
                Archiver = bitmapContent.BitmapInfo.Archiver;

                // デコーダ
                Decoder = (bitmapContent is AnimatedContent) ? "MediaPlayer" : bitmapContent.BitmapInfo.Decoder;
            }
            else
            {
                ThumbnailBitmap.Reset();
                ImageSize = null;
                FileSize = null;
                ShotInfo = null;
                ISOSpeedRatings = null;
                Model = null;
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
            if (_viewContent != null)
            {
                var place = _viewContent.Page?.GetFilePlace();
                if (!string.IsNullOrWhiteSpace(place))
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }

    }
}
