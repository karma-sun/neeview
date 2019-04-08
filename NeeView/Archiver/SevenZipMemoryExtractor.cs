using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeeView
{
    public class SevenZipMemoryExtractor
    {
        #region inner class
        private struct StreamInfo
        {
            public StreamInfo(ArchiveFileInfo fileInfo, MemoryStream stream)
            {
                FileInfo = fileInfo;
                Stream = stream;
            }

            public ArchiveFileInfo FileInfo { get; }
            public MemoryStream Stream { get; }
        }
        #endregion

        private Dictionary<int, StreamInfo> _map;

        public event EventHandler<SevenZipMemoryExtractionArgs> TempFileExtractionFinished;

        public void ExtractArchive(SevenZipExtractor extractor)
        {
            _map = new Dictionary<int, StreamInfo>();

            try
            {
                extractor.FileExtractionFinished += Extractor_FileExtractionFinished;
                extractor.ExtractArchive(GetStreamFunc);
            }
            finally
            {
                extractor.FileExtractionFinished -= Extractor_FileExtractionFinished;
            }
        }

        private void Extractor_FileExtractionFinished(object sender, FileInfoEventArgs e)
        {
            if (_map.TryGetValue(e.FileInfo.Index, out StreamInfo item))
            {
                _map.Remove(e.FileInfo.Index);

                TempFileExtractionFinished?.Invoke(sender, new SevenZipMemoryExtractionArgs(item.FileInfo, item.Stream.ToArray()));
                item.Stream.Dispose();
            }
        }

        private Stream GetStreamFunc(ArchiveFileInfo info)
        {
            ////Debug.WriteLine($"{info.Index}: {info.FileName}");
            var stream = new MemoryStream();
            _map.Add(info.Index, new StreamInfo(info, stream));

            return stream;
        }
    }
}


