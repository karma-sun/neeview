// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="entry"></param>
        public MediaContent(ArchiveEntry entry) : base(entry)
        {
        }

        /// <summary>
        /// コンテンツロード.
        /// サムネイル用に画像を読込つつ再生用にテンポラリファイル作成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadAsync(CancellationToken token)
        {
            // TODO: 動画情報取得を実装。MediaPlayerを使うとメインスレッドでないと問題があるが、どうしよう。

            if (IsLoaded) return;

            // 画像情報の取得
            ////var picture = await LoadPictureAsync(Entry, PictureCreateOptions.CreateBitmap, token);
            this.Size = new System.Windows.Size(640, 480);

            if (!token.IsCancellationRequested)
            {
                ////this.Picture = picture;

                // TempFileに出力し、これをMediaPlayerに再生させる
                CreateTempFile(true);

                RaiseLoaded();
                RaiseChanged();
            }

            // サムネイル作成
            ////if (Thumbnail.IsValid || picture == null) return;
            ////Thumbnail.Initialize(picture.CreateThumbnail());
        }
    }
}
