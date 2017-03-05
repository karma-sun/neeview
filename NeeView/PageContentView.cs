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
    /// </summary>
    public class PageContentView : Grid
    {
        /// <summary>
        /// メイン要素
        /// </summary>
        public FrameworkElement Element { get; private set; }

        /// <summary>
        /// テキスト要素
        /// </summary>
        private TextBlock _textBlock;

        /// <summary>
        /// コンストラクタ
        /// 初期状態ではテキストは非表示
        /// TODO: 後でelementの種類を判別できるように
        /// </summary>
        /// <param name="element"></param>
        /// <param name="textBlock"></param>
        public PageContentView(FrameworkElement element, TextBlock textBlock)
        {
            this.Element = element;
            _textBlock = textBlock;
            _textBlock.Visibility = Visibility.Collapsed;

            this.Children.Add(element);
            this.Children.Add(textBlock);
        }

        /// <summary>
        /// テキスト変更
        /// 文字列が設定されると表示される
        /// TODO: ↑しっくりこない
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (text != null)
            {
                _textBlock.Text = text;
                _textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                _textBlock.Visibility = Visibility.Collapsed;
            }
        }
    }


}
