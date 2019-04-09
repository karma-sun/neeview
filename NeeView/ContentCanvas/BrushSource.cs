using NeeLaboratory.ComponentModel;
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
        [AliasName("@EnumBrushTypeSolidColor")]
        SolidColor,

        [AliasName("@EnumBrushTypeImageTile")]
        ImageTile,

        [AliasName("@EnumBrushTypeImageFill")]
        ImageFill,

        [AliasName("@EnumBrushTypeImageUniform")]
        ImageUniform,

        [AliasName("@EnumBrushTypeImageUniformToFill")]
        ImageUniformToFill,
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
        public static Dictionary<BrushType, string> BrushTypeList => AliasNameExtensions.GetAliasNameDictionary<BrushType>();

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
            if (string.IsNullOrEmpty(this.ImageFileName))
            {
                return Brushes.Transparent;
            }

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
