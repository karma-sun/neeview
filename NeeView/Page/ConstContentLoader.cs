using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ConstContentLoader : IContentLoader
    {
        private ConstContent _content;

        public ConstContentLoader(ConstContent content)
        {
            _content = content;
        }

        public event EventHandler Loaded;

        public void Dispose()
        {
        }

        public async Task LoadContentAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task LoadThumbnailAsync(CancellationToken token)
        {
            _content.Thumbnail.Initialize(_content.ThumbnailType);
            await Task.CompletedTask;
        }

        public void UnloadContent()
        {
        }
    }
}
