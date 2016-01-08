// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバ：Susieアーカイバ
    /// </summary>
    public class SusieArchiver : Archiver
    {
        private string _ArchiveFileName;
        public override string FileName => _ArchiveFileName;

        Dictionary<string, Susie.ArchiveEntry> _ArchiveFileInfoDictionary;

        private object _Lock = new object();


        //
        public SusieArchiver(string archiveFileName)
        {
            _ArchiveFileName = archiveFileName;
        }


        // エントリーリストを得る
        public override List<ArchiveEntry> GetEntries()
        {
            var infoCollection = ModelContext.Susie.GetArchiveInfo(_ArchiveFileName);

            if (infoCollection == null) throw new NotSupportedException();

            _ArchiveFileInfoDictionary = new Dictionary<string, Susie.ArchiveEntry>();
            List<ArchiveEntry> entries = new List<ArchiveEntry>();
            foreach (var entry in infoCollection)
            {
                try
                {
                    string name = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/');

                    entries.Add(new ArchiveEntry()
                    {
                        Path = name, //  entry.FileName,
                        UpdateTime = entry.TimeStamp,
                    });

                    _ArchiveFileInfoDictionary.Add(name, entry);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }

            return entries;
        }


        // エントリーのストリームを得る
        public override Stream OpenEntry(string entryName)
        {
            lock (_Lock)
            {
                var info = _ArchiveFileInfoDictionary[entryName];
                byte[] buffer = info.Load();
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        //
        public override void ExtractToFile(string entryName, string extractFileName)
        {
            var info = _ArchiveFileInfoDictionary[entryName];

            // susieプラグインでは出力ファイル名を指定できないので、
            // テンポラリフォルダに出力してから移動する
            string tempDirectory = Temporary.CreateCountedTempFileName("Susie", "");
            Directory.CreateDirectory(tempDirectory);

            info.ExtractToFolder(tempDirectory);

            try
            {
                var files = Directory.GetFiles(tempDirectory);
                File.Move(files[0], extractFileName);
                Directory.Delete(tempDirectory, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }

}
