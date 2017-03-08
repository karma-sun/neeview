// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public partial class FileInfoControl : UserControl
    {
        public FileInfoSetting Setting
        {
            get { return (FileInfoSetting)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Setting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(FileInfoSetting), typeof(FileInfoControl), new PropertyMetadata(new FileInfoSetting()));



        // コンストラクタ
        public FileInfoControl()
        {
            InitializeComponent();

            var descripter = DependencyPropertyDescriptor.FromProperty(UserControl.DataContextProperty, typeof(FileInfoControl));
            descripter.AddValueChanged(this, OnDataContextChanged);

            var descripter2 = DependencyPropertyDescriptor.FromProperty(FileInfoControl.SettingProperty, typeof(FileInfoControl));
            descripter2.AddValueChanged(this, OnDataContextChanged);
        }

        //
        private class StringSet
        {
            public string Size { get; set; }
            public string FileSize { get; set; }
            public string ShotInfo { get; set; }
            public string ISOSpeedRatings { get; set; }
            public string Model { get; set; }
            public string LastWriteTime { get; set; }
        }

        //
        private void SetString(StringSet strings)
        {
            this.ItemSize.Text = strings.Size;
            this.ItemFileSize.Text = strings.FileSize;
            this.ItemShotInfo.Text = strings.ShotInfo;
            this.ItemISOSpeedRatings.Text = strings.ISOSpeedRatings;
            this.ItemModel.Text = strings.Model;
            this.ItemLastWriteTime.Text = strings.LastWriteTime;
        }

        // コンテンツの切り替わりで内容更新
        private void OnDataContextChanged(object sender, EventArgs e)
        {
            if (Setting != null)
            {
                //this.GroupThumbnail.Visibility = Setting.IsVisibleThumbnail ? Visibility.Visible : Visibility.Collapsed;
                this.GroupLoader.Visibility = Setting.IsVisibleLoader ? Visibility.Visible : Visibility.Collapsed;
            }

            var strings = new StringSet();

            var content = (sender as FileInfoControl)?.DataContext as ViewContent;
            if (content != null)
            {
                try
                {
                    var bitmapContent = content.Content as BitmapContent;
                    if (bitmapContent?.BitmapInfo != null)
                    {
                        this.Thumbnail.Source = bitmapContent.Thumbnail.BitmapSource;

                        strings.Size = string.Format("{0} x {1}", bitmapContent.Size.Width, bitmapContent.Size.Height);
                        if (Setting.IsVisibleBitsPerPixel) strings.Size += string.Format(" ({0}bit)", bitmapContent.BitmapInfo.BitsPerPixel);

                        if (bitmapContent.BitmapInfo.Length >= 0)
                        {
                            strings.FileSize = string.Format("{0:#,0} KB", bitmapContent.BitmapInfo.Length > 0 ? (bitmapContent.BitmapInfo.Length + 1023) / 1024 : 0);
                        }
                        DateTime? lastWriteTime = bitmapContent.BitmapInfo.LastWriteTime;
                        if (bitmapContent.BitmapInfo.Exif != null)
                        {
                            var exif = bitmapContent.BitmapInfo.Exif;
                            strings.ShotInfo = exif.ShotInfo;
                            strings.ISOSpeedRatings = exif.ISOSpeedRatings > 0 ? exif.ISOSpeedRatings.ToString() : null;
                            strings.Model = exif.Model;

                            if (Setting.IsUseExifDateTime && exif.LastWriteTime != null) lastWriteTime = exif.LastWriteTime;
                        }
                        strings.LastWriteTime = lastWriteTime?.ToString("yyyy年M月d日 dddd H:mm");

                        this.ItemArchiver.Text = bitmapContent.BitmapInfo.Archiver;
                        this.ItemDecoder.Text = (bitmapContent is AnimatedContent) ? "MediaPlayer" : bitmapContent.BitmapInfo.Decoder;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            SetString(strings);
        }

        //
        private void OpenPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            var content = this.DataContext as ViewContent;
            if (content != null)
            {
                if (!string.IsNullOrWhiteSpace(content.FilePlace))
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + content.FilePlace + "\"");
                }
            }
        }
    }
}
