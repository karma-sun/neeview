// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


// TODO: サムネイル処理をクラス化して共有する


namespace NeeView
{
    /// <summary>
    /// FolderList.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderList : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkControl));
        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand("OpenExplorerCommand", typeof(BookmarkControl));

        static FolderList()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public event EventHandler<string> Decided;
        public event EventHandler<string> Moved;
        public event EventHandler<string> MovedParent;
        public event EventHandler<int> SelectionChanged;


        private ThumbnailHelper _thumbnailHelper;


        private FolderListVM _VM;
        private bool _autoFocus;

        public FolderInfo SelectedItem => this.ListBox.SelectedItem as FolderInfo;

        //
        public FolderList(FolderListVM vm, bool autoFocus)
        {
            _autoFocus = autoFocus;

            InitializeComponent();

            _VM = vm;
            this.ListBox.DataContext = _VM;

            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
            this.ListBox.CommandBindings.Add(new CommandBinding(OpenExplorerCommand, OpenExplorer_Exec));

            _thumbnailHelper = new ThumbnailHelper(this.ListBox, _VM.RequestThumbnail);
        }

        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null)
            {
                _VM.Remove(item);
            }
        }

        //
        public void OpenExplorer_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as FolderInfo;
            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + item.Path + "\"");
            }
        }

        //
        public void SetSelectedIndex(int index)
        {
            _VM.SelectedIndex = index;
        }

        //
        public void FocusSelectedItem(bool isFocus)
        {
            if (this.ListBox.SelectedIndex < 0) return;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (isFocus)
            {
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }


        // フォルダリスト 選択項目変更
        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
                SelectionChanged?.Invoke(this, listBox.SelectedIndex);
            }
        }


        // フォルダ項目決定
        private void FolderListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
            if (folderInfo != null && !folderInfo.IsEmpty)
            {
                Decided?.Invoke(this, folderInfo.Path);
                e.Handled = true;
            }
        }

        // フォルダ移動決定
        private void FolderListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
            if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
            {
                Moved?.Invoke(this, folderInfo.Path);
            }
            e.Handled = true;
        }

        // フォルダ移動決定(キー)
        private void FolderListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var folderInfo = (sender as ListBoxItem)?.Content as FolderInfo;
            {
                if (e.Key == Key.Return)
                {
                    Decided?.Invoke(this, folderInfo.Path);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right) // →
                {
                    if (folderInfo != null && folderInfo.IsDirectory && folderInfo.IsReady)
                    {
                        Moved?.Invoke(this, folderInfo.Path);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Left || e.Key == Key.Back) // ← Backspace
                {
                    if (folderInfo != null)
                    {
                        MovedParent?.Invoke(this, folderInfo.ParentPath);
                    }
                    e.Handled = true;
                }
            }
        }

        // フォルダ移動決定(キー)
        private void FolderList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            if (e.Key == Key.Left || e.Key == Key.Back) // Backspace
            {
                MovedParent?.Invoke(this, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return)
            {
                e.Handled = true;
            }
        }

        // ロードイベント
        private void FolderList_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem(_autoFocus);
        }
    }


    /// <summary>
    /// FolderList ViewModel
    /// </summary>
    public class FolderListVM : INotifyPropertyChanged
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

        public FolderCollection FolderCollection { get; set; }

        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        public double PicturePanelHeight => ThumbnailHeight + 24.0;

        public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);
        public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);

        #region Property: SelectedIndex
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public FolderListVM()
        {
            OnPropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => OnPropertyChanged(nameof(FolderListItemStyle));
        }


        public void Remove(FolderInfo info)
        {
            if (info.IsEmpty) return;

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            var thumbnail = new Image();
            thumbnail.SnapsToDevicePixels = true;
            thumbnail.Source = info.Icon;
            thumbnail.Width = 32;
            thumbnail.Height = 32;
            thumbnail.Margin = new System.Windows.Thickness(0, 0, 4, 0);
            stackPanel.Children.Add(thumbnail);
            var textblock = new TextBlock();
            textblock.Text = info.Path;
            textblock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(textblock);
            stackPanel.Margin = new Thickness(0, 0, 0, 20);

            Messenger.Send(this, new MessageEventArgs("RemoveFile") { Parameter = new RemoveFileParams() { Path = info.Path, Visual = stackPanel } });
        }


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            PanelContext.ThumbnailManager.RequestThumbnail(FolderCollection.Items, start, count, margin, direction);
        }
    }
}
