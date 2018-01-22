// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static FileIO Current { get; } = new FileIO();

        //
        public FileIO()
        {
        }

        // ファイル削除
        public void RemoveFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }
        }


        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        /// <param name="info"></param>
        public void CopyToClipboard(FolderItem info)
        {
            if (info.IsEmpty) return;

            var files = new List<string>();
            files.Add(info.Path);
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, files.ToArray());
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", files));
            Clipboard.SetDataObject(data);
        }


        #region Remove

        /// <summary>
        /// ファイルを削除
        /// </summary>
        /// <param name="info"></param>
        public async Task<bool> RemoveAsync(FolderItem info)
        {
            if (info.IsEmpty) return false;

            if (FileIOProfile.Current.IsRemoveConfirmed)
            {
                bool isDirectory = System.IO.Directory.Exists(info.Path);
                string itemType = isDirectory ? "フォルダー" : "ファイル";

                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = $"この{itemType}をごみ箱に移動しますか？";
                message.Margin = new Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                var thumbnail = new Image();
                thumbnail.SnapsToDevicePixels = true;
                thumbnail.Source = info.Icon;
                thumbnail.Width = 32;
                thumbnail.Height = 32;
                thumbnail.Margin = new Thickness(0, 0, 4, 0);
                dockPanel.Children.Add(thumbnail);

                var textblock = new TextBlock();
                textblock.Text = info.Path;
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                textblock.Margin = new Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                //
                var dialog = new MessageDialog(dockPanel, $"{itemType}を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Remove) return false;
            }

            return await RemoveFileAsync(info.Path);
        }

        // ファイル削除可能？
        public bool CanRemoveFile(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;
            return (File.Exists(page.GetFilePlace()));
        }

        // ファイルを削除する
        public async Task RemoveFile(Page page)
        {
            if (page == null) return;

            var path = page.GetFilePlace();

            if (FileIOProfile.Current.IsRemoveConfirmed)
            {
                bool isDirectory = System.IO.Directory.Exists(path);
                string itemType = isDirectory ? "フォルダー" : "ファイル";

                // ビジュアル作成
                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = $"この{itemType}をごみ箱に移動しますか？";
                message.Margin = new System.Windows.Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                var thumbnail = await new PageVisual(page).CreateVisualContentAsync(new System.Windows.Size(64, 64), true);
                if (thumbnail != null)
                {
                    thumbnail.Margin = new System.Windows.Thickness(0, 0, 20, 0);
                    dockPanel.Children.Add(thumbnail);
                }

                var textblock = new TextBlock();
                textblock.Text = Path.GetFileName(path);
                textblock.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                textblock.Margin = new System.Windows.Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                //
                var dialog = new MessageDialog(dockPanel, $"{itemType}を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Remove) return;
            }

            // 削除実行
            bool isRemoved = await RemoveFileAsync(path);

            var book = BookHub.Current.Book;

            // ページを本から削除
            if (isRemoved == true && book != null)
            {
                book.RequestRemove(this, page);
            }
        }


        // ファイルを削除する
        public async Task<bool> RemoveFileAsync(string path)
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
                }

                // ゴミ箱に捨てる
                bool isDirectory = System.IO.Directory.Exists(path);
                if (isDirectory)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
                else
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }

                //
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
                else
                {
                    var dialog = new MessageDialog($"削除できませんでした。もう一度実行しますか？\n\n原因: {ex.Message}", "削除できませんでした。リトライしますか？");
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

        /// <summary>
        /// リネーム用にトリミングされたファイル名生成
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static string FixedRenameFileName(string newName)
        {
            return newName?.Trim().TrimEnd(' ', '.');
        }

        /// <summary>
        /// リネーム用パス生成
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static string FixedRenamePath(FolderItem file, string newName)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file.Path), newName);
        }

        /// <summary>
        /// ファイル名前変更
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<bool> RenameAsync(FolderItem file, string newName)
        {
            newName = FixedRenameFileName(newName);

            // ファイル名に使用できない
            if (string.IsNullOrWhiteSpace(newName))
            {
                var dialog = new MessageDialog($"指定されたファイル名は無効です。", "名前を変更できません");
                dialog.ShowDialog();
                return false;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());

                var dialog = new MessageDialog($"ファイル名に使用できない文字が含まれています。\n\n{invalids}", "名前を変更できません");
                dialog.ShowDialog();

                return false;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                var dialog = new MessageDialog($"指定されたデバイス名は無効です。\n\n{match.Groups[1].Value.ToUpper()}", "名前を変更できません");
                dialog.ShowDialog();
                return false;
            }

            string src = file.Path;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            // 全く同じ名前なら処理不要
            if (src == dst) return false;

            // 拡張子変更確認
            if (!file.IsDirectory)
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    var dialog = new MessageDialog($"拡張子を変更すると、使えなくなる可能性があります。\nよろしいですか？", "拡張子を変更します");
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var answer = dialog.ShowDialog();
                    if (answer != UICommands.Yes)
                    {
                        return false;
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
                var dialog = new MessageDialog($"{System.IO.Path.GetFileName(dstBase)} は既に存在しています。\n{System.IO.Path.GetFileName(dst)} に名前を変更しますか？", "同じ名前のファイルが存在しています");
                dialog.Commands.Add(new UICommand("名前を変える"));
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != dialog.Commands[0])
                {
                    return false;
                }
            }

            // 名前変更実行
            var result = await RenameFileAsync(src, dst);
            return result;
        }


        // ファイルの名前を変える
        public async Task<bool> RenameFileAsync(string src, string dst)
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
                }

                // rename
                if (System.IO.Directory.Exists(src))
                {
                    System.IO.Directory.Move(src, dst);
                }
                else
                {
                    System.IO.File.Move(src, dst);
                }

                // 閉じた本を開き直す
                if (isContinue && requestLoadCount == _bookHub.RequestLoadCount)
                {
                    RenameHistory(src, dst);
                    _bookHub.RequestLoad(dst, null, BookLoadOption.Resume, false);
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

                var confirm = new MessageDialog($"名前の変更に失敗しました。もう一度実行しますか？\n\n{ex.Message}", "名前を変更できませんでした");
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
            PagemarkCollection.Current.Rename(src, dst);
        }

        #endregion

    }
}
