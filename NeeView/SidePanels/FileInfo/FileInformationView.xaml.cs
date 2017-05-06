﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
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
    /// FileInformationView.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInformationView : UserControl
    {
        private FileInformationViewModel _vm;

        //
        public FileInformationView()
        {
            InitializeComponent();
        }

        //
        public FileInformationView(FileInformation model) : this()
        { 
            _vm = new FileInformationViewModel(model);
            this.DataContext = _vm;
        }

        //
        private void Root_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsVisible = (bool)e.NewValue;
        }
    }
}