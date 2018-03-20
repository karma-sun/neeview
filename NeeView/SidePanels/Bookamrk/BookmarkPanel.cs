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
        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.BookmarkName;

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;

        //
        public BookmarkPanel(BookmarkList model)
        {
            View = new BookmarkListView(model);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as ImageSource;
            IconMargin = new Thickness(8);
        }
    }
}
