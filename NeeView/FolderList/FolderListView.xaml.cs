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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;


// TODO: サムネイル処理をクラス化して共有する


namespace NeeView
{
    /// <summary>
    /// FolderListView.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListView : UserControl
    {
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(BookmarkControl));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        static FolderListView()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        // TODO: この内のいくつかはFolderListControlで処理すべき
        public event EventHandler<string> Decided;
        public event EventHandler<string> Moved;
        public event EventHandler<string> MovedParent;
        public event EventHandler MovedHome;
        public event EventHandler MovedPrevious;
        public event EventHandler MovedNext;


        private ThumbnailHelper _thumbnailHelper;


        /// <summary>
        /// VM property.
        /// </summary>
        private FolderListViewModel _VM;
        public FolderListViewModel VM
        {
            get { return _VM; }
            set { if (_VM != value) { _VM = value; } }
        }

        /// <summary>
        /// 初期化時にフォーカス取得
        /// </summary>
        private bool _autoFocus;


        /// <summary>
        /// constructore
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="autoFocus"></param>
        public FolderListView(FolderListViewModel vm, bool autoFocus)
        {
            _autoFocus = autoFocus;

            InitializeComponent();

            _VM = vm;
            this.ListBox.DataContext = _VM;

            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Executed));
            this.ListBox.CommandBindings.Add(new CommandBinding(CopyCommand, Copy_Executed, Copy_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Executed, FileCommand_CanExecute));
            this.ListBox.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Executed, FileCommand_CanExecute));

            this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));

            _thumbnailHelper = new ThumbnailHelper(this.ListBox, _VM.RequestThumbnail);
        }


        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose()
        {
            _VM?.Dispose();
        }

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

        /// <summary>
        /// ファイル系コマンド実行可能判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            e.CanExecute = (item != null && !item.IsEmpty && !item.IsDrive);
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
                _VM.Copy(item);
            }
        }

        /// <summary>
        /// 削除コマンド実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderItem;
            if (item != null)
            {
                _VM.Remove(item);
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
                    rename.Closing += (s, ev) =>
                    {
                        if (ev.OldValue != ev.NewValue)
                        {
                            var newName = item.IsShortcut ? ev.NewValue + ".lnk" : ev.NewValue;
                            //Debug.WriteLine($"{ev.OldValue} => {newName}");
                            _VM.Rename(item, newName);
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

                    ((MainWindow)Application.Current.MainWindow).RenameManager.Open(rename);
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
                Decided?.Invoke(this, item.TargetPath);
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

        /// <summary>
        /// フォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目が表示されるようにスクロール
            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (isFocus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        /// <summary>
        /// SelectionChanged
        /// フォルダリスト 選択項目変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                // 選択項目が表示されるようにスクロール
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        /// <summary>
        /// SlingleClick
        /// フォルダ項目決定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo != null && !folderInfo.IsEmpty)
            {
                Decided?.Invoke(this, folderInfo.TargetPath);
                e.Handled = true;
            }
        }

        /// <summary>
        /// DoubleClick
        /// フォルダ移動決定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
            {
                Moved?.Invoke(this, folderInfo.TargetPath);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 項目でのキー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var folderInfo = (sender as ListBoxItem)?.Content as FolderItem;
            {
                if (e.Key == Key.Return)
                {
                    Decided?.Invoke(this, folderInfo.TargetPath);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right) // →
                {
                    if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
                    {
                        Moved?.Invoke(this, folderInfo.TargetPath);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Left || e.Key == Key.Back) // ← Backspace
                {
                    if (folderInfo != null)
                    {
                        MovedParent?.Invoke(this, folderInfo.ParentPath);
                    }
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// キー入力処理(Preview)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            if (e.Key == Key.Home)
            {
                MovedHome?.Invoke(this, null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Up)
                {
                    MovedParent?.Invoke(this, null);
                    e.Handled = true;
                }
                else if (key == Key.Left)
                {
                    MovedPrevious?.Invoke(this, null);
                    e.Handled = true;
                }
                else if (key == Key.Right)
                {
                    MovedNext?.Invoke(this, null);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// キー入力処理(Preview)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Back) // Backspace
            {
                MovedParent?.Invoke(this, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Loaded
        /// 開始直後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem(_autoFocus);
        }
    }


    /// <summary>
    /// IconOverlay StyleConverter
    /// </summary>
    public class FolderIconOverlayConverter : IValueConverter
    {
        public Style FolderOverlayDisable { get; set; }

        public Style FolderOverlayChecked { get; set; }

        public Style FolderOverlayStar { get; set; }

        //
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var overlay = (FolderItemIconOverlay)value;
                switch (overlay)
                {
                    case FolderItemIconOverlay.Disable:
                        return FolderOverlayDisable;
                    case FolderItemIconOverlay.Star:
                        return FolderOverlayStar;
                    case FolderItemIconOverlay.Checked:
                        return FolderOverlayChecked;
                    default:
                        return null;
                }
            }
            catch
            {
            }

            return null;
        }

        //
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
