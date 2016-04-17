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

namespace NeeView
{
    /// <summary>
    /// HistoryControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        HistoryControlVM _VM;

        public HistoryControl()
        {
            InitializeComponent();
        }

        //
        public void Initialize(BookHub bookHub)
        {
            _VM = new HistoryControlVM(bookHub);
            this.DockPanel.DataContext = _VM;
        }

        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Update();
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
            var historyItem = (sender as ListBoxItem)?.Content as Book.Memento;
            if (historyItem != null)
            {
                _VM.Load(historyItem.Place);
            }
        }

        // 履歴項目決定(キー)
        private void HistoryListItem_KeyDown(object sender, KeyEventArgs e)
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
        private void HistoryList_KeyDown(object sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return)
            {
                e.Handled = true;
            }
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

        public LinkedList<Book.Memento> Items { get; private set; }

        //
        public HistoryControlVM(BookHub bookHub)
        {
            BookHub = bookHub;
            Update();
        }

        //
        public void Update()
        {
            if (ModelContext.BookHistory == null) return;
            Items = ModelContext.BookHistory.History;
            OnPropertyChanged(nameof(Items));
        }

        //
        public void Load(string path)
        {
            BookHub?.RequestLoad(path, BookLoadOption.None, false);
        }
    }
}
