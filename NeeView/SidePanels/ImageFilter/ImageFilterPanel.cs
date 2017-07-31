// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ImageFilter : Panel
    /// </summary>
    public class ImageFilterPanel : BindableBase, IPanel
    {
        public string TypeCode => nameof(ImageFilterPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "フィルター";

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        //
        public ImageFilterPanel(ImageFilter model)
        {
            View = new ImageFilterView(model);

            Icon = App.Current.MainWindow.Resources["ic_filter_24px"] as ImageSource;
            IconMargin = new Thickness(8);
        }
    }
}
