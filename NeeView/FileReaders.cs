using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IFileReader : IDisposable
    {
        Stream Stream { get; }
    }

    public enum FileReaderType
    {
        File,
        FileOnLoad,
        ZipArchiveEntry,
    }

    public static class FileReaderFactory
    {
        public static IFileReader OpenRead(FileReaderType type, string fileName, Archiver archiver)
        {
            switch (type)
            {
                case FileReaderType.File:
                    return new FileReader(fileName);

                case FileReaderType.FileOnLoad:
                    return new FileOnLoadReader(fileName);

                case FileReaderType.ZipArchiveEntry:
                    return new ZipArchiveEntryReader(fileName, archiver);

                default:
                    throw new ArgumentException("no support FileReaderType.", nameof(type));
            }
        }
    }

    // FileStream
    public class FileReader : IFileReader
    {
        public Stream Stream { get; private set; }

        public FileReader(string fileName)
        {
            Stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }
        }
    }


    // FileStream
    public class FileOnLoadReader : IFileReader
    {
        public Stream Stream { get; private set; }

        public FileOnLoadReader(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                Stream = new MemoryStream();
                fs.CopyTo(Stream);
                Stream.Seek(0, SeekOrigin.Begin);
            }
        }

        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }
        }
    }

    // ZipArchiveEntry Stream
    public class ZipArchiveEntryReader : IFileReader
    {
        public Stream Stream { get; private set; }
        private ZipArchive _Archive;

        public ZipArchiveEntryReader(string entryName, Archiver archiver)
        {
            Stream = archiver.OpenEntry(entryName);
#if false
            _Archive = ZipFile.OpenRead(archiveFileName);

            ZipArchiveEntry entry = _Archive.GetEntry(entryName);
            if (entry == null) throw new ArgumentException($"アーカイブエントリ {entryName} が見つかりません");

            Stream = new MemoryStream();
            entry.Open().CopyTo(Stream);
            Stream.Seek(0, SeekOrigin.Begin);
#endif
        }

        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }

            if (_Archive != null)
            {
                _Archive.Dispose();
                _Archive = null;
            }
        }
    }



}
