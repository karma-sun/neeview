// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows;
using NeeView.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PagemarkListViewl.xaml の相互作用ロジック
    /// </summary>
    public partial class PagemarkListViewl : UserControl
    {
        private PagemarkListViewModel _vm;


        public PagemarkListViewl()
        {
            InitializeComponent();
        }

        public PagemarkListViewl(PagemarkList model) : this()
        {
            _vm = new PagemarkListViewModel(model);
            this.DockPanel.DataContext = _vm;
        }
    }
}
