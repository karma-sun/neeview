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

#pragma warning disable CS0067
        public event EventHandler Loaded;
#pragma warning restore CS0067

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
