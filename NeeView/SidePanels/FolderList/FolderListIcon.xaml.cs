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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderListIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListIcon : UserControl
    {
        public Visibility FolderVisibility
        {
            get { return (Visibility)GetValue(FolderVisibilityProperty); }
            set { SetValue(FolderVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderVisibilityProperty =
            DependencyProperty.Register("FolderVisibility", typeof(Visibility), typeof(FolderListIcon), new PropertyMetadata(Visibility.Visible, FolderVisibilityChanged));

        private static void FolderVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderListIcon control)
            {
                control.Folder.Visibility = control.FolderVisibility;
            }
        }

        
        /// <summary>
        /// constructor
        /// </summary>
        public FolderListIcon()
        {
            InitializeComponent();
        }

    }
}
