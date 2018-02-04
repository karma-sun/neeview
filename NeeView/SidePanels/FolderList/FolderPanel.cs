// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FolderPanel : BindableBase, IPanel
    {
        public string TypeCode => nameof(FolderPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "フォルダーリスト";

        private FolderPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => _view.IsVisibleLock;

        public PanelPlace DefaultPlace => PanelPlace.Left;


        //
        public FolderPanel(FolderPanelModel folderPanel, FolderList folderList, PageList pageList)
        {
            _view = new FolderPanelView(folderPanel, folderList, pageList);

            Icon = App.Current.MainWindow.Resources["pic_folder_24px"] as DrawingImage;
            IconMargin = new Thickness(8);
        }
    }

}
