// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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


        /// <summary>
        /// ファイルを削除
        /// </summary>
        /// <param name="info"></param>
        public async Task RemoveAsync(FolderItem info)
        {
            if (info.IsEmpty) return;

            if (Preference.Current.file_remove_confirm)
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

                if (answer != UICommands.Remove) return;
            }

            // TODO: BookHubの役割を減らす
            await BookHub.Current.RemoveFileAsync(info.Path);
        }


        /// <summary>
        /// ファイル名前変更
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<bool> RenameAsync(FolderItem file, string newName)
        {
            newName = newName?.Trim().TrimEnd(' ', '.');

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
            if (src == dst) return true;

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
            // TODO: BookHubの役割を減らす
            var result = await BookHub.Current.RenameFileAsync(src, dst);
            return result;
        }

    }
}
