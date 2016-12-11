// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using NeeLaboratory.Media;

namespace NeeLaboratory.Controls
{
    /// <summary>
    /// ColorPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        /// <summary>
        /// Color property
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.Black, ColorProperty_Changed));

        private static void ColorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorPicker;
            if (control != null)
            {
                control.Flush();
            }
        }




        public bool IsHsvMode
        {
            get { return (bool)GetValue(IsHsvModeProperty); }
            set { SetValue(IsHsvModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHsvMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHsvModeProperty =
            DependencyProperty.Register("IsHsvMode", typeof(bool), typeof(ColorPicker), new PropertyMetadata(false, IsHsvModeProperty_Changed));

        private static void IsHsvModeProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorPicker;
            if (control != null)
            {
                control.RaisePropertyChanged(nameof(IsRgbVisible));
                control.RaisePropertyChanged(nameof(IsHsvVisible));
            }
        }

        private bool _isPropertyLocked;

        private void Flush()
        {
            if (!_isPropertyLocked)
            {
                _rgb = Color;
                _hsv = Color.ToHSV();
            }
            RaisePropertyChanged(null);
        }

        private Color _rgb;

        /// <summary>
        /// Property: R
        /// </summary>
        public byte R
        {
            get { return _rgb.R; }
            set { UpdateColor(Color.FromArgb(_rgb.A, value, _rgb.G, _rgb.B)); }
        }

        /// <summary>
        /// Property: G
        /// </summary>
        public byte G
        {
            get { return _rgb.G; }
            set { UpdateColor(Color.FromArgb(_rgb.A, _rgb.R, value, _rgb.B)); }
        }

        /// <summary>
        /// Property: B
        /// </summary>
        public byte B
        {
            get { return _rgb.B; }
            set { UpdateColor(Color.FromArgb(_rgb.A, _rgb.R, _rgb.G, value)); }
        }



        public HSVColor _hsv;

        public int H
        {
            get { return (int)_hsv.H; }
            set
            {
                UpdateColor(HSVColor.FromHSV(_hsv.A, value, _hsv.S, _hsv.V));
            }
        }

        public double S
        {
            get { return _hsv.S; }
            set
            {
                UpdateColor(HSVColor.FromHSV(_hsv.A, _hsv.H, value, _hsv.V));
            }
        }

        public double V
        {
            get { return _hsv.V; }
            set
            {
                UpdateColor(HSVColor.FromHSV(_hsv.A, _hsv.H, _hsv.S, value));
            }
        }

        private void UpdateColor(Color rgb)
        {
            _rgb = rgb;
            _hsv = _rgb.ToHSV();
            UpdateColor();
        }

        private void UpdateColor(HSVColor hsv)
        {
            _hsv = hsv;
            _rgb = _hsv.ToARGB();
            UpdateColor();
        }

        private void UpdateColor()
        {
            _isPropertyLocked = true;
            Color = _rgb;
            _isPropertyLocked = false;
        }


        /// <summary>
        /// Property: IsRgbVisible
        /// </summary>
        public bool IsRgbVisible => !IsHsvMode;

        /// <summary>
        /// Property: IsHsvVisible
        /// </summary>
        public bool IsHsvVisible => IsHsvMode;


        /// <summary>
        /// 
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(value as string);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
