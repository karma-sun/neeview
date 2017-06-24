// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// FolderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListBox : UserControl
    {
        public static readonly RoutedCommand LoadWithRecursiveCommand = new RoutedCommand("LoadWithRecursiveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(FolderListBox));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(FolderListBox));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(FolderListBox));


        static FolderListBox()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        public FolderListBox()
        {
            InitializeComponent();
        }

        //
        public FolderListBox(FolderListViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = vm;

            this.ListBox.CommandBindings.Add(new CommandBinding(LoadWithRecursiveCommand, LoadWithRecursive_Executed, LoadWithRecursive_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, FileCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, FileCommand_CanExecute));

            this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
            _thumbnailHelper = new ThumbnailHelper(this.ListBox, _vm.Model.RequestThumbnail);
        }


        //
        private FolderListViewModel _vm;


        // TODO: Behaviour化できないかな？
        private ThumbnailHelper _thumbnailHelper;


        // サムネイルの更新要求 (テスト用)
        public void UpdateThumbnail()
        {
            _thumbnailHelper.UpdateThumbnails(1);
        }


        #region RoutedCommand


        /// <summary>
        /// サブフォルダーを読み込む？
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && item.IsDirectory);
        }


        /// <summary>
        /// サブフォルダーを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWithRecursive_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            _vm.Model.LoadBook(item.TargetPath, BookLoadOption.Recursive);
        }

        /// <summary>
        /// ファイル系コマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && !item.IsEmpty && !item.IsDrive && FileIOProfile.Current.IsEnabled);
        }

        /// <summary>
        /// コピーコマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && !item.IsEmpty);
        }

        /// <summary>
        /// コピーコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                FileIO.Current.CopyToClipboard(item);
            }
        }

        /// <summary>
        /// 削除コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                await FileIO.Current.RemoveAsync(item);
            }
        }

        /// <summary>
        /// 名前変更コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var listView = sender as ListBox;

            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null && (item.Attributes & FolderItemAttribute.Empty) != FolderItemAttribute.Empty)
            {
                var listViewItem = VisualTreeTools.GetListBoxItemFromItem(listView, item);
                var textBlock = VisualTreeTools.FindVisualChild<TextBlock>(listViewItem, "FileNameTextBlock");

                // 
                if (textBlock != null)
                {
                    var rename = new RenameControl();
                    rename.Target = textBlock;
                    rename.IsFileName = !item.IsDirectory;
                    rename.Closing += async (s, ev) =>
                    {
                        if (ev.OldValue != ev.NewValue)
                        {
                            var newName = item.IsShortcut ? ev.NewValue + ".lnk" : ev.NewValue;
                            //Debug.WriteLine($"{ev.OldValue} => {newName}");
                            await FileIO.Current.RenameAsync(item, newName);
                        }
                    };
                    rename.Closed += (s, ev) =>
                    {
                        listViewItem.Focus();
                        if (ev.MoveRename != 0)
                        {
                            RenameNext(ev.MoveRename);
                        }
                    };
                    rename.Close += (s, ev) =>
                    {
                        _vm.IsRenaming = false;
                    };

                    ((MainWindow)Application.Current.MainWindow).RenameManager.Open(rename);
                    _vm.IsRenaming = true;
                }
            }
        }


        /// <summary>
        /// 項目を移動して名前変更処理を続行する
        /// </summary>
        /// <param name="delta"></param>
        private void RenameNext(int delta)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目を1つ移動
            this.ListBox.SelectedIndex = (this.ListBox.SelectedIndex + this.ListBox.Items.Count + delta) % this.ListBox.Items.Count;
            this.ListBox.UpdateLayout();

            // ブック切り替え
            var item = this.ListBox.SelectedItem as FolderItem;
            if (item != null)
            {
                _vm.Model.LoadBook(item.TargetPath);
            }

            // リネーム発動
            Rename_Executed(this.ListBox, null);
        }

        /// <summary>
        /// エクスプローラーで開くコマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenExplorer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + item.Path + "\"");
            }
        }


        #endregion


        /// <summary>
        /// スクロール変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // リネームキャンセル
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();
        }


        #region list event process

        // 最後のフォーカスフラグ
        // ロード前のフォーカス設定を反映させるため
        private bool _lastFocusRequest;

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            _lastFocusRequest = isFocus;

            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (isFocus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                if (lbi == null) return;

                var isFocused = lbi.Focus();

                // フォーカスできない場合にはディスパッチャーで再実行
                if (!isFocused)
                {
                    this.Dispatcher.BeginInvoke((Action)(() => lbi.Focus()));
                }
            }
        }

        //
        private bool _storeFocus;

        /// <summary>
        /// 選択項目フォーカス状態を取得
        /// リスト項目変更前処理。
        /// </summary>
        /// <returns></returns>
        public void StoreFocus()
        {
            var index = this.ListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            _storeFocus = lbi != null ? lbi.IsFocused : false;
        }

        /// <summary>
        /// 選択項目フォーカス反映
        /// リスト変更後処理。
        /// </summary>
        /// <param name="isFocused"></param>
        public void RestoreFocus()
        {
            if (_storeFocus)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

                var index = this.ListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                var isSuccess = lbi?.Focus();
            }

            _thumbnailHelper.UpdateThumbnails(1);
        }



        //
        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem(_lastFocusRequest);
        }

        private void FolderList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Home)
            {
                _vm.MoveToHome.Execute(null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    _vm.MoveToUp.Execute(null);
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    _vm.MoveToPrevious.Execute(null);
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    _vm.MoveToNext.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void FolderList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Back) // Backspace
            {
                _vm.MoveToUp.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                // 選択項目が表示されるようにスクロール
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        //
        private void FolderListItem_MouseSingleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo != null && !folderInfo.IsEmpty)
            {
                _vm.Model.LoadBook(folderInfo.TargetPath);
                e.Handled = true;
            }
        }

        //
        private void FolderListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
            {
                _vm.MoveTo.Execute(folderInfo.TargetPath);
            }
            e.Handled = true;
        }

        //
        private void FolderListItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Model.LoadBook(folderInfo.TargetPath);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right) // →
                {
                    if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
                    {
                        _vm.MoveTo.Execute(folderInfo.TargetPath);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Left || e.Key == Key.Back) // ← Backspace
                {
                    if (folderInfo != null)
                    {
                        _vm.MoveToUp.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }


        //
        private void FolderListItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo == null) return;

            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseDown(sender, e, folderInfo);
        }

        //
        private void FolderListItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseUp(sender, e);
        }

        //
        private void FolderListItem_MouseMove(object sender, MouseEventArgs e)
        {
            // 一時的にドラッグ禁止
            ////_vm.Drag_MouseMove(sender, e);
        }

        #endregion
    }
}
