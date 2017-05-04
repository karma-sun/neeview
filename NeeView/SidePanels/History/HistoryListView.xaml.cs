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
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListView : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(HistoryListView));

        private HistoryListViewModel _vm;

        private ThumbnailHelper _thumbnailHelper;

        //
        public HistoryListView()
        {
            InitializeComponent();
        }

        //
        public HistoryListView(HistoryList model) : this()
        {
            _vm = new HistoryListViewModel(model);
            _vm.SelectedItemChanging += OnItemsChanging;
            _vm.SelectedItemChanged += OnItemsChanged;
            this.DockPanel.DataContext = _vm;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.HistoryListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));

            _thumbnailHelper = new ThumbnailHelper(this.HistoryListBox, _vm.RequestThumbnail);
        }

        //
        private void OnItemsChanging(object sender, HistoryListViewModel.SelectedItemChangeEventArgs e)
        {
            var index = this.HistoryListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            e.IsFocused = lbi != null ? lbi.IsFocused : false;
        }

        //
        private void OnItemsChanged(object sender, HistoryListViewModel.SelectedItemChangeEventArgs e)
        {
            if (e.IsFocused)
            {
                this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);

                var index = this.HistoryListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }

            _thumbnailHelper.UpdateThumbnails(1);
        }

        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as BookMementoUnit;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        // 履歴項目決定
        private void HistoryListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = ((sender as ListBoxItem)?.Content as BookMementoUnit).Memento;

            _vm.Load(historyItem?.Place);
            e.Handled = true;
        }

        // 履歴項目決定(キー)
        private void HistoryListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var historyItem = ((sender as ListBoxItem)?.Content as BookMementoUnit).Memento;

            if (e.Key == Key.Return)
            {
                _vm.Load(historyItem?.Place);
                e.Handled = true;
            }
        }

        // リストのキ入力
        private void HistoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });


            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        // 表示/非表示イベント
        private async void HistoryListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_vm.Visibility == Visibility.Visible)
            {
                _vm.UpdateItems();

                await Task.Yield();
                if (this.HistoryListBox.SelectedIndex < 0) this.HistoryListBox.SelectedIndex = 0;
                FocusSelectedItem();
            }
        }


        // フォーカス
        public void FocusSelectedItem()
        {
            if (this.HistoryListBox.SelectedIndex < 0) return;

            this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(this.HistoryListBox.SelectedIndex));
            lbi?.Focus();
        }

        // 選択項目が表示されるようにスクロールする
        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.HistoryListBox.SelectedItem == null || this.HistoryListBox.SelectedIndex < 0) return;

            // スクロール
            this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);
        }
    }

    // Tooltip表示用コンバータ
    [ValueConversion(typeof(Book.Memento), typeof(string))]
    public class BookMementoToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Book.Memento)
            {
                var bookMemento = (Book.Memento)value;
                return bookMemento.LastAccessTime == default(DateTime) ? bookMemento.Place : bookMemento.Place + "\n" + bookMemento.LastAccessTime;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
