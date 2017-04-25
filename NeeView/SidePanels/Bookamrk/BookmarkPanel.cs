using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class BookmarkPanel : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "ブックマーク";

        private BookmarkPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        //
        public BookmarkPanel()
        {
            _view = new BookmarkPanelView();

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as ImageSource;
            IconMargin = new Thickness(8);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }
    }
}
