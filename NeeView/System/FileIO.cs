using NeeView.IO;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Effects;

// TODO: 要整備。表示やフロー等も含まれてしまっている。依存関係が強すぎる

namespace NeeView
{
    /// <summary>
    /// File I/O
    /// </summary>
    public class FileIO
    {
        private class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool MoveFile(string lpExistingFileName, string lpNewFileName);
        }

        static FileIO() => Current = new FileIO();
        public static FileIO Current { get; }




        // ファイルかディレクトリの存在チェック
        public static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }


        /// <summary>
        /// パスの衝突を連番をつけて回避
        /// </summary>
        public static string CreateUniquePath(string source)
        {
            if (!Exists(source))
            {
                return source;
            }

            var path = source;

            bool isFile = File.Exists(path);
            var directory = Path.GetDirectoryName(path);
            var filename = isFile ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
            var extension = isFile ? Path.GetExtension(path) : "";
            int count = 1;

            var regex = new Regex(@"^(.+)\((\d+)\)$");
            var match = regex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value);
            }

            do
            {
                path = Path.Combine(directory, $"{filename} ({++count}){extension}");
            }
            while (Exists(path));

            return path;
        }

        /// <summary>
        /// ディレクトリが親子関係にあるかをチェック
        /// </summary>
        /// <returns></returns>
        public static bool IsSubDirectoryRelationship(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == dir2) return true;

            var path1 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir1.FullName)).ToUpperInvariant();
            var path2 = LoosePath.TrimDirectoryEnd(LoosePath.NormalizeSeparator(dir2.FullName)).ToUpperInvariant();
            if (path1.Length < path2.Length)
            {
                return path2.StartsWith(path1);
            }
            else
            {
                return path1.StartsWith(path2);
            }
        }

        /// <summary>
        /// DirectoryInfoの等価判定
        /// </summary>
        public static bool DirectoryEquals(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1 == null && dir2 == null) return true;
            if (dir1 == null || dir2 == null) return false;

            var path1 = LoosePath.NormalizeSeparator(dir1.FullName).TrimEnd(LoosePath.Separator).ToUpperInvariant();
            var path2 = LoosePath.NormalizeSeparator(dir2.FullName).TrimEnd(LoosePath.Separator).ToUpperInvariant();
            return path1 == path2;
        }

        #region Copy

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーにコピーする
        /// </summary>
        public static void CopyToFolder(IEnumerable<string> froms, string toDirectory)
        {
            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Copy(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Copy

        #region Move

        /// <summary>
        /// ファイル、ディレクトリーを指定のフォルダーに移動する
        /// </summary>
        public static void MoveToFolder(IEnumerable<string> froms, string toDirectory)
        {

            var toDirPath = LoosePath.TrimDirectoryEnd(toDirectory);

            var dir = new DirectoryInfo(toDirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            ShellFileOperation.Move(App.Current.MainWindow, froms, toDirPath);
        }

        #endregion Move

        #region Remove

        // ファイル削除 (Direct)
        public static void RemoveFile(string filename)
        {
            new FileInfo(filename).Delete();
        }


        /// <summary>
        /// ページ削除済？
        /// </summary>
        public bool IsPageRemoved(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return !(File.Exists(path) || Directory.Exists(path));
        }


        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public bool CanRemovePage(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return (File.Exists(path) || Directory.Exists(path));
        }

        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public bool CanRemovePage(List<Page> pages)
        {
            return pages.All(e => CanRemovePage(e));
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public async Task<bool> RemovePageAsync(Page page)
        {
            if (page == null)
            {
                return false;
            }

            var thumbnail = await CreatePageVisualAsync(page);
            return await RemoveFileAsync(page.GetFilePlace(), Resources.FileDeletePageDialog_Title, thumbnail);
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public async Task<bool> RemovePageAsync(List<Page> pages)
        {
            if (pages == null || pages.Count == 0)
            {
                return false;
            }

            if (pages.Count == 1)
            {
                return await RemovePageAsync(pages.First());
            }
            else
            {
                return await RemoveFileAsync(pages.Select(e => e.GetFilePlace()).ToList(), Resources.FileDeletePageDialog_Title);
            }
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public async Task<bool> RemoveFileAsync(string path, string title, FrameworkElement thumbnail)
        {
            if (Config.Current.System.IsRemoveConfirmed)
            {
                var content = CreateRemoveDialogContent(path, thumbnail);
                if (!ConfirmRemove(content, title ?? GetRemoveDialogTitle(path)))
                {
                    return false;
                }
            }

            return await RemoveCoreAsync(new List<string>() { path });
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public async Task<bool> RemoveFileAsync(List<string> paths, string title)
        {
            if (paths != null && !paths.Any())
            {
                return false;
            }
            if (paths.Count == 1)
            {
                return await RemoveFileAsync(paths.First(), title, null);
            }
            if (Config.Current.System.IsRemoveConfirmed)
            {
                var content = CreateRemoveDialogContent(paths);
                if (!ConfirmRemove(content, title ?? GetRemoveDialogTitle(paths)))
                {
                    return false;
                }
            }

            return await RemoveCoreAsync(paths);
        }

        /// <summary>
        /// ページからダイアログ用サムネイル作成
        /// </summary>
        private async Task<Image> CreatePageVisualAsync(Page page)
        {
            var imageSource = await page.LoadThumbnailAsync(CancellationToken.None);

            var image = new Image();
            image.Source = imageSource;
            image.Effect = new DropShadowEffect()
            {
                Opacity = 0.5,
                ShadowDepth = 2,
                RenderingBias = RenderingBias.Quality
            };
            image.MaxWidth = 96;
            image.MaxHeight = 96;

            return image;
        }

        /// <summary>
        /// ファイルからダイアログ用サムネイル作成
        /// </summary>
        private Image CreateFileVisual(string path)
        {
            return new Image
            {
                SnapsToDevicePixels = true,
                Source = NeeLaboratory.IO.FileSystem.GetTypeIconSource(path, NeeLaboratory.IO.FileSystem.IconSize.Normal),
                Width = 32,
                Height = 32,
            };
        }

        /// <summary>
        /// 1ファイル用確認ダイアログコンテンツ
        /// </summary>
        private FrameworkElement CreateRemoveDialogContent(string path, FrameworkElement thumbnail)
        {
            var dockPanel = new DockPanel();

            var message = new TextBlock();
            message.Text = string.Format(Resources.FileDeleteDialog_Message, GetRemoveFilesTypeName(path));
            message.Margin = new Thickness(0, 0, 0, 10);
            DockPanel.SetDock(message, Dock.Top);
            dockPanel.Children.Add(message);

            if (thumbnail == null)
            {
                thumbnail = CreateFileVisual(path);
            }

            thumbnail.Margin = new Thickness(0, 0, 10, 0);
            dockPanel.Children.Add(thumbnail);

            var textblock = new TextBlock();
            textblock.Text = path;
            textblock.VerticalAlignment = VerticalAlignment.Bottom;
            textblock.TextWrapping = TextWrapping.Wrap;
            textblock.Margin = new Thickness(0, 0, 0, 2);
            dockPanel.Children.Add(textblock);

            return dockPanel;
        }

        private string GetRemoveDialogTitle(string path)
        {
            return string.Format(Resources.FileDeleteDialog_Title, GetRemoveFilesTypeName(path));
        }

        private string GetRemoveDialogTitle(List<string> paths)
        {
            return string.Format(Resources.FileDeleteDialog_Title, GetRemoveFilesTypeName(paths));
        }

        private string GetRemoveFilesTypeName(string path)
        {
            bool isDirectory = System.IO.Directory.Exists(path);
            return isDirectory ? Resources.WordFolder : Resources.WordFile;
        }

        private string GetRemoveFilesTypeName(List<string> paths)
        {
            if (paths.Count == 1)
            {
                return GetRemoveFilesTypeName(paths.First());
            }

            bool isDirectory = paths.All(e => System.IO.Directory.Exists(e));
            return isDirectory ? Resources.WordFolders : Resources.WordFiles;
        }

        /// <summary>
        /// 複数ファイル用確認ダイアログコンテンツ
        /// </summary>
        private FrameworkElement CreateRemoveDialogContent(List<string> paths)
        {
            var message = new TextBlock();
            message.Text = string.Format(Resources.FileDeleteMultiDialog_Message, paths.Count);
            message.Margin = new Thickness(0, 10, 0, 10);
            DockPanel.SetDock(message, Dock.Top);

            return message;
        }

        /// <summary>
        /// 削除確認
        /// </summary>
        private bool ConfirmRemove(FrameworkElement content, string title)
        {
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(UICommands.Delete);
            dialog.Commands.Add(UICommands.Cancel);
            var answer = dialog.ShowDialog();

            return (answer == UICommands.Delete);
        }

        /// <summary>
        /// 削除メイン
        /// </summary>
        private async Task<bool> RemoveCoreAsync(List<string> paths)
        {
            try
            {
                // 開いている本であるならば閉じる
                if (paths.Contains(BookHub.Current.Address))
                {
                    await BookHub.Current.RequestUnload(this, true).WaitAsync();
                    await ArchiverManager.Current.UnlockAllArchivesAsync();
                }

                ShellFileOperation.Delete(Application.Current.MainWindow, paths, Config.Current.System.IsRemoveWantNukeWarning);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"{Resources.WordCause}: {ex.Message}", Resources.FileDeleteErrorDialog_Title);
                dialog.ShowDialog();
                return false;
            }
        }

        #endregion Remove

        #region Rename

        // ファイル名前変更
        public async Task<string> RenameAsync(FolderItem file, string newName)
        {
            newName = newName?.Trim().TrimEnd(' ', '.');

            // ファイル名に使用できない
            if (string.IsNullOrWhiteSpace(newName))
            {
                var dialog = new MessageDialog(Resources.FileRenameWrongDialog_Message, Resources.FileRenameErrorDialog_Title);
                dialog.ShowDialog();
                return null;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());

                var dialog = new MessageDialog($"{Resources.FileRenameInvalidDialog_Message}\n\n{invalids}", Resources.FileRenameErrorDialog_Title);
                dialog.ShowDialog();

                return null;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                var dialog = new MessageDialog($"{Resources.FileRenameWrongDeviceDialog_Message}\n\n{match.Groups[1].Value.ToUpper()}", Resources.FileRenameErrorDialog_Title);
                dialog.ShowDialog();
                return null;
            }

            string src = file.TargetPath.SimplePath;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            // 全く同じ名前なら処理不要
            if (src == dst) return null;

            // 拡張子変更確認
            if (!file.IsDirectory)
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    var dialog = new MessageDialog(Resources.FileRenameExtensionDialog_Message, Resources.FileRenameExtensionDialog_Title);
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var answer = dialog.ShowDialog();
                    if (answer != UICommands.Yes)
                    {
                        return null;
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, true) == 0)
            {
                // nop.
            }

            // 重複ファイル名回避
            else if (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst))
            {
                string dstBase = dst;
                string dir = System.IO.Path.GetDirectoryName(dst);
                string name = System.IO.Path.GetFileNameWithoutExtension(dst);
                string ext = System.IO.Path.GetExtension(dst);
                int count = 1;

                do
                {
                    dst = $"{dir}\\{name} ({++count}){ext}";
                }
                while (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst));

                // 確認
                var dialog = new MessageDialog(string.Format(Resources.FileRenameConfrictDialog_Message, Path.GetFileName(dstBase), Path.GetFileName(dst)), Resources.FileRenameConfrictDialog_Title);
                dialog.Commands.Add(new UICommand(Resources.WordRename));
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != dialog.Commands[0])
                {
                    return null;
                }
            }

            // 名前変更実行
            var result = await RenameAsyncInner(src, dst);

            return result ? dst : null;
        }


        // ファイル名前変更 コア
        private async Task<bool> RenameAsyncInner(string src, string dst)
        {
            var _bookHub = BookHub.Current;
            int retryCount = 1;

        Retry:

            try
            {
                bool isContinue = false;
                int requestLoadCount = _bookHub.RequestLoadCount;

                // 開いている本であるならば再び開くようにする
                if (_bookHub.Address == src)
                {
                    isContinue = true;
                    await _bookHub.RequestUnload(this, false).WaitAsync();
                }

                // 開いている本のロックをはずす
                await ArchiverManager.Current.UnlockAllArchivesAsync();

                // rename
                try
                {
                    if (System.IO.Directory.Exists(src))
                    {
                        System.IO.Directory.Move(src, dst);
                    }
                    else if (System.IO.File.Exists(src))
                    {
                        System.IO.File.Move(src, dst);
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
                catch (IOException) when (string.Compare(src, dst, true) == 0)
                {
                    // 大文字小文字の違いだけである場合はWIN32APIで処理する
                    NativeMethods.MoveFile(src, dst);
                }


                try
                {
                    // 閉じた本を開き直す
                    if (isContinue && requestLoadCount == _bookHub.RequestLoadCount)
                    {
                        RenameHistory(src, dst);
                        _bookHub.RequestLoad(this, dst, null, BookLoadOption.Resume | BookLoadOption.IsBook, false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }

                var confirm = new MessageDialog($"{Resources.FileRenameFailedDialog_Message}\n\n{ex.Message}", Resources.FileRenameFailedDialog_Title);
                confirm.Commands.Add(UICommands.Retry);
                confirm.Commands.Add(UICommands.Cancel);
                var answer = confirm.ShowDialog();
                if (answer == UICommands.Retry)
                {
                    retryCount = 1;
                    goto Retry;
                }
                else
                {
                    return false;
                }
            }

        }

        // 履歴上のファイル名変更
        private void RenameHistory(string src, string dst)
        {
            BookMementoCollection.Current.Rename(src, dst);
        }

        #endregion


        #region Dialogs

        public class Win32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Win32Window(Window window)
            {
                this.Handle = new WindowInteropHelper(window).Handle;
            }
        }


        /// <summary>
        /// フォルダー選択ダイアログ
        /// </summary>
        public static string OpenFolderBrowserDialog(Window owner, string description)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = description;

            var result = dialog.ShowDialog(new Win32Window(owner));
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else
            {
                return null;
            }
        }

        #endregion Dialogs
    }
}
