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

        public BookmarkControl()
        {
            InitializeComponent();

            _VM = new BookmarkControlVM();
            this.DockPanel.DataContext = _VM;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.BookmarkListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Book.Memento;
            if (item != null)
            {
                ModelContext.Bookmarks.Remove(item.Place);
            }
        }


        //
        public void Initialize(BookHub bookHub)
        {
            _VM.Initialize(bookHub);
        }

        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            _VM.Update();
            this.BookmarkListBox.Items.Refresh();

            // フォーカス
            this.BookmarkListBox.SelectedIndex = 0;
            this.BookmarkListBox.ScrollIntoView(this.BookmarkListBox.SelectedItem);
            this.BookmarkListBox.UpdateLayout();
            ListBoxItem lbi = (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(this.BookmarkListBox.SelectedIndex));
            lbi?.Focus();
            */
        }

        // 履歴項目決定
        private void BookmarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as Book.Memento;
            if (historyItem != null)
            {
                _VM.Load(historyItem.Place);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void BookmarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as Book.Memento;
            {
                if (e.Key == Key.Return)
                {
                    _VM.Load(historyItem.Place);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void BookmarkList_KeyDown(object sender, KeyEventArgs e)
        {
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
            var list = (sender as ListBox).Tag as ObservableCollection<Book.Memento>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<Book.Memento>(sender, e, list);
                e.Handled = true;
            }
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

        public BookHub BookHub { get; private set; }


        public BookmarkCollection Bookmark => ModelContext.Bookmarks;

        //
        public void Initialize(BookHub bookHub)
        {
            BookHub = bookHub;
        }

        //
        public void Load(string path)
        {
            BookHub?.RequestLoad(path, BookLoadOption.None, false);
        }
    }
}
