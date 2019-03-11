using SevenZip;
using System;

namespace NeeView
{
    /// <summary>
    /// SevenZipTempFileExtractor用イベント引数
    /// </summary>
    public class SevenZipTempFileExtractionArgs : EventArgs
    {
        public SevenZipTempFileExtractionArgs(ArchiveFileInfo fileInfo, string fileName)
        {
            FileInfo = fileInfo;
            FileName = fileName;
        }

        public ArchiveFileInfo FileInfo { get; }
        public string FileName { get; }
    }
}


