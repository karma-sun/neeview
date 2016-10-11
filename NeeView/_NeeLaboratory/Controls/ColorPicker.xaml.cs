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

namespace NeeLaboratory.Controls
{
    /// <summary>
    /// ColorPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
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
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.Black));


        /// <summary>
        /// Property: R
        /// </summary>
        public byte R
        {
            get { return Color.R; }
            set { if (Color.R != value) { Color = Color.FromArgb(Color.A, value, Color.G, Color.B); OnPropertyChanged(); } }
        }

        /// <summary>
        /// Property: G
        /// </summary>
        public byte G
        {
            get { return Color.G; }
            set { if (Color.G != value) { Color = Color.FromArgb(Color.A, Color.R, value, Color.B); OnPropertyChanged(); } }
        }

        /// <summary>
        /// Property: B
        /// </summary>
        public byte B
        {
            get { return Color.B; }
            set { if (Color.B != value) { Color = Color.FromArgb(Color.A, Color.R, Color.G, value); OnPropertyChanged(); } }
        }



        /// <summary>
        /// 
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }
    }



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
