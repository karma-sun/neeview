using NeeView.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    //
    public class PictureProfile : BindableBase
    {
        // singleton
        private static PictureProfile _current;
        public static PictureProfile Current => _current = _current ?? new PictureProfile();


        /// <summary>
        /// IsSusieEnabled property.
        /// </summary>
        private bool _isSusieEnabled;
        public bool IsSusieEnabled
        {
            get { return _isSusieEnabled; }
            set { if (_isSusieEnabled != value) { _isSusieEnabled = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsSusieFirst property.
        /// </summary>
        private bool _IsSusieFirst;
        public bool IsSusieFirst
        {
            get { return _IsSusieFirst; }
            set { if (_IsSusieFirst != value) { _IsSusieFirst = value; RaisePropertyChanged(); } }
        }

        // 画像最大サイズ
        public Size Maximum { get; set; } = new Size(4096, 4096);


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsSusieEnabled { get; set; }
            [DataMember]
            public bool IsSusieFirst { get; set; }
            [DataMember]
            public Size Maximum { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsSusieEnabled = this.IsSusieEnabled;
            memento.IsSusieFirst = this.IsSusieFirst;
            memento.Maximum = this.Maximum;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsSusieEnabled = memento.IsSusieEnabled;
            this.IsSusieFirst = memento.IsSusieFirst;
            this.Maximum = memento.Maximum;
        }
        #endregion

    }
}
