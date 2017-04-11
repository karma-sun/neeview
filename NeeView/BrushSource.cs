// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// ブラシの種類
    /// </summary>
    public enum BrushType
    {
        SolidColor, // 単色
        ImageTile, // 画像タイル
    }

    /// <summary>
    /// ブラシ構成要素.
    /// 単色ブラシ、画像タイルブラシ対応
    /// </summary>
    [DataContract]
    public class BrushSource : INotifyPropertyChanged
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
        /// BackgroundBrushType property.
        /// </summary>
        private BrushType _Type;
        [DataMember]
        public BrushType Type
        {
            get { return _Type; }
            set { if (_Type != value) { _Type = value; RaisePropertyChanged(); } }
        }

        //
        public static Dictionary<BrushType, string> BackgroundBrushTypeList { get; } = new Dictionary<BrushType, string>()
        {
            [BrushType.SolidColor] = "背景色",
            [BrushType.ImageTile] = "画像タイル",
        };

        /// <summary>
        /// Color property.
        /// </summary>
        private Color _Color;
        [DataMember]
        public Color Color
        {
            get { return _Color; }
            set { if (_Color != value) { _Color = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ImageFileName property.
        /// </summary>
        private string _ImageFileName;
        [DataMember]
        public string ImageFileName
        {
            get { return _ImageFileName; }
            set { if (_ImageFileName != value) { _ImageFileName = value; RaisePropertyChanged(); } }
        }
        
        /// <summary>
        /// Brush property.
        /// </summary>
        private Brush _Brush;
        public Brush Brush
        {
            get { return _Brush; }
            set { if (_Brush != value) { _Brush = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public BrushSource()
        {
            _Type = BrushType.SolidColor;
            _Color = Colors.LightGray;
        }

        /// <summary>
        /// ブラシを作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateBrush()
        {
            switch (Type)
            {
                case BrushType.SolidColor:
                    return new SolidColorBrush(Color);
                case BrushType.ImageTile:
                    return CreateImageTileBrush();
                default:
                    return Brushes.LightGray;
            }
        }

        /// <summary>
        /// タイルブラシを作成
        /// </summary>
        /// <returns></returns>
        private Brush CreateImageTileBrush()
        {
            try
            {
                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.UriSource = new Uri(ImageFileName);
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();
                bmpImage.Freeze();

                var brush = new ImageBrush(bmpImage);
                brush.AlignmentX = AlignmentX.Left;
                brush.AlignmentY = AlignmentY.Top;
                brush.Viewport = new Rect(0, 0, bmpImage.PixelWidth, bmpImage.PixelHeight);
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Stretch = Stretch.Fill;
                brush.TileMode = TileMode.Tile;

                return brush;
            }
            catch
            {
                return Brushes.LightGray;
            }
        }

        /// <summary>
        /// 複製
        /// </summary>
        /// <returns></returns>
        public BrushSource Clone()
        {
            var clone = (BrushSource)MemberwiseClone();
            clone.PropertyChanged = null;
            return clone;
        }
    }
}
