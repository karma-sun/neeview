using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// プレイリストに対応したブック読み込み処理。
    /// 複数ファイルの場合には一時プレイリストを作成する。
    /// </summary>
    public static class PlaylistBookLoader
    {
        public static void Load(object sender, string path, bool isRefreshFolderList)
        {
            if (path is null) return;

            if (PlaylistArchive.IsSupportExtension(path))
            {
                LoadPlaylist(sender, path, isRefreshFolderList);
            }
            else
            {
                BookHub.Current.RequestLoad(sender, path, null, BookLoadOption.None, isRefreshFolderList);
            }
        }


        public static string Load(object sender, IEnumerable<string> files, bool isRefreshFolderList)
        {
            var path = CreateLoadPath(files);
            Load(sender, path, isRefreshFolderList);
            return path;
        }

        public static string CreateLoadPath(IEnumerable<string> files)
        {
            if (files is null || !files.Any())
            {
                return null;
            }

            if (files.Count() == 1)
            {
                return files.First();
            }
            else
            {
                return CreateTempPlaylist(Temporary.Current.TempDownloadDirectory, files);
            }
        }

        public static void LoadPlaylist(object sender, string playlistFile, bool isRefreshFolderList)
        {
            Debug.Assert(PlaylistArchive.IsSupportExtension(playlistFile));

            BookHub.Current.RequestLoad(sender, playlistFile, null, BookLoadOption.None, false);
            if (isRefreshFolderList)
            {
                BookshelfFolderList.Current.RequestPlace(new QueryPath(playlistFile), null, FolderSetPlaceOption.UpdateHistory);
            }
        }

        private static string CreateTempPlaylist(string outputDirectory, IEnumerable<string> files)
        {
            string name = DateTime.Now.ToString("yyyyMMddHHmmss") + PlaylistArchive.Extension;
            string path = FileIO.CreateUniquePath(System.IO.Path.Combine(outputDirectory, name));
            PlaylistFile.Save(path, new Playlist(files), true);
            return path;
        }
    }

}
