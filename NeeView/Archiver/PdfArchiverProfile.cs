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
        static PdfArchiverProfile() => Current = new PdfArchiverProfile();
        public static PdfArchiverProfile Current { get; }


        private bool _isEnabled = true;

        private Size _renderSize = new Size(1920, 1080);


        //
        private PdfArchiverProfile()
        {
        }


        [PropertyMember("@ParamArchiverPdfIsEnabled")]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamArchiverPdfSupportFileTypes")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".pfd");

        [PropertyMember("@ParamArchiverPdfRenderSize", Tips = "@ParamArchiverPdfRenderSizeTips")]
        public Size RenderSize
        {
            get { return _renderSize; }
            set
            {
                if (_renderSize != value)
                {
                    _renderSize = new Size(
                        Math.Max(value.Width, 256),
                        Math.Max(value.Height, 256));
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SizeLimitedRenderSize));
                }
            }
        }

        // 最大画像サイズで制限したサイズ
        public Size SizeLimitedRenderSize
        {
            get
            {
                return new Size(
                    Math.Min(_renderSize.Width, Config.Current.Performance.MaximumSize.Width),
                    Math.Min(_renderSize.Height, Config.Current.Performance.MaximumSize.Height));
            }
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
                size = this.SizeLimitedRenderSize;
            }
            else if (this.SizeLimitedRenderSize.IsContains(size))
            {
                size = size.Uniformed(this.SizeLimitedRenderSize);
            }
            else if (!Config.Current.Performance.MaximumSize.IsContains(size))
            {
                size = size.Uniformed(Config.Current.Performance.MaximumSize);
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
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

#pragma warning disable CS0612

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
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
        }
        #endregion
    }
}
