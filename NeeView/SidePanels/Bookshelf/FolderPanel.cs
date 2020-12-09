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
        private FolderListView _view;
        private BookshelfFolderListPresenter _folderListPresenter;

        public FolderPanel(BookshelfFolderList folderList)
        {
            _view = new FolderListView(folderList);
            _folderListPresenter = new BookshelfFolderListPresenter(_view, folderList);

            Icon = App.Current.MainWindow.Resources["pic_bookshelf"] as DrawingImage;

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(FolderPanel);

        public ImageSource Icon { get; private set; }

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

        public BookshelfFolderListPresenter Presenter => _folderListPresenter;

        public void Refresh()
        {
            _folderListPresenter.Refresh();
        }

        public void Focus()
        {
            _folderListPresenter.FocusAtOnce();
        }
    }

}
