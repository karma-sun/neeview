using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IContentLoader : IDisposable
    {
        event EventHandler Loaded;

        Task LoadContentAsync(CancellationToken token);

        void UnloadContent();

        Task LoadThumbnailAsync(CancellationToken token);
    }

}
