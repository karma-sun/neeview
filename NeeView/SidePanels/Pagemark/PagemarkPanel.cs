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
    public class PagemarkPanel : IPanel, INotifyPropertyChanged
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


        public string TypeCode => nameof(PagemarkPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "ページマーク";

        private PagemarkPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;


        //
        public PagemarkPanel()
        {
            _view = new PagemarkPanelView();

            Icon = App.Current.MainWindow.Resources["pic_bookmark_24px"] as ImageSource;
            IconMargin = new Thickness(10);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }
    }
}
