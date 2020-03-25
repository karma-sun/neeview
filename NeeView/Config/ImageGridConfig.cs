using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    public class ImageGridConfig : BindableBase
    {
        private bool _isEnabled;
        private Color _color = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
        private int _divX = 8;
        private int _divY = 8;
        private bool _isSquare;


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value);  }
        }

        [PropertyMember("@ParamGridLineColor"), DefaultValue(typeof(Color), "#80808080")]
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        [PropertyRange("@ParamGridLineDivX", 1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivX
        {
            get { return _divX; }
            set { SetProperty(ref _divX, value); }
        }

        [PropertyRange("@ParamGridLineDivY", 1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivY
        {
            get { return _divY; }
            set { SetProperty(ref _divY, value); }
        }

        [PropertyMember("@ParamGridLineIsSquare"), DefaultValue(false)]
        public bool IsSquare
        {
            get { return _isSquare; }
            set { SetProperty(ref _isSquare, value); }
        }
    }
}