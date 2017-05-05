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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderListPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderPanelView : UserControl 
    {
        private FolderPanelViewModel _vm;

        private FolderListView _folderList;
        private PageListView _pageList;

        //
        public FolderPanelView()
        {
            InitializeComponent();
        }

        //
        public FolderPanelView(FolderPanelModel model, FolderList folderList, PageList pageList) : this()
        {
            _vm = new FolderPanelViewModel(model);
            this.Root.DataContext = _vm;

            _folderList = new FolderListView(folderList);
            this.FolderList.Content = _folderList;

            _pageList = new PageListView(pageList);
            this.PageList.Content = _pageList;
        }

        //
        public bool IsVisibleLock => _folderList.IsRenaming;
    }


    /// <summary>
    /// 
    /// </summary>
    public class FolderPanelViewModel : INotifyPropertyChanged
    {
        // PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Model property.
        /// </summary>
        public FolderPanelModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private FolderPanelModel _model;


        //
        public FolderPanelViewModel(FolderPanelModel model)
        {
            _model = model;
        }
    }
}
