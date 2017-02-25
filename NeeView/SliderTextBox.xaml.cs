﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    /// SliderTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SliderTextBox : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //
        public event EventHandler ValueChanged;

        //
        public Slider Target
        {
            get { return (Slider)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetSlider.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(Slider), typeof(SliderTextBox), new PropertyMetadata(null, TargetPropertyChanged));

        private static void TargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SliderTextBox)?.UpdateTarget();
        }


        /// <summary>
        /// DispText property.
        /// </summary>
        private string _DispText;
        public string DispText
        {
            get { return _DispText; }
            set { if (_DispText != value) { _DispText = value; RaisePropertyChanged(); } }
        }

        public SliderTextBox()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }

        private void UpdateTarget()
        {
            if (Target == null) return;

            // 注意：リークします
            DependencyPropertyDescriptor.FromProperty(Slider.MaximumProperty, typeof(Slider))
                .AddValueChanged(this.Target, (s, e) => Update());
            DependencyPropertyDescriptor.FromProperty(Slider.ValueProperty, typeof(Slider))
                .AddValueChanged(Target, (s, e) => UpdateDispText());

            Update();
        }

        private void Update()
        {
            int length = (int)Math.Log10(this.Target.Maximum) + 1;
            this.Root.MinWidth = (length * 2 + 1) * 7 + 10;

            UpdateDispText();
        }

        private void UpdateDispText()
        {
            this.DispText = Target != null ? $"{Target.Value + 1}/{Target.Maximum + 1}" : "";
        }


        private void SliderTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TextBox.Visibility = Visibility.Visible;
            this.TextBox.Focus();
        }


        private void SliderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSource();
            this.TextBox.Visibility = Visibility.Hidden;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TextBox.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                UpdateSource();
                this.TextBox.SelectAll();
                e.Handled = true;
            }
        }

        private void UpdateSource()
        {
            this.TextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ValueChanged?.Invoke(this, null);
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                this.Target.Value = this.Target.Value - 1;
            else
                this.Target.Value = this.Target.Value + 1;

            ValueChanged?.Invoke(this, null);
            this.TextBox.SelectAll();
            e.Handled = true;
        }
    }

    public class SliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return double.Parse((string)value) - 1;
            }
            catch
            {
                return value;
            }
        }
    }

}