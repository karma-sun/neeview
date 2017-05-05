using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// FileInformation : Model
    /// </summary>
    public class FileInformation : INotifyPropertyChanged
    {
        // PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        /// <summary>
        /// IsUseExifDateTime property.
        /// </summary>
        public bool IsUseExifDateTime
        {
            get { return _IsUseExifDateTime; }
            set { if (_IsUseExifDateTime != value) { _IsUseExifDateTime = value; RaisePropertyChanged(); } }
        }

        private bool _IsUseExifDateTime;


        /// <summary>
        /// IsVisibleBitsPerPixel property.
        /// </summary>
        public bool IsVisibleBitsPerPixel
        {
            get { return _IsVisibleBitsPerPixel; }
            set { if (_IsVisibleBitsPerPixel != value) { _IsVisibleBitsPerPixel = value; RaisePropertyChanged(); } }
        }

        private bool _IsVisibleBitsPerPixel;


        /// <summary>
        /// IsVisibleLoader property.
        /// </summary>
        public bool IsVisibleLoader
        {
            get { return _IsVisibleLoader; }
            set { if (_IsVisibleLoader != value) { _IsVisibleLoader = value; RaisePropertyChanged(); } }
        }

        private bool _IsVisibleLoader;



        /// <summary>
        /// ViewContent property.
        /// </summary>
        public ViewContent ViewContent
        {
            get { return _viewContent; }
            set { if (_viewContent != value) { _viewContent = value; RaisePropertyChanged(); } }
        }

        private ViewContent _viewContent;


        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsUseExifDateTime { get; set; }
            [DataMember]
            public bool IsVisibleBitsPerPixel { get; set; }
            [DataMember]
            public bool IsVisibleLoader { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsUseExifDateTime = this.IsUseExifDateTime;
            memento.IsVisibleBitsPerPixel = this.IsVisibleBitsPerPixel;
            memento.IsVisibleLoader = this.IsVisibleLoader;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            IsUseExifDateTime = memento.IsUseExifDateTime;
            IsVisibleBitsPerPixel = memento.IsVisibleBitsPerPixel;
            IsVisibleLoader = memento.IsVisibleLoader;
        }
    }


    /// <summary>
    /// 旧：ファイル情報パネル設定
    /// 互換性のために残してあります
    /// </summary>
    [DataContract]
    public class FileInfoSetting
    {
        [DataMember]
        public bool IsUseExifDateTime { get; set; }

        [DataMember]
        public bool IsVisibleBitsPerPixel { get; set; }

        [DataMember]
        public bool IsVisibleLoader { get; set; }
    }
}
