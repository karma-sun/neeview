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

namespace NeeView
{
    /// <summary>
    /// ページ表示用コンテンツ
    /// </summary>
    public class ViewContent : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // コンテンツ コントロール
        #region Property: Content
        private FrameworkElement _Content;
        public FrameworkElement Content
        {
            get { return _Content; }
            set { _Content = value; OnPropertyChanged(); }
        }
        #endregion

        // コンテンツの幅
        #region Property: Width
        private double _Width;
        public double Width
        {
            get { return _Width; }
            set { _Width = value; OnPropertyChanged(); }
        }
        #endregion

        // コンテンツの高さ
        #region Property: Height
        private double _Height;
        public double Height
        {
            get { return _Height; }
            set { _Height = value; OnPropertyChanged(); }
        }
        #endregion

        // コンテンツのオリジナルサイズ
        private Size _Size;
        public Size Size
        {
            get { return IsValid ? _Size : new Size(0,0); }
            set { _Size = value; }
        }

        // コンテンツの色
        public Brush Color { get; set; } = Brushes.Black;

        // 表示名
        public string FullPath { get; set; }

        // ページの場所
        public PagePosition Position { get; set; }

        // 表示パーツサイズ
        public int PartSize { get; set; }

        // 方向
        public PageReadOrder ReadOrder { get; set; }

        // スケールモード
        #region Property: BitmapScalingMode
        private BitmapScalingMode _BitmapScalingMode = BitmapScalingMode.HighQuality;
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _BitmapScalingMode; }
            set { _BitmapScalingMode = value; OnPropertyChanged(); }
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

    }
}
