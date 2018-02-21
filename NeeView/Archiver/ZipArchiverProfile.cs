using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ZipArchiverProfile : BindableBase
    {
        public static ZipArchiverProfile Current { get; private set; }

        private bool _isEnabled = true;

        public ZipArchiverProfile()
        {
            Current = this;
        }

        [PropertyMember("標準の圧縮ファイル展開を使用する")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("圧縮ファイルの拡張子", Tips = "zip形式のみ対応しています。")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".zip");

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(".zip")]
            public string SupportFileTypes { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = this.IsEnabled;
            memento.SupportFileTypes = this.SupportFileTypes.ToString();

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.SupportFileTypes.FromString(memento.SupportFileTypes.ToString());
        }

        #endregion

    }
}
