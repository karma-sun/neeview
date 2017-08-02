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
        public Size RenderSize { get; set; } = new Size(1024, 1024);

        //
        public void Validate()
        {
            this.RenderSize = new Size(
                NVUtility.Clamp(this.RenderSize.Width, 256, PictureProfile.Current.MaximumSize.Width),
                NVUtility.Clamp(this.RenderSize.Height, 256, PictureProfile.Current.MaximumSize.Height));
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
            [DataMember, DefaultValue(typeof(Size), "1920,1080")]
            [PropertyMember(Name = "PDF画像標準サイズ", Tips = "PDFのページはこの大きさに収まるサイズで画像化されます", IsVisible = false)]
            public Size RenderSize { get; set; }

            [Obsolete, DataMember]
            public Size RenderMaxSize { get; set; }

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
            memento.RenderSize = this.RenderSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.RenderSize = memento.RenderSize;

            Validate();
        }
        #endregion
    }
}
