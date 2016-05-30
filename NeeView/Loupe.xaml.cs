// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace NeeView
{
    /// <summary>
    /// Loupe.xaml の相互作用ロジック
    /// </summary>
    public partial class Loupe : UserControl
    {
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(double), typeof(Loupe), new PropertyMetadata(100.0, (d, e) => { }));

        //
        public static void SizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Loupe)d).Update();
        }


        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(Loupe), new PropertyMetadata(2.0, ScalePropertyChanged));

        //
        public static void ScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Loupe)d).Update();
        }



        public UIElement Visual
        {
            get { return (UIElement)GetValue(VisualProperty); }
            set { SetValue(VisualProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visual.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualProperty =
            DependencyProperty.Register("Visual", typeof(UIElement), typeof(Loupe), new PropertyMetadata(null, VisualPropertyChanged));

        //
        public static void VisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = (Loupe)d;
            if (e.OldValue != null)
            {
                var target = (UIElement)e.OldValue;
                target.MouseMove -= my.OnMouseMoveAtVisual;
                target.MouseLeave -= my.OnMouseMoveAtVisual;
            }
            if (e.NewValue != null)
            {
                var target = (UIElement)e.NewValue;
                target.MouseMove += my.OnMouseMoveAtVisual;
                target.MouseLeave += my.OnMouseMoveAtVisual;
            }
        }


        //
        public Loupe()
        {
            InitializeComponent();

            this.Root.DataContext = this;

            var descripter = DependencyPropertyDescriptor.FromProperty(Loupe.SizeProperty, typeof(Loupe));
            descripter.AddValueChanged(this, (s, e) => Update());
        }

        //
        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible) Update();
        }

        //
        private void OnMouseMoveAtVisual(object sender, MouseEventArgs e)
        {
            Update();
        }

        //
        private void Update()
        {
            if (!this.IsVisible || Visual == null) return;

            // Visibility
            this.LoupeMain.Visibility = Visual.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;

            // Position
            var pos = Mouse.GetPosition(Visual);
            Canvas.SetLeft(this.LoupeMain, pos.X - Size * 0.5);
            Canvas.SetTop(this.LoupeMain, pos.Y - Size * 0.5);

            // Viewbox
            var sourceSize = Size / Scale;
            var viewbox = new Rect()
            {
                X = pos.X - sourceSize * 0.5,
                Y = pos.Y - sourceSize * 0.5,
                Width = sourceSize,
                Height = sourceSize,
            };
            this.LoupeBrush.Viewbox = viewbox;

            // Viewport
            var viewport = new Rect()
            {
                X = 0,
                Y = 0,
                Width = Size,
                Height = Size,
            };
            this.LoupeBrush.Viewport = viewport;
        }

    }
}
