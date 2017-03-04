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
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// アーカイブのページ
    /// フォルダサムネイル作成用
    /// </summary>
    public class ArchivePage : Page
    {
        public static bool IsAutoRecursive { get; set; } = true;

        private string _entryName;

        //
        private string _thumbnailName;
        public override string ThumbnailName => _thumbnailName;

        // コンストラクタ
        public ArchivePage(ArchiveEntry entry, string entryName)
        {
            Debug.Assert(entry.Archiver == RootArchive.Current); // ルートアーカイブのみサポート

            this.Entry = entry;
            this.Place = entry.EntryName;

            _entryName = entryName;

            _thumbnailName = LoosePath.Combine(Entry.FileSize < 0 ? FullPath : LastName, _entryName);
        }

        // コンストラクタ
        public ArchivePage(ArchiveEntry entry) : this(entry, null)
        {
        }

        //
        public override Page TinyClone()
        {
            return new ArchivePage(Entry, _entryName);
        }

        private TrashBox _trashBox = new TrashBox();

        private ArchiveEntry _thumbnailEntry;

        //
        private void OpenEntry()
        {
            try
            {
                // TODO: 自動再帰する設定
                var archiver = GetStartArchiver(Place); // ##
                if (archiver != null)
                {
                    _trashBox.Add(archiver);
                    _thumbnailEntry = OpenEntry(archiver, _entryName, !archiver.IsFileSystem);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Cannot open {Place}\n{e.Message}");
            }
        }

        /// <summary>
        /// 起点となるアーカイブ取得
        /// 単独アーカイブ、もしくは単独サブフォルダであれば自動再帰する
        /// </summary>
        /// <param name="place">場所</param>
        /// <returns>アーカイブ。作成できなかったときはnull</returns>
        private Archiver GetStartArchiver(string place)
        {
            if (File.Exists(place))
            {
                return ModelContext.ArchiverManager.CreateArchiver(place, null);
            }

            if (!Directory.Exists(place)) return null;

            var files = Directory.GetFiles(place);
            var directories = Directory.GetDirectories(place);

            if (!IsAutoRecursive || files.Length + directories.Length > 1)
            {
                return ModelContext.ArchiverManager.CreateArchiver(place, null);
            }

            else if (directories.Length == 1)
            {
                return GetStartArchiver(directories.First());
            }
            else if (files.Length == 1)
            {
                if (ModelContext.ArchiverManager.IsSupported(files.First()))
                {
                    return ModelContext.ArchiverManager.CreateArchiver(files.First(), null);
                }
                else
                {
                    return ModelContext.ArchiverManager.CreateArchiver(place, null);
                }
            }
            else
            {
                return null; // 空フォルダ
            }
        }

        /// <summary>
        /// アーカイブから指定エントリを取得する
        /// アーカイブの再帰対応(予定)
        /// </summary>
        /// <param name="archiver">アーカイブ</param>
        /// <param name="isRecursive">再帰する/しない</param>
        /// <returns>最初の有効エントリ。見つからない場合はnull</returns>
        private ArchiveEntry OpenEntry(Archiver archiver, string entryName, bool isRecursive)
        {
            if (archiver == null) return null;
            if (entryName == null) return OpenEntry(archiver, isRecursive);

            // アーカイブエントリ取得
            var entries = archiver.GetEntries();

            // 指定の画像ファイル取得
            var entry = entries.FirstOrDefault(e => e.EntryName == entryName);
            if (entry != null) return entry;

            // 再起しないのであればここで終了
            if (!isRecursive) return null;

            // サブフォルダ取得
            var folders = entries.Where(e => ModelContext.ArchiverManager.IsSupported(e.EntryName)).ToList();
            if (folders.Count == 0) return null;

            // 最長一致するサブフォルダ取得
            char[] trims = new char[] { '\\', '/' };
            var folder = folders.Where(e => entryName.StartsWith(e.EntryName.TrimEnd(trims))).OrderBy(e => e.EntryName.Length).FirstOrDefault();
            if (folder == null) return null;


            // フォルダのアーカイブを取得し、再帰する
            var archiverType = ModelContext.ArchiverManager.GetSupportedType(folder.EntryName);
            // x エントリのストリームをアーカイブに渡す。ストリームはアーカイブでDisposeされる
            var tempFile = archiver.ExtractToTemp(folder);
            _trashBox.Add(tempFile as TempFile);
            var subArchiver = ModelContext.ArchiverManager.CreateArchiver(archiverType, tempFile.Path, null, folder);
            _trashBox.Add(subArchiver);

            var childEntryName = entryName.Substring(folder.EntryName.TrimEnd(trims).Length + 1);

            // 再帰
            return OpenEntry(subArchiver, childEntryName, true);
        }



        /// <summary>
        /// アーカイブから最初の有効エントリを取得する
        /// アーカイブの再帰対応(予定)
        /// </summary>
        /// <param name="archiver">アーカイブ</param>
        /// <param name="isRecursive">再帰する/しない</param>
        /// <returns>最初の有効エントリ。見つからない場合はnull</returns>
        private ArchiveEntry OpenEntry(Archiver archiver, bool isRecursive)
        {
            if (archiver == null) return null;

            // アーカイブエントリ取得
            var entries = archiver.GetEntries();

            // 並び替え
            entries = SortEntries(entries, PageSortMode.FileName);

            // 最初の画像ファイル取得
            var entry = entries.FirstOrDefault(e => ModelContext.BitmapLoaderManager.IsSupported(e.EntryName));
            if (entry != null) return entry;

            // 再起しないのであればここで終了
            if (!isRecursive) return null;

            // 最初のフォルダ取得
            entry = entries.FirstOrDefault(e => ModelContext.ArchiverManager.IsSupported(e.EntryName));
            if (entry == null) return null;

            // フォルダのアーカイブを取得し、再帰する
            var archiverType = ModelContext.ArchiverManager.GetSupportedType(entry.EntryName);
            // x エントリのストリームをアーカイブに渡す。ストリームはアーカイブでDisposeされる
            var tempFile = archiver.ExtractToTemp(entry);
            _trashBox.Add(tempFile as TempFile);
            var subArchiver = ModelContext.ArchiverManager.CreateArchiver(archiverType, tempFile.Path, null, entry);
            _trashBox.Add(subArchiver);
            // 再帰
            return OpenEntry(subArchiver, true);
        }


        // エントリを閉じる
        private void CloseEntry()
        {
            if (_thumbnailEntry != null)
            {
                //Entry?.Archiver?.Dispose();
                _thumbnailEntry = null;
            }
            _trashBox.Clear();
        }


        // TODO: 入力されたentriesを変更しないようにする
        public static List<ArchiveEntry> SortEntries(List<ArchiveEntry> entries, PageSortMode sortMode)
        {
            if (entries == null || entries.Count <= 0) return entries;

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    entries.Sort((a, b) => CompareFileNameOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    entries.Sort((a, b) => CompareFileNameOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    entries.Sort((a, b) => CompareDateTimeOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStampDescending:
                    entries.Sort((a, b) => CompareDateTimeOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    entries = entries.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return entries;
        }

        // ファイル名, 日付, ID の順で比較
        private static int CompareFileNameOrder(ArchiveEntry e1, ArchiveEntry e2, Func<string, string, int> compare)
        {
            if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, compare);
            else if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else
                return e1.Id - e2.Id;
        }

        // 日付, ファイル名, ID の順で比較
        private static int CompareDateTimeOrder(ArchiveEntry e1, ArchiveEntry e2, Func<string, string, int> compare)
        {
            if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, compare);
            else
                return e1.Id - e2.Id;
        }

        // ファイル名比較. ディレクトリを優先する
        private static int CompareFileName(string s1, string s2, Func<string, string, int> compare)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return compare(s1, s2);
            else
                return compare(d1, d2);
        }

        // 日付比較。null対応
        private static int CompareDateTime(DateTime? _t1, DateTime? _t2)
        {
            DateTime t1 = _t1 ?? DateTime.MinValue;
            DateTime t2 = _t2 ?? DateTime.MinValue;
            return (t1.Ticks - t2.Ticks < 0) ? -1 : 1;
        }

        // コンテンツをロードする
        protected override object LoadContent()
        {
            try
            {
                OpenEntry();
                if (_thumbnailEntry == null) return null;

                var loader = new BitmapContentLoader();
                loader.LoadContent(_thumbnailEntry, true);
                this.Width = loader.Width;
                this.Height = loader.Height;
                this.Color = loader.Color;
                return loader.Content;
            }
            finally
            {
                CloseEntry();
            }
        }
    }
}
