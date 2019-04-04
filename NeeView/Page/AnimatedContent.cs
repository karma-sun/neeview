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
        public AnimatedContent(ArchiveEntry entry) : base(entry)
        {
            IsAnimated = true;
        }


        public override bool IsLoaded => FileProxy != null;
        public override bool IsViewReady => IsLoaded; 

        public override bool CanResize => false;


        /// <summary>
        /// コンテンツロード.
        /// サムネイル用に画像を読込つつ再生用にテンポラリファイル作成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override async Task LoadContentAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            // 画像情報の取得
            this.Picture = LoadPicture(Entry, token);

            // TempFileに出力し、これをMediaPlayerに再生させる
            CreateTempFile(true);

            RaiseLoaded();
            UpdateDevStatus();

            await Task.CompletedTask;
        }
    }
}
