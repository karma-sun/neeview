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
    public enum ResizeInterpolation
    {
        Lanczos, // デフォルト

        NearestNeighbor,
        Average,
        Linear,
        Quadratic,
        Hermite,
        Mitchell,
        CatmullRom,
        Cubic,
        CubicSmoother,
        Spline36,
    }

    //
    public class PictureProfile : BindableBase
    {
        // 
        public static PictureProfile Current { get; private set; }

        #region Fields

        // 有効ファイル拡張子
        private PictureFileExtension _fileExtension = new PictureFileExtension();

        #endregion

        #region Properties

        // 画像最大サイズ
        public Size Maximum { get; set; } = new Size(4096, 4096);


        /// <summary>
        /// IsResizeEnabled property.
        /// </summary>
        private bool _isResizeFilterEnabled;
        public bool IsResizeFilterEnabled
        {
            get { return _isResizeFilterEnabled; }
            set { if (_isResizeFilterEnabled != value) { _isResizeFilterEnabled = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ResizeInterpolation property.
        /// </summary>
        private ResizeInterpolation _resizeInterpolation;
        public ResizeInterpolation ResizeInterpolation
        {
            get { return _resizeInterpolation; }
            set { if (_resizeInterpolation != value) { _resizeInterpolation = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// IsResizeSharp property.
        /// </summary>
        private bool _IsResizeSharp;
        public bool IsResizeSharp
        {
            get { return _IsResizeSharp; }
            set { if (_IsResizeSharp != value) { _IsResizeSharp = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Clop property.
        /// </summary>
        private Thickness _Clop;
        public Thickness Clop
        {
            get { return _Clop; }
            set { if (_Clop != value) { _Clop = value; RaisePropertyChanged(); } }
        }





        #endregion

        #region Constructors

        //
        public PictureProfile()
        {
            Current = this;
        }

        #endregion

        #region Methods

        // 対応拡張子判定
        public bool IsSupported(string fileName)
        {
            return _fileExtension.IsSupported(fileName);
        }

        // 最大サイズ内におさまるサイズを返す
        public Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(this.Maximum);
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public Size Maximum { get; set; }
            [DataMember]
            public bool IsResizeFilterEnabled { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Maximum = this.Maximum;
            memento.IsResizeFilterEnabled = this.IsResizeFilterEnabled;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.Maximum = memento.Maximum;
            this.IsResizeFilterEnabled = memento.IsResizeFilterEnabled;
        }
        #endregion

    }
}
