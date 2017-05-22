// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PagemarkPanel : BindableBase, IPanel
    {
        public string TypeCode => nameof(PagemarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "ページマーク";

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;

        //
        public PagemarkPanel(PagemarkList model)
        {
            View = new PagemarkListViewl(model);

            Icon = App.Current.MainWindow.Resources["pic_bookmark_24px"] as ImageSource;
            IconMargin = new Thickness(10);
        }
    }
}
