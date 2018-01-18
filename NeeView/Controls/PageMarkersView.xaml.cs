// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    /// PageMakers : View
    /// </summary>
    public partial class PageMarkersView : UserControl
    {
        public PageMarkers Source
        {
            get { return (PageMarkers)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PageMarkers), typeof(PageMarkersView), new PropertyMetadata(null, SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PageMarkersView)?.Initialize();
        }


        //
        private PageMarkersViewModel _vm;

        //
        public PageMarkersView()
        {
            InitializeComponent();
        }

        //
        private void Initialize()
        {
            _vm = new PageMarkersViewModel(this.Source, this.RootCanvas);
            this.DataContext = _vm;
        }
    }
}
