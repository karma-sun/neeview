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
    public class HistoryPanel : IPanel, INotifyPropertyChanged
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


        public string TypeCode => nameof(HistoryPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "履歴";

        private HistoryPanelView _view;
        public FrameworkElement View => _view;

        //
        public HistoryPanel()
        {
            _view = new HistoryPanelView();

            Icon = _view.Resources["ic_history_24px"] as ImageSource;
            IconMargin = new Thickness(6, 8, 10, 8);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }
    }
}
