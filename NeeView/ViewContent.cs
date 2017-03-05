// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// ページ表示用コンテンツ
    /// </summary>
    public class ViewContent : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

       
        // コンテンツ コントロール
        #region Property: Content
        private PageContentView _content;
        public PageContentView Content
        {
            get { return _content; }
            set { _content = value; RaisePropertyChanged(); }
        }
        #endregion

        // コンテンツの幅 (with DPI)
        #region Property: Width
        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }
        #endregion

        // コンテンツの高さ (with DPI)
        #region Property: Height
        private double _height;
        public double Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged(); }
        }
        #endregion

        // コンテンツのオリジナルサイズ
        private Size _size;
        public Size Size
        {
            get { return IsValid ? _size : new Size(0, 0); }
            set { _size = value; }
        }

        // コンテンツソースサイズ
        public Size SourceSize { get; set; }

        // コンテンツの色
        public Color Color = Colors.Black;

        // 表示名
        public string FullPath { get; set; }

        // フォルダの場所 ページの上位の有効パス
        public string FolderPlace { get; set; }

        // ファイルの場所 ページを含む有効パス
        public string FilePlace { get; set; }

        // ファイル名
        public string FileName => LoosePath.GetFileName(FullPath.TrimEnd('\\'));

        // 画像ソース(あれば)
        public BitmapSource Bitmap { get; set; }

        // ファイル情報(あれば)
        public FileBasicInfo Info { get; set; }

        // ファイルプロキシ(あれば)
        public FileProxy FileProxy { get; set; }

        // ページの場所
        public PagePosition Position { get; set; }

        // 表示パーツサイズ
        public int PartSize { get; set; }

        // 方向
        public PageReadOrder ReadOrder { get; set; }

        // スケールモード
        #region Property: BitmapScalingMode
        private BitmapScalingMode _bitmapScalingMode = BitmapScalingMode.HighQuality;
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _bitmapScalingMode; }
            set { _bitmapScalingMode = value; RaisePropertyChanged(); }
        }
        #endregion

        // 有効判定
        public bool IsValid => (Content != null);

        // ページパーツ文字
        public string GetPartString()
        {
            if (PartSize == 1)
            {
                int part = ReadOrder == PageReadOrder.LeftToRight ? 1 - Position.Part : Position.Part;
                return part == 0 ? "(R)" : "(L)";
            }
            else
            {
                return "";
            }
        }

        // 表示スケール(%)
        public double Scale => Width / Size.Width;

        // ピクセル深度
        private int _bitsPerPixel;
        public int BitsPerPixel
        {
            get
            {
                if (_bitsPerPixel == 0) _bitsPerPixel = Bitmap.GetSourceBitsPerPixel();
                return _bitsPerPixel;
            }
        }
    }
}
