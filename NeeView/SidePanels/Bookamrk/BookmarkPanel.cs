using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class BookmarkPanel : BindableBase, IPanel
    {
        public BookmarkPanel(BookmarkList model)
        {
            _view = new BookmarkListView(model);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as ImageSource;
            IconMargin = new Thickness(8);
        }

        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookmarkName;

        private BookmarkListView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => _view.IsBusy;

        public PanelPlace DefaultPlace => PanelPlace.Left;
    }
}
