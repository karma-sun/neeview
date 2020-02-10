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

        public BookmarkPanel(FolderList folderList)
        {
            _view = new BookmarkListView(folderList);
            _view.IsVisibleLockChanged += (s, e) => IsVisibleLockChanged?.Invoke(s, e);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as DrawingImage;
            IconMargin = new Thickness(9);
        }

        public event EventHandler IsVisibleLockChanged;

        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookmarkName;

        public FrameworkElement View => _view;

        public bool IsVisibleLock => _view.IsVisibleLock;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        public void Refresh()
        {
            _view.Refresh();
        }
    }

}
