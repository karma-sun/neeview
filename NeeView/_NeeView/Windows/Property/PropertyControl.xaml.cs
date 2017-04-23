// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// PropertyControl.xaml の相互作用ロジック
    /// </summary>
    [ContentProperty("Value")]
    public partial class PropertyControl : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public string Tips
        {
            get { return (string)GetValue(TipsProperty); }
            set { SetValue(TipsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tips.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TipsProperty =
            DependencyProperty.Register("Tips", typeof(string), typeof(PropertyControl), new PropertyMetadata(null));


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyControl), new PropertyMetadata(null));


        public double ColumnRate
        {
            get { return (double)GetValue(ColumnRateProperty); }
            set { SetValue(ColumnRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnRateProperty =
            DependencyProperty.Register("ColumnRate", typeof(double), typeof(PropertyControl), new PropertyMetadata(1.0 / 4.0, ColumnRateProperty_Changed));

        private static void ColumnRateProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyControl;
            if (control != null)
            {
                control.HeaderGridColumn.Width = new GridLength(control.ColumnRate, GridUnitType.Star);
                control.ValueGridColumn.Width = new GridLength((1.0 - control.ColumnRate), GridUnitType.Star);
            }
        }

        //
        public PropertyControl()
        {
            InitializeComponent();
        }
    }
}
