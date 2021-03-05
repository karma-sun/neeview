using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    public class PageCommandResource
    {
        public CommandBinding CreateCommandBinding(RoutedCommand command, string key = null)
        {
            key = key ?? command.Name;
            switch (key)
            {
                case "OpenCommand":
                    return new CommandBinding(command, Open_Exec, Open_CanExec);
                case "OpenBookCommand":
                    return new CommandBinding(command, OpenBook_Exec, OpenBook_CanExec);
                case "OpenExplorerCommand":
                    return new CommandBinding(command, OpenExplorer_Executed, OpenExplorer_CanExecute);
                case "OpenExternalAppCommand":
                    return new CommandBinding(command, OpenExternalApp_Executed, OpenExternalApp_CanExecute);
                case "CopyCommand":
                    return new CommandBinding(command, Copy_Exec, Copy_CanExec);
                case "CopyToFolderCommand":
                    return new CommandBinding(command, CopyToFolder_Execute, CopyToFolder_CanExecute);
                case "MoveToFolderCommand":
                    return new CommandBinding(command, MoveToFolder_Execute, MoveToFolder_CanExecute);
                case "RemoveCommand":
                    return new CommandBinding(command, Remove_Exec, Remove_CanExec);
                case "OpenDestinationFolderCommand":
                    return new CommandBinding(command, OpenDestinationFolderDialog_Execute);
                case "OpenExternalAppDialogCommand":
                    return new CommandBinding(command, OpenExternalAppDialog_Execute);
                case "PagemarkCommand":
                    return new CommandBinding(command, Pagemark_Execute, Pagemark_CanExecute);
                default:
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

        protected virtual Page GetSelectedPage(object sender)
        {
            return (sender as ListBox)?.SelectedItem as Page;
        }

        protected virtual List<Page> GetSelectedPages(object sender)
        {
            return (sender as ListBox)?.SelectedItems?
                .Cast<Page>()
                .Where(e => e != null)
                .ToList();
        }


        /// <summary>
        /// ページを開く
        /// </summary>
        public void Open_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = page != null;
        }

        public void Open_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var page = (sender as ListBox)?.SelectedItem as Page;
            if (page == null) return;

            Jump(page);
        }

        private void Jump(Page page)
        {
            BookOperation.Current.JumpPage(this, page);
        }


        /// <summary>
        /// ブックを開く
        /// </summary>
        public void OpenBook_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var page = GetSelectedPage(sender);
            e.CanExecute = page != null && page.PageType == PageType.Folder;
        }

        public void OpenBook_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var page = GetSelectedPage(sender);
            if (page == null) return;

            if (page.PageType == PageType.Folder)
            {
                BookHub.Current.RequestLoad(this, page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
            }
        }

        /// <summary>
        /// エクスプローラーで開くコマンド実行
        /// </summary>
        public void OpenExplorer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = GetSelectedPage(sender);
            e.CanExecute = item != null;
        }

        public void OpenExplorer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = GetSelectedPage(sender);
            if (item != null)
            {
                var path = item.SystemPath;
                ExternalProcess.Start("explorer.exe", "/select,\"" + path + "\"");
            }
        }

        /// <summary>
        /// 外部アプリで開く
        /// </summary>
        public void OpenExternalApp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OpenExternalApp_CanExecute(sender);
        }

        public bool OpenExternalApp_CanExecute(object sender)
        {
            var items = GetSelectedPages(sender);
            return items != null && items.Any() && CanCopyToFolder(items);
        }

        public void OpenExternalApp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var externalApp = e.Parameter as ExternalApp;
            if (externalApp == null) return;

            var items = GetSelectedPages(sender);
            if (items != null && items.Any())
            {
                externalApp.Execute(items);
            }
        }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        public void Copy_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);
            e.CanExecute = items != null && items.Any() && CanCopyToFolder(items);
        }

        public void Copy_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);

            var listBox = (ListBox)sender;
            if (items != null && items.Count > 0)
            {
                try
                {
                    App.Current.MainWindow.Cursor = Cursors.Wait;
                    Copy(items);
                }
                finally
                {
                    App.Current.MainWindow.Cursor = null;
                }
            }

            e.Handled = true;
        }

        private void Copy(List<Page> pages)
        {
            ClipboardUtility.Copy(pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All });
        }

        private bool CanCopyToFolder(IEnumerable<Page> pages)
        {
            return PageUtility.CanCreateRealizedFilePathList(pages);
        }

        private void CopyToFolder(IEnumerable<Page> pages, string destDirPath)
        {
            var paths = PageUtility.CreateRealizedFilePathList(pages, CancellationToken.None);
            FileIO.CopyToFolder(paths, destDirPath);
        }


        /// <summary>
        /// フォルダーにコピーコマンド用
        /// </summary>
        public void CopyToFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CopyToFolder_CanExecute(sender);
        }

        public bool CopyToFolder_CanExecute(object sender)
        {
            var items = GetSelectedPages(sender);
            return items != null && items.Any() && CanCopyToFolder(items);
        }

        public void CopyToFolder_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var folder = e.Parameter as DestinationFolder;
            if (folder == null) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = GetSelectedPages(sender);
                if (items != null && items.Any())
                {
                    ////Debug.WriteLine($"CopyToFolder: to {folder.Path}");
                    CopyToFolder(items, folder.Path);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.Bookshelf_CopyToFolderFailed, ToastIcon.Error));
            }
            finally
            {
                e.Handled = true;
            }
        }


        /// <summary>
        /// フォルダーに移動コマンド用
        /// </summary>
        public void MoveToFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MoveToFolder_CanExecute(sender);
        }

        public bool MoveToFolder_CanExecute(object sender)
        {
            var items = GetSelectedPages(sender);
            return Config.Current.System.IsFileWriteAccessEnabled && items != null && items.Any() && CanMoveToFolder(items);
        }

        public void MoveToFolder_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var folder = e.Parameter as DestinationFolder;
            if (folder == null) return;

            try
            {
                if (!Directory.Exists(folder.Path))
                {
                    throw new DirectoryNotFoundException();
                }

                var items = GetSelectedPages(sender);
                if (items != null && items.Any())
                {
                    ////Debug.WriteLine($"MoveToFolder: to {folder.Path}");
                    MoveToFolder(items, folder.Path);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, Properties.Resources.PageList_Message_MoveToFolderFailed, ToastIcon.Error));
            }
            finally
            {
                e.Handled = true;
            }
        }

        private bool CanMoveToFolder(IEnumerable<Page> pages)
        {
            return pages.All(e => e.Entry.IsFileSystem);
        }

        private void MoveToFolder(IEnumerable<Page> pages, string destDirPath)
        {
            var movePages = pages.Where(e => e.Entry.IsFileSystem).ToList();
            var paths = movePages.Select(e => e.GetFilePlace()).ToList();
            FileIO.MoveToFolder(paths, destDirPath);

            // 移動後のブックページ整合性処理
            // TODO: しっかり実装するならページのファイルシステムの監視が必要になる。ファイルの追加削除が自動的にページに反映するように。
            BookOperation.Current.ValidateRemoveFile(movePages);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);
            if (items is null || items.Count <= 0)
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = items.All(x => CanRemove(x));
            }
        }

        public async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);
            if (items is null || items.Count <= 0)
            {
                return;
            }

            await RemoveAsync(items);
            e.Handled = true;
        }

        private bool CanRemove(Page page)
        {
            return BookOperation.Current.CanDeleteFile(page);
        }

        private async Task RemoveAsync(Page page)
        {
            await BookOperation.Current.DeleteFileAsync(page);
        }

        private async Task RemoveAsync(List<Page> pages)
        {
            await BookOperation.Current.DeleteFileAsync(pages);
        }


        public void OpenDestinationFolderDialog_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var listBox = sender as ListBox;
            DestinationFolderDialog.ShowDialog(Window.GetWindow(listBox));
        }

        public void OpenExternalAppDialog_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var listBox = sender as ListBox;
            ExternalAppDialog.ShowDialog(Window.GetWindow(listBox));
        }

        /// <summary>
        /// ページマーク
        /// </summary>
        public void Pagemark_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);
            if (items != null && items.Count > 0)
            {
                e.CanExecute = items.All(x => PagemarkUtility.CanPagemark(x));
            }
            else
            {
                e.CanExecute = false;
            }
        }

        public void Pagemark_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var items = GetSelectedPages(sender);
            if (items != null && items.Count > 0)
            {
                if (items.Any(x => PagemarkUtility.GetPagemark(x) is null))
                {
                    foreach (var item in items)
                    {
                        PagemarkUtility.AddPagemark(item);
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        PagemarkUtility.RemovePagemark(item);
                    }
                }
            }
        }

        public bool Pagemark_IsChecked(object sender)
        {
            var item = GetSelectedPage(sender);
            return PagemarkUtility.GetPagemark(item) != null;
        }
    }

}
