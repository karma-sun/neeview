using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    //
    public class PdfArchiverProfile : BindableBase
    {
        public static PdfArchiverProfile Current { get; private set; }

        private bool _isEnabled = true;

        //
        public PdfArchiverProfile()
        {
            Current = this;
        }


        [PropertyMember("PDFを使用する")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("PDFファイルの拡張子")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".pfd");

        [PropertyMember("PDFページ標準サイズ", Tips = "通常は表示サイズにあわせてレンダリングしますが、下限はこの標準サイズになります。 より小さくなる場合には縮小して表示します。")]
        public Size RenderSize { get; set; } = new Size(1024, 1024);


        //
        public void Validate()
        {
            this.RenderSize = new Size(
                MathUtility.Clamp(this.RenderSize.Width, 256, PictureProfile.Current.MaximumSize.Width),
                MathUtility.Clamp(this.RenderSize.Height, 256, PictureProfile.Current.MaximumSize.Height));
        }

        /// <summary>
        /// 適切な描写サイズを生成する
        /// </summary>
        /// <param name="size">希望するサイズ</param>
        /// <returns></returns>
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty)
            {
                size = this.RenderSize;
            }
            else if (this.RenderSize.IsContains(size))
            {
                size = size.Uniformed(this.RenderSize);
            }
            else if (!PictureProfile.Current.MaximumSize.IsContains(size))
            {
                size = size.Uniformed(PictureProfile.Current.MaximumSize);
            }

            return size;
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(true)]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(typeof(Size), "1920,1080")]
            public Size RenderSize { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public Size RenderMaxSize { get; set; }


            [OnDeserializing]
            public void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

#pragma warning disable CS0612

            [OnDeserialized]
            public void OnDeserialized(StreamingContext c)
            {
                if (this.RenderSize == default(Size))
                {
                    this.RenderSize = this.RenderMaxSize;
                }
            }

#pragma warning restore CS0612

        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = this.IsEnabled;
            memento.RenderSize = this.RenderSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsEnabled = memento.IsEnabled;
            this.RenderSize = memento.RenderSize;

            Validate();
        }
        #endregion
    }
}
