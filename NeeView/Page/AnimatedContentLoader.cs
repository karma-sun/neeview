using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class AnimatedContentLoader : BitmapContentLoader
    {
        private AnimatedContent _content;

        public AnimatedContentLoader(AnimatedContent content) : base(content)
        {
            _content = content;
        }

        /// <summary>
        /// コンテンツロード.
        /// 静止画用に画像を読み込みつつ再生用にテンポラリファイル作成
        /// </summary>
        public override async Task LoadContentAsync(CancellationToken token)
        {
            await LoadContentAsyncTemplate(() =>
            {
                // 静止画像の生成
                PictureCreateBitmapSource(token);

                // TempFileに出力し、これをMediaPlayerに再生させる
                _content.CreateTempFile(true);
            },
            token);
        }
    }
}
