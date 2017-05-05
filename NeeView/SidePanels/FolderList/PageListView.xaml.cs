// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
    public partial class PageListView : UserControl
    {
        public BookHub BookHub
        {
            get { return (BookHub)GetValue(BookHubProperty); }
            set { SetValue(BookHubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookHub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookHubProperty =
            DependencyProperty.Register("BookHub", typeof(BookHub), typeof(PageListView), new PropertyMetadata(null, new PropertyChangedCallback(BookHubPropertyChanged)));

        //
        public static void BookHubPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            PageListView ctrl = d as PageListView;
            if (ctrl != null)
            {
                ctrl._VM.BookHub = ctrl.BookHub;
            }
        }


        // delete command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListView));

        // static constructor
        static PageListView()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }


        /// <summary>
        /// 一度だけフォーカスする
        /// </summary>
        public bool FocusAtOnce { get; set; }

        //
        private PageListViewModel _VM;

        /// <summary>
        /// 応急処置：外部からVMが参照されるのはよろしくない
        /// </summary>
        public PageListViewModel VM => _VM;

        //
        private ThumbnailHelper _thumbnailHelper;

        //
        public PageListView()
        {
            InitializeComponent();
        }

        // constructor
        public PageListView(PageList model) : this()
        {
            _VM = new PageListViewModel(model);
            _VM.PagesChanged += OnPagesChanged;
            this.DockPanel.DataContext = _VM;

            this.PageListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));

            _thumbnailHelper = new ThumbnailHelper(this.PageListBox, _VM.RequestThumbnail);
        }

        //
        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null && _VM.CanRemove(item) && Preference.Current.file_permit_command;
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
        /*
        public void Initialize(MainWindowVM vm)
        {
            _VM.Initialize(vm);
        }
        */


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

        // フォルダーリスト 選択項目変更
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
    public class PageNameConverter : IValueConverter
    {
        public Style SmartTextStyle { get; set; }
        public Style DefaultTextStyle { get; set; }
        public Style NameOnlyTextStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var format = (PageNameFormat)value;
                switch (format)
                {
                    default:
                    case PageNameFormat.None:
                        return DefaultTextStyle;
                    case PageNameFormat.Smart:
                        return SmartTextStyle;
                    case PageNameFormat.NameOnly:
                        return NameOnlyTextStyle;
                }
            }
            catch { }

            return DefaultTextStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
