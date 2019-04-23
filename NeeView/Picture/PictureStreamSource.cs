using System;
using System.IO;
using System.Threading;

namespace NeeView
{
    public class PictureStreamSource
    {
        private byte[] _rawData;
        private ArchiveEntry _entry;

        public PictureStreamSource(ArchiveEntry entry)
        {
            _entry = entry;
        }

        public ArchiveEntry ArchiveEntry => _entry;

        public long GetMemorySize()
        {
            return _rawData != null ? _rawData.Length : 0;
        }

        public bool IsInitialized()
        {
            return _rawData != null;
        }

        public virtual void Initialize(CancellationToken token)
        {
            if (IsInitialized()) return;

            using (var stream = _entry.OpenEntry())
            {
                InitializeCore(stream, token);
            }
        }

        protected void InitializeCore(Stream stream, CancellationToken token)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    stream.CopyToAsync(ms, 81920, token).Wait();
                }
                catch (AggregateException ex)
                {
                    token.ThrowIfCancellationRequested();
                    throw ex.InnerException;
                }
                catch
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }

                // ストリームをRawDataとして保持。ArchiveEntryのデータがメモリ上に存在するならばそれを参照する
                _rawData = (_entry.Data as byte[]) ?? ms.ToArray();
            }
        }

        public Stream CreateStream(CancellationToken token)
        {
            if (!IsInitialized())
            {
                Initialize(token);
            }

            return new MemoryStream(_rawData);
        }
    }



    public class PictureNamedStreamSource : PictureStreamSource
    {
        private static PictureStream _pictureStream = new PictureStream();

        public PictureNamedStreamSource(ArchiveEntry entry) : base(entry)
        {
        }

        public string Decoder { get; private set; }

        public override void Initialize(CancellationToken token)
        {
            if (IsInitialized()) return;

            using (var namedStream = _pictureStream.Create(this.ArchiveEntry))
            {
                InitializeCore(namedStream.Stream, token);
                Decoder = namedStream.Name;
            }
        }
    }
}
