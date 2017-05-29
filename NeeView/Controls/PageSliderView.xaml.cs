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

namespace NeeView
{
    /// <summary>
    /// PageSliderView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSliderView : UserControl
    {
        public PageSlider Source
        {
            get { return (PageSlider)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PageSlider), typeof(PageSliderView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageSliderView control)
            {
                control.Initialize();
            }
        }

        public UIElement FocusTo
        {
            get { return (UIElement)GetValue(FocusToProperty); }
            set { SetValue(FocusToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusTo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusToProperty =
            DependencyProperty.Register("FocusTo", typeof(UIElement), typeof(PageSliderView), new PropertyMetadata(null));


        // 
        private PageSliderViewModel _vm;


        /// <summary>
        /// constructor
        /// </summary>
        public PageSliderView()
        {
            InitializeComponent();
        }


        //
        public void Initialize()
        {
            if (this.Source == null) return;

            _vm = new PageSliderViewModel(this.Source);
            this.Root.DataContext = _vm;

            // マーカー初期化
            this.PageMarkersView.Source = this.Source.PageMarkers;

            // 
            _vm.Model.AddPropertyChanged(nameof(PageSlider.IsSliderDirectionReversed), Model_IsSliderDirectionReversedChanged);
        }


        // スライダーの方向切替反映
        public void Model_IsSliderDirectionReversedChanged(object sender, PropertyChangedEventArgs e)
        {
            // Retrieve the Track from the Slider control
            var track = this.Slider.Template.FindName("PART_Track", this.Slider) as System.Windows.Controls.Primitives.Track;
            // Force it to rerender
            track.InvalidateVisual();
        }
        

        /// <summary>
        /// スライダーエリアでのマウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _vm.MouseWheel(sender, e);
        }

        //
        private void PageSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _vm.Decide(false);
        }

        //
        private void PageSliderTextBox_ValueChanged(object sender, EventArgs e)
        {
            _vm.Decide(true);
        }


        // テキストボックス入力時に単キーのショートカットを無効にする
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカット無効
            KeyExGesture.AllowSingleKey = false;
            //e.Handled = true;
        }

    }
}

