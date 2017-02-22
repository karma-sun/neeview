// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// FolderList ViewModel
    /// </summary>
    public class FolderListViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion


        public FolderCollection FolderCollection { get; private set; }

        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        public double PicturePanelHeight => ThumbnailHeight + 24.0;

        public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);
        public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);



        /// <summary>
        /// SelectIndex property.
        /// </summary>
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = NVUtility.Clamp(value, 0, this.FolderCollection.Items.Count - 1);
                RaisePropertyChanged();
            }
        }


        //
        public FolderListViewModel(FolderCollection collection)
        {
            this.FolderCollection = collection;
            this.FolderCollection.Changing += FolderCollection_Changing;

            RaisePropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => RaisePropertyChanged(nameof(FolderListItemStyle));
        }


        //
        private void FolderCollection_Changing(object sender, System.IO.FileSystemEventArgs e)
        {
            if (e.ChangeType != System.IO.WatcherChangeTypes.Deleted) return;

            var index = this.FolderCollection.IndexOfPath(e.FullPath);
            if (SelectedIndex != index) return;

            if (SelectedIndex < this.FolderCollection.Items.Count - 1)
            {
                SelectedIndex++;
            }
            else if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
        }


        public void Copy(FolderInfo info)
        {
            if (info.IsEmpty) return;

            var files = new List<string>();
            files.Add(info.Path);
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, files.ToArray());
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", files));
            Clipboard.SetDataObject(data);
        }

        //
        /// <summary>
        /// RemoveCommand command.
        /// </summary>
        private RelayCommand<object> _removeCommand;
        public RelayCommand<object> RemoveCommand
        {
            get { return _removeCommand = _removeCommand ?? new RelayCommand<object>(RemoveCommand_Executed); }
        }

        private void RemoveCommand_Executed(object parameter)
        {
            var item = parameter as FolderInfo;
            if (item != null)
            {
                Remove(item);
            }
        }


        public void Remove(FolderInfo info)
        {
            if (info.IsEmpty) return;

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            var thumbnail = new Image();
            thumbnail.SnapsToDevicePixels = true;
            thumbnail.Source = info.Icon;
            thumbnail.Width = 32;
            thumbnail.Height = 32;
            thumbnail.Margin = new System.Windows.Thickness(0, 0, 4, 0);
            stackPanel.Children.Add(thumbnail);
            var textblock = new TextBlock();
            textblock.Text = info.Path;
            textblock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(textblock);
            stackPanel.Margin = new Thickness(0, 0, 0, 20);

            Messenger.Send(this, new MessageEventArgs("RemoveFile") { Parameter = new RemoveFileParams() { Path = info.Path, Visual = stackPanel } });
        }

        //
        public void Rename(FolderInfo info)
        {
            if (info.IsEmpty) return;

            throw new NotImplementedException();
        }

        //
        public bool Rename(FolderInfo file, string newName)
        {
            string src = file.Path;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            if (src == dst) return true;

            // 拡張子変更確認
            if (!file.IsDirectory)
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    var resut = MessageBox.Show($"拡張子を変更すると、使えなくなる可能性があります。\n\n変更しますか？", "名前の変更の確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (resut != MessageBoxResult.OK)
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
                var resut = MessageBox.Show($"{System.IO.Path.GetFileName(dstBase)} は既に存在します。\n{System.IO.Path.GetFileName(dst)} に名前を変更しますか？", "名前の変更の確認", MessageBoxButton.OKCancel);
                if (resut != MessageBoxResult.OK)
                {
                    return false;
                }
            }

            // 名前変更実行
            var result = Messenger.Send(this, new MessageEventArgs("RenameFile") { Parameter = new RenameFileParams() { OldPath = src, Path = dst } });
            return result == true ? true : false;
        }




        /// <summary>
        /// OpenPlaceCommand command.
        /// </summary>
        private RelayCommand<object> _openPlaceCommand;
        public RelayCommand<object> OpenPlaceCommand
        {
            get { return _openPlaceCommand = _openPlaceCommand ?? new RelayCommand<object>(OpenPlaceCommand_Executed); }
        }

        private void OpenPlaceCommand_Executed(object parameter)
        {
            var item = parameter as FolderInfo;
            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + item.Path + "\"");
            }
        }






        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            //Debug.WriteLine($"{start}({count})");
            PanelContext.ThumbnailManager.RequestThumbnail(FolderCollection.Items, start, count, margin, direction);
        }
    }
}
