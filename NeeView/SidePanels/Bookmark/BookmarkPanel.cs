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
    public class BookmarkPanel : BindableBase, IPanel
    {
        private BookmarkListView _view;
        private BookmarkFolderListPresenter _presenter;

        public BookmarkPanel(FolderList folderList)
        {
            _view = new BookmarkListView(folderList);
            _presenter = new BookmarkFolderListPresenter(_view, folderList);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as DrawingImage;
            IconMargin = new Thickness(9);

            Config.Current.Bookmark.AddPropertyChanged(nameof(BookmarkConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookmarkName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.Bookmark.IsSelected; }
            set { if (Config.Current.Bookmark.IsSelected != value) Config.Current.Bookmark.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Bookmark.IsVisible;
            set => Config.Current.Bookmark.IsVisible = value;
        }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        public void Refresh()
        {
            _presenter.Refresh();
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
