using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public enum PanelListItemImageShape
    {
        [AliasName("@EnumPanelListItemImageShapeOriginal")]
        Original,

        [AliasName("@EnumPanelListItemImageShapeSquare")]
        Square,

        [AliasName("@EnumPanelListItemImageShapeBookShape")]
        BookShape,

        [AliasName("@EnumPanelListItemImageShapeBanner")]
        Banner,
    }

    /// <summary>
    /// リスト項目の表示形式
    /// </summary>
    [DataContract]
    public class PanelListItemProfile : BindableBase
    {
        public static PanelListItemProfile DefaultNormalItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 0, false, true, false, 0.0);
        public static PanelListItemProfile DefaultContentItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 64, true, true, false, 0.5);
        public static PanelListItemProfile DefaultBannerItemProfile = new PanelListItemProfile(PanelListItemImageShape.Banner, 200, false, true, false, 0.0);
        public static PanelListItemProfile DefaultThumbnailItemProfile = new PanelListItemProfile(PanelListItemImageShape.Original, 128, false, true, true, 0.0);


        private static Rect _rectDefault = new Rect(0, 0, 1, 1);
        private static Rect _rectBanner = new Rect(0, 0, 1, 0.6);
        private static SolidColorBrush _brushBanner = new SolidColorBrush(Color.FromArgb(0x20, 0x99, 0x99, 0x99));

        private PanelListItemImageShape _imageShape;
        private int _imageWidth;
        private bool _isImagePopupEnabled;
        private bool _isTextVisibled;
        private bool _isTextWrapped;
        private double _noteOpacity;
        private bool _isTextheightDarty = true;
        private double _textHeight = double.NaN;


        public PanelListItemProfile()
        {
        }

        public PanelListItemProfile(PanelListItemImageShape imageShape, int imageWidth, bool isImagePopupEnabled, bool isTextVisibled, bool isTextWrapped, double noteOpacity)
        {
            _imageShape = imageShape;
            _imageWidth = imageWidth;
            _isImagePopupEnabled = isImagePopupEnabled;
            _isTextVisibled = isTextVisibled;
            _isTextWrapped = isTextWrapped;
            _noteOpacity = noteOpacity;

            UpdateTextHeight();
        }


        #region 公開プロパティ

        [DataMember(EmitDefaultValue = false)]
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
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (SetProperty(ref _imageWidth, value))
                {
                    RaisePropertyChanged(nameof(ShapeWidth));
                    RaisePropertyChanged(nameof(ShapeHeight));
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public bool IsImagePopupEnabled
        {
            get { return _isImagePopupEnabled; }
            set { SetProperty(ref _isImagePopupEnabled, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        public bool IsTextVisibled
        {
            get { return _isTextVisibled; }
            set { SetProperty(ref _isTextVisibled, value); }
        }

        [DataMember(EmitDefaultValue = false)]
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

        [DataMember(EmitDefaultValue = false)]
        public double NoteOpacity
        {
            get { return _noteOpacity; }
            set
            {
                if (SetProperty(ref _noteOpacity, value))
                {
                    RaisePropertyChanged(nameof(NoteVisibility));
                }
            }
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

        [PropertyMapIgnore]
        public Visibility NoteVisibility
        {
            get { return NoteOpacity > 0.0 ? Visibility.Visible : Visibility.Collapsed; }
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
                    FontSize = Config.Current.Panels.FontSize,
                };
                if (Config.Current.Panels.FontName != null)
                {
                    textBlock.FontFamily = new FontFamily(Config.Current.Panels.FontName);
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
