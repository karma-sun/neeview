using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageList : INotifyPropertyChanged
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


        /// <summary>
        /// PanelListItemStyle property.
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        //
        private PanelListItemStyle _panelListItemStyle;



        // ページリスト(表示部用)
        public ObservableCollection<Page> PageCollection => _bookOperation.PageList;


        /// <summary>
        /// 一度だけフォーカスするフラグ
        /// </summary>
        public bool FocusAtOnce { get; set; }

        //
        public BookHub BookHub { get; private set; }

        //
        public BookOperation _bookOperation;

        //
        public PageList(BookHub bookHub)
        {
            this.BookHub = bookHub;

            _bookOperation = BookOperation.Current; // TODO: 引数でもらう

            _bookOperation.AddPropertyChanged(nameof(_bookOperation.PageList),
                (s, e) => RaisePropertyChanged(nameof(PageCollection)));
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }
        #endregion
    }
}
