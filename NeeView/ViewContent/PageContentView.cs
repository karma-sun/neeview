// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// PageControl FrameworkElement
    /// ページ名レイヤーを備えたもの
    /// </summary>
    public class PageContentView : Grid
    {
        /// <summary>
        /// コンテンツ
        /// </summary>
        private ContentControl _contentControl;

        /// <summary>
        /// テキスト要素
        /// </summary>
        private TextBlock _textBlock;


        /// <summary>
        /// コンストラクター
        /// 初期状態ではテキストは非表示って何！
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public PageContentView(string text)
        {
            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            textBlock.FontSize = 20;
            textBlock.Margin = new Thickness(10);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            _contentControl = new ContentControl();

            _textBlock = textBlock;
            _textBlock.Visibility = Visibility.Collapsed;

            this.Children.Add(_contentControl);
            this.Children.Add(_textBlock);
        }


        /// <summary>
        /// Content property.
        /// </summary>
        public FrameworkElement Content
        {
            get { return (FrameworkElement)_contentControl.Content; }
            set { if (_contentControl.Content != value) { _contentControl.Content = value; } }
        }

        /// <summary>
        /// Text property.
        /// TODO: サムネイルしか使ってないじゃん！
        /// </summary>
        public string Text
        {
            get { return _textBlock.Text; }
            set
            {
                _textBlock.Text = value;
                _textBlock.Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

    }
}
