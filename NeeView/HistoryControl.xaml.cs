// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

// TODO: パフォーマンス改善。リストの更新を表示されている時に限定。それ以外は更新フラグのみにする

namespace NeeView
{
    /// <summary>
    /// HistoryControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(HistoryControl));

        HistoryControlVM _VM;


        public HistoryControl()
        {
            InitializeComponent();

            _VM = new HistoryControlVM();
            this.DockPanel.DataContext = _VM;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.HistoryListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as BookMementoUnit;
            if (item != null)
            {
                ModelContext.BookHistory.Remove(item.Memento.Place);
            }
        }

        //
        public void Initialize(BookHub bookHub)
        {
            _VM.Initialize(bookHub, this.HistoryListBox.IsVisible);
        }

        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.UpdateItems();
            this.HistoryListBox.Items.Refresh();

            // フォーカス
            this.HistoryListBox.SelectedIndex = 0;
            this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);
            this.HistoryListBox.UpdateLayout();
            ListBoxItem lbi = (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(this.HistoryListBox.SelectedIndex));
            lbi?.Focus();
        }

        // 履歴項目決定
        private void HistoryListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = ((sender as ListBoxItem)?.Content as BookMementoUnit).Memento;

            if (historyItem != null)
            {
                _VM.Load(historyItem.Place);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void HistoryListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var historyItem = ((sender as ListBoxItem)?.Content as BookMementoUnit).Memento;
            {
                if (e.Key == Key.Return)
                {
                    _VM.Load(historyItem.Place);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void HistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        // 表示/非表示イベント
        private async void HistoryListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                _VM.UpdateItems();

                await Task.Yield();
                FocusSelectedItem();
            }
        }


        //
        public void FocusSelectedItem()
        {
            if (this.HistoryListBox.SelectedIndex < 0) return;

            this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(this.HistoryListBox.SelectedIndex));
            lbi?.Focus();
        }

        //
        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.HistoryListBox.SelectedItem == null || this.HistoryListBox.SelectedIndex < 0) return;

            // スクロール
            this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HistoryControlVM : INotifyPropertyChanged
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

        public List<BookMementoUnit> Items { get; set; }

        #region Property: SelectedItem
        private BookMementoUnit _SelectedItem;
        public BookMementoUnit SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: Visibility
        private Visibility _Visibility;
        public Visibility Visibility
        {
            get { return _Visibility; }
            set { _Visibility = value; OnPropertyChanged(); }
        }
        #endregion



        private bool _IsDarty;

        //
        public void Initialize(BookHub bookHub, bool isVisible)
        {
            BookHub = bookHub;

            _IsDarty = true;
            if (isVisible) UpdateItems();

            BookHub.HistoryChanged += BookHub_HistoryChanged;
            BookHub.HistoryListSync += BookHub_HistoryListSync;
        }

        //
        private void BookHub_HistoryListSync(object sender, string e)
        {
            SelectedItem = ModelContext.BookHistory.Find(e);
        }

        //
        private void BookHub_HistoryChanged(object sender, BookMementoCollectionChangedArgs e)
        {
            _IsDarty = _IsDarty || e.HistoryChangedType != BookMementoCollectionChangedType.Update;
            if (_IsDarty && Visibility == Visibility.Visible)
            {
                UpdateItems();
            }
        }

        //
        public void UpdateItems()
        {
            if (_IsDarty)
            {
                _IsDarty = false;
                Items = ModelContext.BookHistory.Items.ToList();
                OnPropertyChanged(nameof(Items));
                SelectedItem = Items.Count > 0 ? Items[0] : null;
            }
        }

        //
        public void Load(string path)
        {
            BookHub?.RequestLoad(path, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace, true);
        }
    }
}
