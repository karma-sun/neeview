// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    //
    public class PdfArchiverProfile
    {
        public static PdfArchiverProfile Current { get; private set; }

        //
        public PdfArchiverProfile()
        {
            Current = this;
        }

        /// <summary>
        /// RendeSize property.
        /// </summary>
        public Size RenderSize { get; set; } = new Size(1920, 1080);

        /// <summary>
        /// RenderMaxSize property.
        /// </summary>
        public Size RenderMaxSize { get; set; } = new Size(4096, 4096);

        //
        public void Validate()
        {
            this.RenderSize = new Size(Math.Max(this.RenderSize.Width, 256), Math.Max(this.RenderSize.Height, 256));
            this.RenderMaxSize = new Size(Math.Max(this.RenderSize.Width, this.RenderMaxSize.Width), Math.Max(this.RenderSize.Height, this.RenderMaxSize.Height));
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
            else if (!this.RenderMaxSize.IsContains(size))
            {
                size = size.Uniformed(this.RenderMaxSize);
            }

            return size;
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(typeof(Size), "1920,1080")]
            [PropertyMember(Name = "PDF画像標準サイズ", Tips = "PDFのページはこの大きさに収まるサイズで画像化されます", IsVisible = false)]
            public Size RenderSize { get; set; }

            [DataMember, DefaultValue(typeof(Size), "4096,4096")]
            [PropertyMember(Name = "PDF画像最大サイズ", Tips = "拡大表示によるPDFレンダリング画像の最大サイズです", IsVisible = false)]
            public Size RenderMaxSize { get; set; }

            [OnDeserialized]
            public void OnDeserialized(StreamingContext c)
            {
                if (this.RenderSize == default(Size))
                {
                    this.RenderSize = this.RenderMaxSize;
                    this.RenderMaxSize = new Size(4096, 4096);
                }
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.RenderSize = this.RenderSize;
            memento.RenderMaxSize = this.RenderMaxSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.RenderSize = memento.RenderSize;
            this.RenderMaxSize = memento.RenderMaxSize;

            Validate();
        }
        #endregion
    }
}
