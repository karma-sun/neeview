using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// CancellationTokenでストリームを強制Disposeさせる
    /// </summary>
    public class StreamCanceller : IDisposable
    {
        CancellationTokenRegistration _tokenRegistration;

        public StreamCanceller(Stream stream, CancellationToken token)
        {
            // regist CancellationToken Callback
            _tokenRegistration = token.Register(() =>
            {
                Debug.WriteLine($"Stream.Dispose: {stream}");
                IsStreamCanceled = true;
                stream?.Dispose();
            });
        }

        public bool IsStreamCanceled { get; private set; }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tokenRegistration.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
