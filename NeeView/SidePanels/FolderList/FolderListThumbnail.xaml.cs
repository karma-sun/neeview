﻿using System;
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
    /// FolderListThumbnail.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListThumbnail : UserControl
    {
        public FolderListThumbnail()
        {
            InitializeComponent();
        }

        private void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            e.Handled = !ThumbnailProfile.Current.IsThumbnailPopup;
        }
    }

}
