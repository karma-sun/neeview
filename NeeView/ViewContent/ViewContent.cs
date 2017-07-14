﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ページ表示用コンテンツ
    /// </summary>
    public class ViewContent : BindableBase
    {
        #region Properties, Fields

        /// <summary>
        /// ViewContentSource
        /// TODO: 他のパラメータとあわせて整備
        /// </summary>
        public ViewContentSource Source { get; set; }
        
        /// <summary>
        /// ページ
        /// </summary>
        public Page Page => Source?.Page;

        /// <summary>
        /// コンテンツ
        /// </summary>
        public PageContent Content => Source?.Content;

        /// <summary>
        /// Property: View.
        /// </summary>
        private PageContentView _view;
        public PageContentView View
        {
            get { return _view; }
            set { _view = value; RaisePropertyChanged(); }
        }

        // コンテンツの幅 (with DPI)
        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }

        // コンテンツの高さ (with DPI)
        private double _height;
        public double Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged(); }
        }

        // コンテンツのオリジナルサイズ
        private Size _size;
        public Size Size
        {
            get { return IsValid ? _size : new Size(0, 0); }
            set { _size = value; }
        }

        // コンテンツの色
        public Color Color = Colors.Black;

        // フルパス名
        public string FullPath => Page?.FullPath;

        // ファイル名
        public string FileName => LoosePath.GetFileName(Page?.FullPath.TrimEnd('\\'));

        // フォルダーの場所
        public string FolderPlace => Page?.GetFolderPlace();


        // ファイルプロキシ(必要であれば)
        // 寿命確保用。GCされてファイルが消えないように。
        public FileProxy FileProxy { get; set; }

        // ページの場所
        public PagePosition Position => Source.Position;


        // スケールモード
        private BitmapScalingMode _bitmapScalingMode = BitmapScalingMode.HighQuality;
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _bitmapScalingMode; }
            set { _bitmapScalingMode = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// AnimationImageVisibility property.
        /// </summary>
        private Visibility _AnimationImageVisibility = Visibility.Collapsed;
        public Visibility AnimationImageVisibility
        {
            get { return _AnimationImageVisibility; }
            set { if (_AnimationImageVisibility != value) { _AnimationImageVisibility = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// AnimationPlayerVisibility property.
        /// </summary>
        private Visibility _AnimationPlayerVisibility = Visibility.Visible;
        public Visibility AnimationPlayerVisibility
        {
            get { return _AnimationPlayerVisibility; }
            set { if (_AnimationPlayerVisibility != value) { _AnimationPlayerVisibility = value; RaisePropertyChanged(); } }
        }

        // 有効判定
        public bool IsValid => (View != null);


        // 表示スケール(%)
        public double Scale => Width / Size.Width;

        //
        public ViewContentReserver Reserver { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ViewContent()
        {
        }

        public ViewContent(ViewContentSource source)
        {
            this.Source = source;
            this.Size = source.Size;
            this.Color = Colors.Black;
        }

        #endregion

        #region Medhods

        // ページパーツ文字
        public string GetPartString()
        {
            if (Source.PartSize == 1)
            {
                int part = Source.ReadOrder == PageReadOrder.LeftToRight ? 1 - Source.Position.Part : Source.Position.Part;
                return part == 0 ? "(R)" : "(L)";
            }
            else
            {
                return "";
            }
        }


        //
        protected ViewContentParameters CreateBindingParameter()
        {
            var parameter = new ViewContentParameters()
            {
                ForegroundBrush = new Binding(nameof(ContentCanvasBrush.ForegroundBrush)) { Source = ContentCanvasBrush.Current },
                BitmapScalingMode = new Binding(nameof(BitmapScalingMode)) { Source = this },
                AnimationImageVisibility = new Binding(nameof(AnimationImageVisibility)) { Source = this },
                AnimationPlayerVisibility = new Binding(nameof(AnimationPlayerVisibility)) { Source = this },
            };

            return parameter;
        }

        //
        public virtual bool IsBitmapScalingModeSupported() => false;


        //
        public virtual bool Rebuild(double scale)
        {
            ////Debug.WriteLine($"UpdateContent: {Width}x{Height} x{scale}");
            return true;
        }

        #endregion
    }
    
    /// <summary>
    /// Reserver
    /// </summary>
    public class ViewContentReserver
    {
        public Thumbnail Thumbnail { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
    }
    
    /// <summary>
    /// View生成用パラメータ
    /// </summary>
    public class ViewContentParameters
    {
        public Binding ForegroundBrush { get; set; }
        public Binding BitmapScalingMode { get; set; }
        public Binding AnimationImageVisibility { get; set; }
        public Binding AnimationPlayerVisibility { get; set; }
        public ViewContentReserver Reserver { get; set; } // 未使用
    }
}