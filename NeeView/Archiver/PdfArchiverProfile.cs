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


        // 最大画像サイズで制限したサイズ
        public Size SizeLimitedRenderSize
        {
            get
            {
                return new Size(
                    Math.Min(Config.Current.Archive.Pdf.RenderSize.Width, Config.Current.Performance.MaximumSize.Width),
                    Math.Min(Config.Current.Archive.Pdf.RenderSize.Height, Config.Current.Performance.MaximumSize.Height));
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
        public class Memento : IMemento
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

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                if (this.RenderSize == default(Size))
                {
                    this.RenderSize = this.RenderMaxSize;
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.Archive.Pdf.IsEnabled = IsEnabled;
                config.Archive.Pdf.RenderSize = RenderSize;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsEnabled = Config.Current.Archive.Pdf.IsEnabled;
            memento.RenderSize = Config.Current.Archive.Pdf.RenderSize;
            return memento;
        }

        #endregion
    }
}
