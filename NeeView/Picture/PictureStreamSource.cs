using System;
using System.Diagnostics;
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
                InitializeCore(stream, _entry.GetRawData(), token);
            }
        }


        private void CopyStream(Stream inputStream, Stream outputStream, int bufferSize, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var buffer = new byte[bufferSize];

            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                token.ThrowIfCancellationRequested();
            }
        }

        protected void InitializeCore(Stream stream, byte[] rawData, CancellationToken token)
        {
            if (rawData != null)
            {
                ////Debug.WriteLine($"PictureStreamSource: {_entry.EntryLastName} from RawData");
                _rawData = rawData;
            }
            else
            {
                ////Debug.WriteLine($"PictureStreamSource: {_entry.EntryLastName} from Stream");
                using (var ms = new MemoryStream())
                {
                    CopyStream(stream, ms, 81920, token);
                    _rawData = ms.ToArray();
                }
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
                InitializeCore(namedStream.Stream, namedStream.RawData, token);
                Decoder = namedStream.Name;
            }
        }
    }


}
