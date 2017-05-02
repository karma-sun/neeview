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

        public bool IsVisibleLock => false;


        //
        public HistoryPanel()
        {
            _view = new HistoryPanelView();

            Icon = App.Current.MainWindow.Resources["pic_history_24px"] as ImageSource;
            IconMargin = new Thickness(7, 8, 9, 8);
            //IconMargin = new Thickness(8);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public HistoryControlViewModel.Memento HistoryControlMemento;
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.HistoryControlMemento = _view.History.VM.CreateMemento();
            return memento;
        }

        //
        public void Resore(Memento memento)
        {
            if (memento == null) return;
            _view.History.VM?.Restore(memento.HistoryControlMemento);
        }
        #endregion
    }
}
