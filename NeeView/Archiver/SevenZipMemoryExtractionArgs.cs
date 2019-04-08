using SevenZip;
using System;

namespace NeeView
{
    public class SevenZipMemoryExtractionArgs : EventArgs
    {
        public SevenZipMemoryExtractionArgs(ArchiveFileInfo fileInfo, byte[] rawData)
        {
            FileInfo = fileInfo;
            RawData = rawData;
        }

        public ArchiveFileInfo FileInfo { get; }
        public byte[] RawData { get; }
    }
}


