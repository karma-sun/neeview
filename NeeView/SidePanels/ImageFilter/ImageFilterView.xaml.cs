// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
    /// FilterView.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageFilterView : UserControl
    {
        private ImageFilterViewModel _vm;

        // コンストラクタ
        public ImageFilterView()
        {
            InitializeComponent();
        }

        //
        public ImageFilterView(ImageFilter model) : this()
        {
            InitializeComponent();

            _vm = new ImageFilterViewModel(model);
            this.DataContext = _vm;
        }

        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }
    }
}
