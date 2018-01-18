// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// PointInspector.xaml の相互作用ロジック
    /// </summary>
    public partial class SizeInspector : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Size), typeof(SizeInspector), new PropertyMetadata(new Size(), SizePropertyChanged));

        private static void SizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SizeInspector)?.RaisePropertyChanged(null);
        }

        /// <summary>
        /// Size: Width
        /// </summary>
        public double X
        {
            get { return Size.Width; }
            set { if (Size.Width != value) { Size = new Size(value, Size.Height); RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Size: Height
        /// </summary>
        public double Y
        {
            get { return Size.Height; }
            set { if (Size.Height != value) { Size = new Size(Size.Width, value); RaisePropertyChanged(); } }
        }

        //
        public SizeInspector()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }
    }
}
