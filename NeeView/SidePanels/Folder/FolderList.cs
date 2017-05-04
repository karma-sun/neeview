using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    //
    public class FolderPlaceChangedEventArgs : EventArgs
    {
        public string Place { get; set; }
        public string Select { get; set; }
        public bool IsFocus { get; set; }
    }


    //
    public class FolderList : INotifyPropertyChanged
    {
        // PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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


        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { if (_isVisibleHistoryMark != value) { _isVisibleHistoryMark = value; RaisePropertyChanged(); } }
        }

        private bool _isVisibleHistoryMark = true;


        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { if (_isVisibleBookmarkMark != value) { _isVisibleBookmarkMark = value; RaisePropertyChanged(); } }
        }

        private bool _isVisibleBookmarkMark = true;



        /// <summary>
        /// 
        /// </summary>
        public BookHub BookHub { get; private set; }

        //
        public FolderPanelModel FolderPanel { get; private set; }


        /// <summary>
        /// フォルダーアイコン表示位置
        /// </summary>
        public FolderIconLayout FolderIconLayout => Preference.Current.folderlist_iconlayout;


        //
        public FolderList(BookHub bookHub, FolderPanelModel folderPanel)
        {
            this.FolderPanel = folderPanel;
            this.BookHub = bookHub;
        }

        public event EventHandler<FolderPlaceChangedEventArgs> PlaceChanged;

        //
        public void SetPlace(string place, string select, bool isFocus)
        {
            var args = new FolderPlaceChangedEventArgs()
            {
                Place = place,
                Select = select,
                IsFocus = isFocus
            };
            PlaceChanged?.Invoke(this, args);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;

            // Preference反映
            RaisePropertyChanged(nameof(FolderIconLayout));
        }

        #endregion
    }
}
