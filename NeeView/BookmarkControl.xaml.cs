// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// BookmarkControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkControl : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkControl));

        BookmarkControlVM _VM;
        ThumbnailHelper _ThumbnailHelper;

        public BookmarkControl()
        {
            InitializeComponent();

            _VM = new BookmarkControlVM();
            _VM.SelectedItemChanging += OnItemsChanging;
            _VM.SelectedItemChanged += OnItemsChanged;
            this.DockPanel.DataContext = _VM;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.BookmarkListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));

            _ThumbnailHelper = new ThumbnailHelper(this.BookmarkListBox, _VM.RequestThumbnail);
        }

        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as BookMementoUnitNode;
            if (item != null)
            {
                _VM.Remove(item);
            }
        }


        //
        public void Initialize(BookHub bookHub)
        {
            _VM.Initialize(bookHub);
        }


        //
        private void OnItemsChanging(object sender, BookmarkControlVM.SelectedItemChangeEventArgs e)
        {
            var index = this.BookmarkListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            e.IsFocused = lbi != null ? lbi.IsFocused : false;
        }

        //
        private void OnItemsChanged(object sender, BookmarkControlVM.SelectedItemChangeEventArgs e)
        {
            if (e.IsFocused)
            {
                this.BookmarkListBox.ScrollIntoView(this.BookmarkListBox.SelectedItem);

                var index = this.BookmarkListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }
        }




        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // nop.                
        }

        // 履歴項目決定
        private void BookmarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as BookMementoUnitNode;
            if (historyItem != null)
            {
                _VM.Load(historyItem.Value.Memento.Place);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void BookmarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var historyItem = (sender as ListBoxItem)?.Content as BookMementoUnitNode;
            {
                if (e.Key == Key.Return)
                {
                    _VM.Load(historyItem.Value.Memento.Place);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void BookmarkList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void BookmarkListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e);
        }

        private void BookmarkListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<BookMementoUnitNode>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<BookMementoUnitNode>(sender, e, list);
                e.Handled = true;
            }
        }


        // 表示/非表示イベント
        private async void BookmarkListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (this.BookmarkListBox.SelectedIndex < 0)
                {
                    this.BookmarkListBox.SelectedIndex = 0;
                }

                await Task.Yield();
                FocusSelectedItem();
            }
        }

        //
        public void FocusSelectedItem()
        {
            if (this.BookmarkListBox.SelectedIndex < 0) return;

            this.BookmarkListBox.ScrollIntoView(this.BookmarkListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(this.BookmarkListBox.SelectedIndex));
            lbi?.Focus();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BookmarkControlVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion


        // 項目変更イベント。フォーカス保存用
        public class SelectedItemChangeEventArgs
        {
            public bool IsFocused { get; set; }
        }
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanging;
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanged;


        public BookHub BookHub { get; private set; }

        public BookmarkCollection Bookmark => ModelContext.Bookmarks;
        

        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        public double PicturePanelHeight => ThumbnailHeight + 24.0;

        public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);
        public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);


        //
        public void Initialize(BookHub bookHub)
        {
            BookHub = bookHub;

            OnPropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => OnPropertyChanged(nameof(FolderListItemStyle));
        }

        //
        public void Load(string path)
        {
            BookHub?.RequestLoad(path, null, BookLoadOption.SkipSamePlace, true);
        }


        public void Remove(BookMementoUnitNode item)
        {
            if (item == null) return;

            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            Bookmark.SelectedItem = Bookmark.GetNeighbor(item);
            SelectedItemChanged?.Invoke(this, args);

            ModelContext.Bookmarks.Remove(item.Value.Memento.Place);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            PanelContext.ThumbnailManager.RequestThumbnail(Bookmark.Items, start, count, margin, direction);
        }
    }
}
