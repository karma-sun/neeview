// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    //
    public class PictureProfile : BindableBase
    {
        // 
        private static PictureProfile _current;
        public static PictureProfile Current => _current = _current ?? new PictureProfile();

        // 有効ファイル拡張子
        private PictureFileExtension _fileExtension = new PictureFileExtension();

        // 画像最大サイズ
        public Size Maximum { get; set; } = new Size(8192, 8192);


        //
        public bool IsSupported(string fileName)
        {
            return _fileExtension.IsSupported(fileName);
        }

        // 除外パス判定
        // TODO: これ、BookProfileじゃね？
        public bool IsExcludedPath(string path)
        {
            return path.Split('/', '\\').Any(e => BookProfile.Current.Excludes.Contains(e));
        }


        //
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(this.Maximum);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Size Maximum { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Maximum = this.Maximum;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            //this.Maximum = memento.Maximum;
        }
        #endregion

    }
}
