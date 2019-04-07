using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MediaContentLoader : BitmapContentLoader
    {
        private MediaContent _content;

        public MediaContentLoader(MediaContent content): base(content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override async Task LoadContentAsync(CancellationToken token)
        {
            if (_content.IsLoaded) return;

            _content.SetSize(new Size(1280, 720));

            if (!token.IsCancellationRequested)
            {
                // TempFileに出力し、これをMediaPlayerに再生させる
                _content.CreateTempFile(true);

                RaiseLoaded();

                _content.UpdateDevStatus();
            }

            await Task.CompletedTask;
        }
    }
}
