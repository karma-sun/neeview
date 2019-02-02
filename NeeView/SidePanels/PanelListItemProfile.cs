﻿using NeeLaboratory.ComponentModel;
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
    }

    /// <summary>
    /// リスト項目の表示形式
    /// </summary>
    [DataContract]
    public class PanelListItemProfile : BindableBase
    {
        private PanelListItemImageShape _imageShape;
        private int _imageWidth;
        private bool _isImagePopupEnabled;
        private bool _isTextVisibled;
        private bool _isTextWrapped;
        private double _noteOpacity;
        private bool _isTextheightDarty = true;

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


        [DataMember(EmitDefaultValue = false)]
        public PanelListItemImageShape ImageShape
        {
            get { return _imageShape; }
            set
            {
                if (SetProperty(ref _imageShape, value))
                {
                    RaisePropertyChanged(nameof(ShapeWidth));
                    RaisePropertyChanged(nameof(ShapeHeight));
                    RaisePropertyChanged(nameof(ImageStretch));
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
                    RaisePropertyChanged(nameof(ImageHeightQuarter));
                }
            }
        }

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

        public int ShapeHeight
        {
            get { return _imageWidth; }
        }

        /// <summary>
        /// バナー用縦幅
        /// </summary>
        public int ImageHeightQuarter
        {
            get { return _imageWidth / 4; }
        }

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


        private double _textHeight = double.NaN;
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
            set
            {
                SetProperty(ref _textHeight, value);
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

        public Visibility NoteVisibility
        {
            get { return NoteOpacity > 0.0 ? Visibility.Visible : Visibility.Collapsed; }
        }


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
                    FontSize = SidePanelProfile.Current.FontSize,
                };
                if (SidePanelProfile.Current.FontName != null)
                {
                    textBlock.FontFamily = new FontFamily(SidePanelProfile.Current.FontName);
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