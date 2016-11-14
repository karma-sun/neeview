// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// PageListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListControl : UserControl
    {
        public BookHub BookHub
        {
            get { return (BookHub)GetValue(BookHubProperty); }
            set { SetValue(BookHubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookHub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookHubProperty =
            DependencyProperty.Register("BookHub", typeof(BookHub), typeof(PageListControl), new PropertyMetadata(null, new PropertyChangedCallback(BookHubPropertyChanged)));

        //
        public static void BookHubPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            PageListControl ctrl = d as PageListControl;
            if (ctrl != null)
            {
                ctrl._VM.BookHub = ctrl.BookHub;
            }
        }


        // delete command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListControl));

        // static constructor
        static PageListControl()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }


        /// <summary>
        /// 一度だけフォーカスする
        /// </summary>
        public bool FocusAtOnce { get; set; }

        //
        private PageListControlVM _VM;

        // constructor
        public PageListControl()
        {
            InitializeComponent();

            _VM = new PageListControlVM();
            _VM.PagesChanged += OnPagesChanged;
            this.DockPanel.DataContext = _VM;

            this.PageListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));
        }

        //
        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null ? _VM.CanRemove(item) : false;
        }

        //
        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            if (item != null)
            {
                await _VM.Remove(item);
            }
        }


        //
        private void OnPagesChanged(object sender, EventArgs e)
        {
            this.PageListBox.ScrollIntoView(this.PageListBox.SelectedItem);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _VM.Initialize(vm);
        }


        //
        public void FocusSelectedItem()
        {
            if (this.PageListBox.SelectedIndex < 0) return;

            this.PageListBox.ScrollIntoView(this.PageListBox.SelectedItem);

            if (FocusAtOnce)
            {
                FocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.PageListBox.ItemContainerGenerator.ContainerFromIndex(this.PageListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        // フォルダリスト 選択項目変更
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }


        // 履歴項目決定
        private void PageListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _VM.Jump(page);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PageListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var page = (sender as ListBoxItem)?.Content as Page;
            {
                if (e.Key == Key.Return)
                {
                    _VM.Jump(page);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        //
        private async void PaegList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }
    }

    public enum PageNameFormat
    {
        None,
        Smart,
        NameOnly,
    }

    /// <summary>
    /// 
    /// </summary>
    public class PageListControlVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public event EventHandler PagesChanged;


        #region Property: VM
        private MainWindowVM _VM;
        public MainWindowVM VM
        {
            get { return _VM; }
            set
            {
                _VM = value;
                _VM.PageListChanged += (s, e) => Reflesh();
                RaisePropertyChanged();
            }
        }
        #endregion



        private BookHub _bookHub;
        public BookHub BookHub
        {
            get { return _bookHub; }
            set
            {
                _bookHub = value;
                _bookHub.ViewContentsChanged += BookHub_ViewContentsChanged;
                RaisePropertyChanged();
            }
        }

        private void BookHub_ViewContentsChanged(object sender, ViewSource e)
        {
            var contents = e?.Sources;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().Position < contents.Last().Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }
        }

        public Dictionary<PageNameFormat, string> FormatList { get; } = new Dictionary<PageNameFormat, string>
        {
            [PageNameFormat.None] = "そのまま",
            [PageNameFormat.Smart] = "標準表示",
            [PageNameFormat.NameOnly] = "名前のみ",
        };

        #region Property: Format
        private PageNameFormat _format = PageNameFormat.Smart;
        public PageNameFormat Format
        {
            get { return _format; }
            set { _format = value; RaisePropertyChanged(); }
        }
        #endregion



        public Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;

        #region Property: Title
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: PageSortMode
        private PageSortMode _pageSortMode;
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; _bookHub.SetSortMode(value); }
        }
        #endregion

        #region Property: SelectedItem
        private Page _selectedItem;
        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }
        #endregion

        //
        private void Reflesh()
        {
            Title = System.IO.Path.GetFileName(_bookHub.CurrentBook?.Place);

            _pageSortMode = _bookHub.BookMemento.SortMode;
            RaisePropertyChanged(nameof(PageSortMode));

            App.Current.Dispatcher.Invoke(() => PagesChanged?.Invoke(this, null));
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            VM = vm;
            BookHub = vm.BookHub;
            Reflesh();
        }

        //
        public void Jump(Page page)
        {
            _bookHub.JumpPage(page);
        }


        //
        public bool CanRemove(Page page)
        {
            return _bookHub.CanRemoveFile(page);
        }

        //
        public async Task Remove(Page page)
        {
            await _bookHub.RemoveFile(page);
        }
    }
}
