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
    /// FolderIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderIcon : UserControl, INotifyPropertyChanged
    {
        public FolderItem FolderInfo
        {
            get { return (FolderItem)GetValue(FolderInfoProperty); }
            set { SetValue(FolderInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderInfoProperty =
            DependencyProperty.Register("FolderInfo", typeof(FolderItem), typeof(FolderIcon), new PropertyMetadata(null, FolderInfoChanged));

        private static void FolderInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FolderIcon;
            control?.Flush();
        }

        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        //
        public FolderIcon() 
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }

        //
        public void Flush()
        {
            RaisePropertyChanged(null);
        }
    }
}
