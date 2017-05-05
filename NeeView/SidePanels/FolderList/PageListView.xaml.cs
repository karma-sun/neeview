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
        // delete command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListView));

        // static constructor
        static PageListView()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        //
        private PageListViewModel _vm;

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
            _vm = new PageListViewModel(model);
            _vm.PagesChanged += OnPagesChanged;
            this.DockPanel.DataContext = _vm;

            this.PageListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));

            _thumbnailHelper = new ThumbnailHelper(this.PageListBox, _vm.RequestThumbnail);
        }

        //
        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null && _vm.CanRemove(item) && Preference.Current.file_permit_command;
        }

        //
        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            if (item != null)
            {
                await _vm.Remove(item);
            }
        }


        //
        private void OnPagesChanged(object sender, EventArgs e)
        {
            this.PageListBox.ScrollIntoView(this.PageListBox.SelectedItem);
        }


        //
        public void FocusSelectedItem()
        {
            if (this.PageListBox.SelectedIndex < 0) return;

            this.PageListBox.ScrollIntoView(this.PageListBox.SelectedItem);

            if (_vm.Model.FocusAtOnce)
            {
                _vm.Model.FocusAtOnce = false;
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
                _vm.Jump(page);
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
                    _vm.Jump(page);
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
