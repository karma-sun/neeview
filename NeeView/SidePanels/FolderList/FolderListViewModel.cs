// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
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

        /// <summary>
        /// フォルダーコレクション
        /// </summary>
        public FolderCollection FolderCollection { get; private set; }

        /// <summary>
        /// フォルダーの場所
        /// </summary>
        public string Place => FolderCollection?.Place;

        /// <summary>
        /// フォルダーの場所 表示用
        /// </summary>
        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "このPC" : Place;

        /// <summary>
        /// フォルダー項目表示スタイル
        /// </summary>
        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        /// <summary>
        /// バナーの高さ
        /// </summary>
        //public double PicturePanelHeight => ThumbnailHeight + 24.0;

        /// <summary>
        /// バナー画像幅
        /// </summary>
        //public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);

        /// <summary>
        /// バナー画像高さ
        /// </summary>
        //public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);


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

        /// <summary>
        /// IsRenaming property.
        /// </summary>
        private bool _isRenaming;
        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { if (_isRenaming != value) { _isRenaming = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="collection"></param>
        public FolderListViewModel(FolderCollection collection)
        {
            this.FolderCollection = collection;
            this.FolderCollection.Deleting += FolderCollection_Deleting;

            RaisePropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => RaisePropertyChanged(nameof(FolderListItemStyle));
        }


        /// <summary>
        /// 終了処理
        /// </summary>
        internal void Dispose()
        {
            this.FolderCollection?.Dispose();
        }


        /// <summary>
        /// フォルダーリスト項目変更前処理
        /// 項目が削除される前に有効な選択項目に変更する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderCollection_Deleting(object sender, System.IO.FileSystemEventArgs e)
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


        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        /// <param name="info"></param>
        public void Copy(FolderItem info)
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
        public void Remove(FolderItem info)
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


        /// <summary>
        /// ファイル名前変更
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool Rename(FolderItem file, string newName)
        {
            string src = file.Path;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            if (src == dst) return true;

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                // 確認
                MessageBox.Show($"ファイル名に使用できない文字が含まれています。( {newName[invalidCharsIndex]} )", "名前の変更の確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

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
        /// エクスプローラーで開く
        /// </summary>
        private RelayCommand<object> _openPlaceCommand;
        public RelayCommand<object> OpenPlaceCommand
        {
            get { return _openPlaceCommand = _openPlaceCommand ?? new RelayCommand<object>(OpenPlaceCommand_Executed); }
        }

        private void OpenPlaceCommand_Executed(object parameter)
        {
            var item = parameter as FolderItem;
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

        /// <summary>
        /// 選択項目を基準とした項目取得
        /// </summary>
        /// <param name="offset">選択項目から前後した項目を指定</param>
        /// <returns></returns>
        internal FolderItem GetFolderItem(int offset)
        {
            if (this.FolderCollection?.Items == null) return null;

            int index = this.SelectedIndex;
            if (index < 0) return null;

            int next = (this.FolderCollection.FolderCollectionParameter.FolderOrder == FolderOrder.Random)
                ? (index + this.FolderCollection.Items.Count + offset) % this.FolderCollection.Items.Count
                : index + offset;

            if (next < 0 || next >= this.FolderCollection.Items.Count) return null;

            return this.FolderCollection[next];
        }

        /// <summary>
        /// フォルダー並び順を切り替え
        /// </summary>
        internal void ToggleFolderOrder()
        {
            if (this.FolderCollection?.Items == null) return;

            this.FolderCollection.FolderCollectionParameter.FolderOrder = this.FolderCollection.FolderCollectionParameter.FolderOrder.GetToggle();
        }

        /// <summary>
        /// フォルダー並び順を取得
        /// </summary>
        /// <returns></returns>
        internal FolderOrder GetFolderOrder()
        {
            return this.FolderCollection.FolderCollectionParameter.FolderOrder;
        }

        /// <summary>
        /// フォルダー並び順を設定
        /// </summary>
        /// <param name="folderOrder"></param>
        internal void SetFolderOrder(FolderOrder folderOrder)
        {
            this.FolderCollection.FolderCollectionParameter.FolderOrder = folderOrder;
        }

        /// <summary>
        /// アイコン更新
        /// </summary>
        /// <param name="path"></param>
        internal void RefleshIcon(string path)
        {
            this.FolderCollection.RefleshIcon(path);
        }

        /// <summary>
        /// 更新の必要性判定
        /// </summary>
        /// <returns></returns>
        internal bool IsDarty()
        {
            return this.FolderCollection.IsDarty();
        }

        /// <summary>
        /// パスがリストに含まれているか判定
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal bool Contains(string path)
        {
            return this.FolderCollection.Contains(path);
        }

        /// <summary>
        /// ふさわしい選択項目インデックスを取得
        /// </summary>
        /// <param name="path">選択したいパス</param>
        /// <returns></returns>
        internal int FixedIndexOfPath(string path)
        {
            var index = this.FolderCollection.IndexOfPath(path);
            return index < 0 ? 0 : index;
        }
    }
}
