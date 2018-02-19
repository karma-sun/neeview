// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer コンテンツ
    /// </summary>
    public class MediaContent : BitmapContent
    {
        public override bool IsLoaded => FileProxy != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MediaContent(ArchiveEntry entry) : base(entry)
        {
            IsAnimated = true;
        }

        /// <summary>
        /// サイズ設定
        /// </summary>
        public void SetSize(Size size)
        {
            this.Size = size;

            if (this.Picture == null) return;
            this.Picture.PictureInfo.Size = size;
            this.Picture.PictureInfo.OriginalSize = size;
        }

#pragma warning disable CS1998
        /// <summary>
        /// コンテンツロード.
        /// </summary>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            this.Picture = new Picture(Entry);
            this.Picture.PictureInfo.BitsPerPixel = 32;

            this.Size = new Size(704, 396);

            if (!token.IsCancellationRequested)
            {
                // TempFileに出力し、これをMediaPlayerに再生させる
                CreateTempFile(true);

                RaiseLoaded();
                RaiseChanged();
            }
        }
    }
#pragma warning restore 

}
