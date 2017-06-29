// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
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
        /// RenderMaxSize property.
        /// </summary>
        public Size RenderMaxSize { get; set; } = new Size(1920, 1080);


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(typeof(Size), "1920,1080")]
            [PropertyMember(Name = "PDF画像最大サイズ", Tips = "PDFのページはこの大きさに収まるサイズで画像化されます")]
            public Size RenderMaxSize { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.RenderMaxSize = this.RenderMaxSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.RenderMaxSize = memento.RenderMaxSize;
        }
        #endregion
    }
}
