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
    /// PagemarkControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PagemarkControl : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PagemarkControl));

        PagemarkControlVM _VM;
        ThumbnailHelper _ThumbnailHelper;

        public PagemarkControl()
        {
            InitializeComponent();

            _VM = new PagemarkControlVM();
            _VM.SelectedItemChanging += OnItemsChanging;
            _VM.SelectedItemChanged += OnItemsChanged;
            this.DockPanel.DataContext = _VM;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.PagemarkListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));

            _ThumbnailHelper = new ThumbnailHelper(this.PagemarkListBox, _VM.RequestThumbnail);
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
        private void OnItemsChanging(object sender, PagemarkControlVM.SelectedItemChangeEventArgs e)
        {
            var index = this.PagemarkListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.PagemarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            e.IsFocused = lbi != null ? lbi.IsFocused : false;
        }

        //
        private void OnItemsChanged(object sender, PagemarkControlVM.SelectedItemChangeEventArgs e)
        {
            if (e.IsFocused)
            {
                this.PagemarkListBox.ScrollIntoView(this.PagemarkListBox.SelectedItem);

                var index = this.PagemarkListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.PagemarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }
        }




        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // nop.                
        }

        // 履歴項目決定
        private void PagemarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as BookMementoUnitNode;
            if (historyItem != null)
            {
                _VM.Load(historyItem.Value.Memento.Place);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PagemarkListItem_KeyDown(object sender, KeyEventArgs e)
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
        private void PagemarkList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void PagemarkListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e);
        }

        private void PagemarkListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<BookMementoUnitNode>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<BookMementoUnitNode>(sender, e, list);
                e.Handled = true;
            }
        }


        // 表示/非表示イベント
        private async void PagemarkListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (this.PagemarkListBox.SelectedIndex < 0)
                {
                    this.PagemarkListBox.SelectedIndex = 0;
                }

                await Task.Yield();
                FocusSelectedItem();
            }
        }

        //
        public void FocusSelectedItem()
        {
            if (this.PagemarkListBox.SelectedIndex < 0) return;

            this.PagemarkListBox.ScrollIntoView(this.PagemarkListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.PagemarkListBox.ItemContainerGenerator.ContainerFromIndex(this.PagemarkListBox.SelectedIndex));
            lbi?.Focus();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PagemarkControlVM : INotifyPropertyChanged
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

        public PagemarkCollection Pagemark => ModelContext.Pagemarks;
        
        #region Property: SelectedItem
        private BookMementoUnitNode _SelectedItem;
        public BookMementoUnitNode SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged(); }
        }
        #endregion

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
            BookHub?.RequestLoad(path, BookLoadOption.SkipSamePlace, true);
        }

        // となりを取得
        public BookMementoUnitNode GetNeighbor(BookMementoUnitNode item)
        {
            if (Pagemark?.Items == null || Pagemark.Items.Count <= 0) return null;

            int index = Pagemark.Items.IndexOf(item);
            if (index < 0) return Pagemark.Items[0];

            if (index + 1 < Pagemark.Items.Count)
            {
                return Pagemark.Items[index + 1];
            }
            else if (index > 0)
            {
                return Pagemark.Items[index - 1];
            }
            else
            {
                return item;
            }
        }

        public void Remove(BookMementoUnitNode item)
        {
            if (item == null) return;

            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            SelectedItem = GetNeighbor(item);
            SelectedItemChanged?.Invoke(this, args);

            //// TODO:
            ////ModelContext.Pagemarks.Remove(item.Value.Memento.Place);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            PanelContext.ThumbnailManager.RequestThumbnail(Pagemark.Items, start, count, margin, direction);
        }
    }
}
