using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FolderPanel : BindableBase, IPanel
    {
        private FolderPanelView _view;

        public FolderPanel(FolderPanelModel folderPanel, FolderList folderList, PageList pageList)
        {
            _view = new FolderPanelView(folderPanel, folderList, pageList);

            Icon = App.Current.MainWindow.Resources["pic_bookshelf"] as DrawingImage;
            IconMargin = new Thickness(9);
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public string TypeCode => nameof(FolderPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookshelfName;

        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;


        public void Refresh()
        {
            _view.Refresh();
        }
    }

}
