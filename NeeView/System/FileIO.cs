using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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


        #region Remove

        // ページ削除可能？
        public bool CanRemovePage(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return (File.Exists(path) || Directory.Exists(path));
        }

        // ページを削除する
        public async Task<bool> RemovePageAsync(Page page)
        {
            if (page == null) return false;

            bool isRemoved = await RemoveAsync(page.GetFilePlace(), Resources.DialogFileDeletePageTitle, async () => await new PageVisual(page).CreateVisualContentAsync(new System.Windows.Size(64, 64), true));
            return isRemoved;
        }

        // ファイルを削除 確認と削除
        public async Task<bool> RemoveAsync(string path, string title = null, Func<Task<FrameworkElement>> createThumbnailAsync = null)
        {
            if (path == null) return false;

            var confirm = await RemoveConfirmAsync(path, title, createThumbnailAsync);
            if (confirm == false) return false;

            return await RemoveAsyncInner(path);
        }

        // ファイルを削除 確認
        public async Task<bool> RemoveConfirmAsync(string path, string title = null, Func<Task<FrameworkElement>> createThumbnailAsync = null)
        {
            if (path == null) return false;

            bool isFile = System.IO.File.Exists(path);
            bool isDirectory = System.IO.Directory.Exists(path);
            if (!isFile && !isDirectory) return false;

            if (FileIOProfile.Current.IsRemoveConfirmed)
            {
                string typeName = isDirectory ? Resources.WordFolder : Resources.WordFile;

                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = string.Format(Resources.DialogFileDelete, typeName);
                message.Margin = new Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                FrameworkElement thumbnail = null;
                if (createThumbnailAsync != null)
                {
                    thumbnail = await createThumbnailAsync();
                }
                if (thumbnail == null)
                {
                    thumbnail = new Image
                    {
                        SnapsToDevicePixels = true,
                        Source = NeeLaboratory.IO.FileSystem.GetTypeIconSource(path, NeeLaboratory.IO.FileSystem.IconSize.Normal),
                        Width = 32,
                        Height = 32
                    };
                }

                if (thumbnail != null)
                {
                    thumbnail.Margin = new Thickness(0, 0, 10, 0);
                    dockPanel.Children.Add(thumbnail);
                }

                var textblock = new TextBlock();
                textblock.Text = path;
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                textblock.TextWrapping = TextWrapping.Wrap;
                textblock.Margin = new Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                var dialog = new MessageDialog(dockPanel, title ?? string.Format(Resources.DialogFileDeleteTitle, typeName));
                dialog.Commands.Add(UICommands.Delete);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Delete) return false;
            }

            return true;
        }

        // ファイルを削除 コア
        private async Task<bool> RemoveAsyncInner(string path)
        {
            var _bookHub = BookHub.Current;
            int retryCount = 1;

        Retry:

            try
            {
                // 開いている本であるならば閉じる
                if (_bookHub.Address == path)
                {
                    await _bookHub.RequestUnload(true).WaitAsync();
                    ArchiverManager.Current.UnlockAllArchives();
                }

                // ゴミ箱に捨てる
                bool isDirectory = System.IO.Directory.Exists(path);
                if (isDirectory)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
                else
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }
                else
                {
                    var dialog = new MessageDialog($"{Resources.DialogFileDeleteError}\n\n{Resources.WordCause}: {ex.Message}", Resources.DialogFileDeleteErrorTitle);
                    dialog.Commands.Add(UICommands.Retry);
                    dialog.Commands.Add(UICommands.Cancel);
                    var confirm = dialog.ShowDialog();
                    if (confirm == UICommands.Retry)
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
        }

        #endregion


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

                // 開いている本であるならば閉じる
                if (_bookHub.Address == src)
                {
                    isContinue = true;
                    await _bookHub.RequestUnload(false).WaitAsync();
                    ArchiverManager.Current.UnlockAllArchives();
                }

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
