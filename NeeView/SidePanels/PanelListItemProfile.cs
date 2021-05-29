using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public enum PanelListItemImageShape
    {
        [AliasName]
        Original,

        [AliasName]
        Square,

        [AliasName]
        BookShape,

        [AliasName]
        Banner,
    }

    /// <summary>
    /// リスト項目の表示形式
    /// </summary>
    [DataContract]
    public class PanelListItemProfile : BindableBase
    {
        public static PanelListItemProfile DefaultNormalItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 0, false, true, false);
        public static PanelListItemProfile DefaultContentItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 64, true, true, false);
        public static PanelListItemProfile DefaultBannerItemProfile = new PanelListItemProfile(PanelListItemImageShape.Banner, 200, false, true, false);
        public static PanelListItemProfile DefaultThumbnailItemProfile = new PanelListItemProfile(PanelListItemImageShape.Original, 128, false, true, true);

        private static Rect _rectDefault = new Rect(0, 0, 1, 1);
        private static Rect _rectBanner = new Rect(0, 0, 1, 0.6);
        private static SolidColorBrush _brushBanner = new SolidColorBrush(Color.FromArgb(0x20, 0x99, 0x99, 0x99));

        private PanelListItemImageShape _imageShape;
        private int _imageWidth;
        private bool _isImagePopupEnabled;
        private bool _isTextVisible;
        private bool _isTextWrapped;
        private bool _isTextheightDarty = true;
        private double _textHeight = double.NaN;


        public PanelListItemProfile()
        {
        }

        public PanelListItemProfile(PanelListItemImageShape imageShape, int imageWidth, bool isImagePopupEnabled, bool isTextVisibled, bool isTextWrapped)
        {
            _imageShape = imageShape;
            _imageWidth = imageWidth;
            _isImagePopupEnabled = isImagePopupEnabled;
            _isTextVisible = isTextVisibled;
            _isTextWrapped = isTextWrapped;

            UpdateTextHeight();
        }


        #region 公開プロパティ

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember]
        public PanelListItemImageShape ImageShape
        {
            get { return _imageShape; }
            set
            {
                if (_imageShape != value)
                {
                    _imageShape = value;
                    RaisePropertyChanged(null);
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyRange(64, 512, TickFrequency = 8, IsEditable = true, Format = "{0} × {0}")]
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (SetProperty(ref _imageWidth, Math.Max(0, value)))
                {
                    RaisePropertyChanged(nameof(ShapeWidth));
                    RaisePropertyChanged(nameof(ShapeHeight));
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember]
        public bool IsImagePopupEnabled
        {
            get { return _isImagePopupEnabled; }
            set { SetProperty(ref _isImagePopupEnabled, value); }
        }

        [DataMember(Name = "IsTextVisibled", EmitDefaultValue = false)]
        [PropertyMember]
        public bool IsTextVisible
        {
            get { return _isTextVisible; }
            set { SetProperty(ref _isTextVisible, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember]
        public bool IsTextWrapped
        {
            get { return _isTextWrapped; }
            set
            {
                if (SetProperty(ref _isTextWrapped, value))
                {
                    UpdateTextHeight();
                }
            }
        }

        #endregion

        #region Obsolete

        [Obsolete, Alternative("Panel.Note in the custom theme file", 39, IsFullName = true)] // ver.39
        [JsonIgnore]
        public double NoteOpacity
        {
            get { return 0.0; }
            set { }
        }

        #endregion

        #region 非公開プロパティ

        [PropertyMapIgnore]
        public int ShapeWidth
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return _imageWidth;
                    case PanelListItemImageShape.BookShape:
                        return (int)(_imageWidth * 0.7071);
                }
            }
        }

        [PropertyMapIgnore]
        public int ShapeHeight
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return _imageWidth;
                    case PanelListItemImageShape.Banner:
                        return _imageWidth / 4;
                }
            }
        }

        [PropertyMapIgnore]
        public Rect Viewbox
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return _rectDefault;
                    case PanelListItemImageShape.Banner:
                        return _rectBanner;
                }
            }
        }

        [PropertyMapIgnore]
        public AlignmentY AlignmentY
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return AlignmentY.Top;
                    case PanelListItemImageShape.Original:
                        return AlignmentY.Bottom;
                    case PanelListItemImageShape.Banner:
                        return AlignmentY.Center;
                }
            }
        }

        [PropertyMapIgnore]
        public Brush Background
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return null;
                    case PanelListItemImageShape.Banner:
                        return _brushBanner;
                }
            }
        }

        [PropertyMapIgnore]
        public Stretch ImageStretch
        {
            get
            {
                switch (_imageShape)
                {
                    default:
                        return Stretch.UniformToFill;
                    case PanelListItemImageShape.Original:
                        return Stretch.Uniform;
                }
            }
        }

        [PropertyMapIgnore]
        public double TextHeight
        {
            get
            {
                if (_isTextheightDarty)
                {
                    _isTextheightDarty = false;
                    _textHeight = CalcTextHeight();
                }
                return _textHeight;
            }
        }

        #endregion


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _textHeight = double.NaN;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            UpdateTextHeight();
        }


        public PanelListItemProfile Clone()
        {
            var profile = ObjectExtensions.DeepCopy(this);
            profile.UpdateTextHeight();
            return profile;
        }

        // TextHeightの更新要求
        public void UpdateTextHeight()
        {
            _isTextheightDarty = true;
            RaisePropertyChanged(nameof(TextHeight));
        }

        // calc 2 line textbox height
        private double CalcTextHeight()
        {
            if (IsTextWrapped)
            {
                // 実際にTextBlockを作成して計算する
                var textBlock = new TextBlock()
                {
                    Text = "Age\nBusy",
                    FontSize = FontParameters.Current.PaneFontSize,
                };
                if (FontParameters.Current.DefaultFontName != null)
                {
                    textBlock.FontFamily = new FontFamily(FontParameters.Current.DefaultFontName);
                };
                var panel = new StackPanel();
                panel.Children.Add(textBlock);
                var area = new Size(256, 256);
                panel.Measure(area);
                panel.Arrange(new Rect(area));
                //panel.UpdateLayout();
                double height = (int)textBlock.ActualHeight + 1.0;

                return height;
            }
            else
            {
                return double.NaN;
            }
        }
    }
}
