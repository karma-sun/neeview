// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
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
        ImageFill, // 画像を拡大して表示
        ImageUniform, // 画像をウインドウサイズに合わせる
        ImageUniformToFill, // 縦横比をウインドウいっぱいに広げる
    }

    /// <summary>
    /// ブラシ構成要素.
    /// 単色ブラシ、画像タイルブラシ対応
    /// </summary>
    [DataContract]
    public class BrushSource : BindableBase
    {
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
        public static Dictionary<BrushType, string> BrushTypeList { get; } = new Dictionary<BrushType, string>()
        {
            [BrushType.SolidColor] = "単色",
            [BrushType.ImageTile] = "画像タイル",
            [BrushType.ImageFill] = "画像を拡大して表示",
            [BrushType.ImageUniform] = "画像をウインドウサイズに合わせる",
            [BrushType.ImageUniformToFill] = "画像をウインドウいっぱいに広げる",
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

#if false
        /// <summary>
        /// Opacity property.
        /// </summary>
        private double _Opacity = 1.0;
        [DataMember]
        public double Opacity
        {
            get { return _Opacity; }
            set { if (_Opacity != value) { _Opacity = value; RaisePropertyChanged(); } }
        }
#endif

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
        /// 
        /// </summary>
        /// <returns></returns>
        public Brush CreateBackBrush()
        {
            return new SolidColorBrush(Color);
        }

        /// <summary>
        /// ブラシを作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateFrontBrush()
        {
            switch (Type)
            {
                default:
                case BrushType.SolidColor:
                    return null;
                case BrushType.ImageTile:
                case BrushType.ImageFill:
                case BrushType.ImageUniform:
                case BrushType.ImageUniformToFill:
                    return CreateImageBrush(Type);
            }
        }

        /// <summary>
        /// タイルブラシを作成
        /// </summary>
        /// <returns></returns>
        private Brush CreateImageBrush(BrushType type)
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
                switch (type)
                {
                    case BrushType.ImageTile:
                        brush.AlignmentX = AlignmentX.Left;
                        brush.AlignmentY = AlignmentY.Top;
                        brush.Viewport = new Rect(0, 0, bmpImage.PixelWidth, bmpImage.PixelHeight);
                        brush.ViewportUnits = BrushMappingMode.Absolute;
                        brush.Stretch = Stretch.Fill;
                        brush.TileMode = TileMode.Tile;
                        break;
                    case BrushType.ImageFill:
                        brush.Stretch = Stretch.Fill;
                        break;
                    case BrushType.ImageUniform:
                        brush.Stretch = Stretch.Uniform;
                        break;
                    case BrushType.ImageUniformToFill:
                        brush.Stretch = Stretch.UniformToFill;
                        break;
                }

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
            clone.ResetPropertyChanged();
            return clone;
        }
    }
}
