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


namespace NeeView
{
    /// <summary>
    /// HistoryControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(HistoryControl));

        HistoryControlVM _VM;
        ThumbnailHelper _ThumbnailHelper;


        public HistoryControl()
        {
            InitializeComponent();

            _VM = new HistoryControlVM();
            _VM.SelectedItemChanging += OnItemsChanging;
            _VM.SelectedItemChanged += OnItemsChanged;
            this.DockPanel.DataContext = _VM;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.HistoryListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));

            _ThumbnailHelper = new ThumbnailHelper(this.HistoryListBox, _VM.RequestThumbnail);
        }

        //
        private void OnItemsChanging(object sender, HistoryControlVM.SelectedItemChangeEventArgs e)
        {
            var index = this.HistoryListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            e.IsFocused = lbi != null ? lbi.IsFocused : false;
        }

        //
        private void OnItemsChanged(object sender, HistoryControlVM.SelectedItemChangeEventArgs e)
        {
            if (e.IsFocused)
            {
                this.HistoryListBox.ScrollIntoView(this.HistoryListBox.SelectedItem);

                var index = this.HistoryListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.HistoryListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }

            _ThumbnailHelper.UpdateThumbnails(1);
        }

        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as BookMementoUnit;
            if (item != null)
            {
                _VM.Remove(item);
            }
        }

        //
        public void Initialize(BookHub bookHub)
        {
            _VM.Initialize(bookHub, this.HistoryListBox.IsVisible);
        }

        // 履歴項目決定
        private void HistoryListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = ((sender as ListBoxItem)?.Content as BookMementoUnit).Memento;

            _VM.Load(historyItem?.Place);
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
                _VM.Load(historyItem?.Place);
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
            _VM.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;

            if (_VM.Visibility == Visibility.Visible)
            {
                _VM.UpdateItems();

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


        // 項目変更イベント。フォーカス保存用
        public class SelectedItemChangeEventArgs
        {
            public bool IsFocused { get; set; }
        }
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanging;
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanged;


        public BookHub BookHub { get; private set; }


        #region Property: Items
        private ObservableCollection<BookMementoUnit> _Items;
        public ObservableCollection<BookMementoUnit> Items
        {
            get { return _Items; }
            set { _Items = value; OnPropertyChanged(); }
        }
        #endregion


        #region Property: SelectedItem
        private BookMementoUnit _SelectedItem;
        public BookMementoUnit SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged(); }
        }
        #endregion


        #region Property: Visibility
        private Visibility _Visibility = Visibility.Hidden;
        public Visibility Visibility
        {
            get { return _Visibility; }
            set { _Visibility = value; OnPropertyChanged(); }
        }
        #endregion

        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        public double PicturePanelHeight => ThumbnailHeight + 24.0;

        public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);
        public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);


        private bool _IsDarty;

        //
        public void Initialize(BookHub bookHub, bool isVisible)
        {
            BookHub = bookHub;

            _IsDarty = true;
            if (isVisible) UpdateItems();

            BookHub.HistoryChanged += BookHub_HistoryChanged;
            BookHub.HistoryListSync += BookHub_HistoryListSync;

            OnPropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => OnPropertyChanged(nameof(FolderListItemStyle));
        }

        //
        private void BookHub_HistoryListSync(object sender, string e)
        {
            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            SelectedItem = ModelContext.BookHistory.Find(e);
            SelectedItemChanged?.Invoke(this, args);
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

                var args = new SelectedItemChangeEventArgs();
                App.Current.Dispatcher.Invoke(() => SelectedItemChanging?.Invoke(this, args));

                var item = SelectedItem;
                Items = new ObservableCollection<BookMementoUnit>(ModelContext.BookHistory.Items);
                SelectedItem = Items.Count > 0 ? item : null;

                App.Current.Dispatcher.Invoke(() => SelectedItemChanged?.Invoke(this, args));
            }
        }

        //
        public void Load(string path)
        {
            if (path == null) return;
            BookHub?.RequestLoad(path, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace, true);
        }


        // となりを取得
        public BookMementoUnit GetNeighbor(BookMementoUnit item)
        {
            if (Items == null || Items.Count <= 0) return null;

            int index = Items.IndexOf(item);
            if (index < 0) return Items[0];

            if (index + 1 < Items.Count)
            {
                return Items[index + 1];
            }
            else if (index > 0)
            {
                return Items[index - 1];
            }
            else
            {
                return item;
            }
        }

        //
        public void Remove(BookMementoUnit item)
        {
            if (item == null) return;

            // 位置ずらし
            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            SelectedItem = GetNeighbor(item);
            SelectedItemChanged?.Invoke(this, args);

            // 削除
            ModelContext.BookHistory.Remove(item.Memento.Place);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            PanelContext.ThumbnailManager.RequestThumbnail(Items, start, count, margin, direction);
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
