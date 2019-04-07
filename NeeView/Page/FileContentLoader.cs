using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FileContentLoader : IContentLoader
    {
        private FileContent _content;

        public FileContentLoader(FileContent content)
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
            await Task.CompletedTask;
        }

        public void UnloadContent()
        {
        }
    }
}
