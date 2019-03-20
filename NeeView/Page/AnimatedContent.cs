using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アニメーションコンテンツ
    /// </summary>
    public class AnimatedContent : BitmapContent
    {
        public override bool IsLoaded => FileProxy != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public AnimatedContent(ArchiveEntry entry) : base(entry)
        {
            IsAnimated = true;
        }

        /// <summary>
        /// コンテンツロード.
        /// サムネイル用に画像を読込つつ再生用にテンポラリファイル作成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            // 画像情報の取得
            var picture = LoadPicture(Entry, PictureCreateOptions.CreateBitmap, token);
            this.Picture = picture;

            // TempFileに出力し、これをMediaPlayerに再生させる
            CreateTempFile(true);

            RaiseLoaded();
            RaiseChanged();

            // サムネイル作成
            if (Thumbnail.IsValid || picture == null) return;
            Thumbnail.Initialize(picture.CreateThumbnail(token));

            await Task.CompletedTask;
        }
    }
}
