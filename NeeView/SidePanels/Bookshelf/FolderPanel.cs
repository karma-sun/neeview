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

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(FolderPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookshelfName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.Bookshelf.IsSelected; }
            set { if (Config.Current.Bookshelf.IsSelected != value) Config.Current.Bookshelf.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Bookshelf.IsVisible;
            set => Config.Current.Bookshelf.IsVisible = value;
        }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;


        public void Refresh()
        {
            _view.Refresh();
        }

        public void Focus()
        {
            _view.FocusAtOnce();
        }
    }

}
