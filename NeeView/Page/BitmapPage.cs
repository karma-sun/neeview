// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像ページ
    /// </summary>
    public class BitmapPage : Page
    {
        public BitmapPage(ArchiveEntry entry)
        {
            Entry = entry;

            Content = new ImageContent(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        } 
    }

    /// <summary>
    /// アニメーション画像ページ
    /// </summary>
    public class AnimatedPage : Page
    {
        public AnimatedPage(ArchiveEntry entry)
        {
            Entry = entry;

            Content = new AnimatedContent(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        }
    }
    

}
