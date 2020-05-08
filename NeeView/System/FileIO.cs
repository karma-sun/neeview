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

        public static FileIO Current { get; } = new FileIO();

        //
        public FileIO()
        {
        }

        // ファイル削除
        public static void RemoveFile(string filename)
        {
            new FileInfo(filename).Delete();
        }

        // ファイルかディレクトリの存在チェック
        public bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        /// <param name="info"></param>
        public void CopyToClipboard(FolderItem info)
        {
            if (info.IsEmpty()) return;

            var files = new List<string>();
            files.Add(info.EntityPath.SimplePath);
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, files.ToArray());
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", files));
            Clipboard.SetDataObject(data);
        }

        public void CopyToClipboard(IEnumerable<FolderItem> infos)
        {
            var collection = new System.Collections.Specialized.StringCollection();
            foreach (var item in infos.Where(e => !e.IsEmpty()).Select(e => e.EntityPath.SimplePath).Where(e => new QueryPath(e).Scheme == QueryScheme.File))
            {
                collection.Add(item);
            }

            if (collection.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetFileDropList(collection);
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", collection));
            Clipboard.SetDataObject(data);
        }


        #region Remove

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
            return await RemoveFileAsync(page.GetFilePlace(), Resources.DialogFileDeletePageTitle, thumbnail);
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
                return await RemoveFileAsync(pages.Select(e => e.GetFilePlace()).ToList(), Resources.DialogFileDeletePageTitle);
            }
        }

        /// <summary>
        /// ファイル削除
        /// </summary>
        public async Task<bool> RemoveFileAsync(string path, string title, FrameworkElement thumbnail)
        {
            var content = CreateRemoveDialogContent(path, thumbnail);
            if (ConfirmRemove(content, title ?? GetRemoveDialogTitle(path)))
            {
                return await RemoveCoreAsync(new List<string>() { path });
            }
            else
            {
                return false;
            }
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
            else
            {
                var content = CreateRemoveDialogContent(paths);
                if (ConfirmRemove(content, title ?? GetRemoveDialogTitle(paths)))
                {
                    return await RemoveCoreAsync(paths);
                }
                else
                {
                    return false;
                }
            }
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
            message.Text = string.Format(Resources.DialogFileDelete, GetRemoveFilesTypeName(path));
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
            return string.Format(Resources.DialogFileDeleteTitle, GetRemoveFilesTypeName(path));
        }

        private string GetRemoveDialogTitle(List<string> paths)
        {
            return string.Format(Resources.DialogFileDeleteTitle, GetRemoveFilesTypeName(paths));
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
            message.Text = string.Format(Resources.DialogFileDeleteMulti, paths.Count);
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
                    await BookHub.Current.RequestUnload(true).WaitAsync();
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
                var dialog = new MessageDialog($"{Resources.WordCause}: {ex.Message}", Resources.DialogFileDeleteFailed);
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
                var dialog = new MessageDialog(Resources.DialogFileRenameWrong, Resources.DialogFileRenameErrorTitle);
                dialog.ShowDialog();
                return null;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());

                var dialog = new MessageDialog($"{Resources.DialogFileRenameInvalid}\n\n{invalids}", Resources.DialogFileRenameErrorTitle);
                dialog.ShowDialog();

                return null;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                var dialog = new MessageDialog($"{Resources.DialogFileRenameWrongDevice}\n\n{match.Groups[1].Value.ToUpper()}", Resources.DialogFileRenameErrorTitle);
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
                    var dialog = new MessageDialog(Resources.DialogFileRenameExtension, Resources.DialogFileRenameExtensionTitle);
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
                var dialog = new MessageDialog(string.Format(Resources.DialogFileRenameConfrict, Path.GetFileName(dstBase), Path.GetFileName(dst)), Resources.DialogFileRenameConfrictTitle);
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
                    await _bookHub.RequestUnload(false).WaitAsync();
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
                        _bookHub.RequestLoad(dst, null, BookLoadOption.Resume | BookLoadOption.IsBook, false);
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

                var confirm = new MessageDialog($"{Resources.DialogFileRenameFailed}\n\n{ex.Message}", Resources.DialogFileRenameFailedTitle);
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

    }
}
