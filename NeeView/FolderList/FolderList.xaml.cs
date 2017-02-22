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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


// TODO: サムネイル処理をクラス化して共有する


namespace NeeView
{
    /// <summary>
    /// FolderList.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderList : UserControl
    {
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(BookmarkControl));

        static FolderList()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
        }

        public event EventHandler<string> Decided;
        public event EventHandler<string> Moved;
        public event EventHandler<string> MovedParent;
        public event EventHandler<int> SelectionChanged;
        public event EventHandler MovedHome;
        public event EventHandler MovedPrevious;
        public event EventHandler MovedNext;


        private ThumbnailHelper _thumbnailHelper;


        private FolderListViewModel _VM;
        private bool _autoFocus;

        public FolderInfo SelectedItem => this.ListBox.SelectedItem as FolderInfo;

        //
        public FolderList(FolderListViewModel vm, bool autoFocus)
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

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).RenameManager.Stop();
        }

        private void FileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            e.CanExecute = (item != null && !item.IsEmpty && !item.IsDrive);
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            e.CanExecute = (item != null && !item.IsEmpty);
        }

        //
        public void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null)
            {
                _VM.Copy(item);
            }
        }

        //
        public void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null)
            {
                _VM.Remove(item);
            }
        }



        //
        public void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var listView = sender as ListBox;

            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null && (item.Attributes & FolderInfoAttribute.Empty) != FolderInfoAttribute.Empty)
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

        //
        private void RenameNext(int delta)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            // 選択項目を1つ移動
            this.ListBox.SelectedIndex = (this.ListBox.SelectedIndex + this.ListBox.Items.Count + delta) % this.ListBox.Items.Count;
            this.ListBox.UpdateLayout();

            // リネーム発動
            Rename_Executed(this.ListBox, null);
        }

        //
        public void OpenExplorer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + item.Path + "\"");
            }
        }

        //
        public void SetSelectedIndex(int index)
        {
            _VM.SelectedIndex = index;
        }

        //
        public void FocusSelectedItem(bool isFocus)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (isFocus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }


        // フォルダリスト 選択項目変更
        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
                SelectionChanged?.Invoke(this, listBox.SelectedIndex);
            }
        }


        // フォルダ項目決定
        private void FolderListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
            if (folderInfo != null && !folderInfo.IsEmpty)
            {
                Decided?.Invoke(this, folderInfo.TargetPath);
                e.Handled = true;
            }
        }

        // フォルダ移動決定
        private void FolderListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
            if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
            {
                Moved?.Invoke(this, folderInfo.TargetPath);
            }
            e.Handled = true;
        }

        // フォルダ移動決定(キー)
        private void FolderListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
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

        //
        private void FolderList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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

        // フォルダ移動決定(キー)
        private void FolderList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

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

        // ロードイベント
        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem(_autoFocus);
        }
    }

}
