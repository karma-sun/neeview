using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class BookmarkPanel : BindableBase, IPanel
    {
        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookmarkName;

        private BookmarkListView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => _view.IsRenaming;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        //
        public BookmarkPanel(FolderList folderList)
        {
            _view = new BookmarkListView(folderList);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as DrawingImage;
            IconMargin = new Thickness(9);
        }
    }

}
