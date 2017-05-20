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

namespace NeeView
{
    /// <summary>
    /// ThumbnailList.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailList : UserControl
    {
        // TODO: これは仮です
        public MainWindowVM ViewModel
        {
            get { return (MainWindowVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainWindowVM), typeof(ThumbnailList), new PropertyMetadata(null, ViewMdoel_Changed));

        private static void ViewMdoel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailList control)
            {
                control._VM = control.ViewModel;
                control.Root.DataContext = control._VM;
            }
        }


        // TODO: これは仮です
        private MainWindowVM _VM;


        // サムネイルリストのパネルコントロール
        private VirtualizingStackPanel _thumbnailListPanel;

        /// <summary>
        /// 
        /// </summary>
        public ThumbnailList()
        {
            InitializeComponent();
        }


        private bool _isDartyThumbnailList = true;

        //
        private void ThumbnailListArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DartyThumbnailList();
        }

        // ??
        public void OnPageListChanged()
        {
            ////var sw = new Stopwatch();
            ////sw.Start();
            this.ThumbnailListBox.Items.Refresh();
            this.ThumbnailListBox.UpdateLayout();
            ////sw.Stop();
            ////Debug.WriteLine($"ThumbnailListBox: {sw.ElapsedMilliseconds}ms");
            DartyThumbnailList();
            LoadThumbnailList(+1);
        }


        //
        public void DartyThumbnailList(bool isUpdateNow = false)
        {
            _isDartyThumbnailList = true;

            if (isUpdateNow || this.Root.IsVisible)
            {
                UpdateThumbnailList();
            }
        }

        //
        public void UpdateThumbnailList()
        {
            UpdateThumbnailList(BookOperation.Current.Index, BookOperation.Current.IndexMax);
        }


        //
        private void UpdateThumbnailList(int index, int indexMax)
        {
            if (_thumbnailListPanel == null) return;

            if (!_VM.IsEnableThumbnailList) return;

            // リストボックス項目と同期がまだ取れていなければ処理しない
            //if (indexMax + 1 != this.ThumbnailListBox.Items.Count) return;

            // ここから
            if (!_isDartyThumbnailList) return;
            _isDartyThumbnailList = false;

            // 項目の幅 取得
            var listBoxItem = this.ThumbnailListBox.ItemContainerGenerator.ContainerFromIndex((int)_thumbnailListPanel.HorizontalOffset) as ListBoxItem;
            double itemWidth = (listBoxItem != null) ? listBoxItem.ActualWidth : 0.0;
            if (itemWidth <= 0.0) return;

            // 表示領域の幅
            double panelWidth = this.Root.ActualWidth;

            // 表示項目数を計算 (なるべく奇数)
            int itemsCount = (int)(panelWidth / itemWidth) / 2 * 2 + 1;
            if (itemsCount < 1) itemsCount = 1;

            // 表示先頭項目
            int topIndex = index - itemsCount / 2;
            if (topIndex < 0) topIndex = 0;

            // 少項目数補正
            if (indexMax + 1 < itemsCount)
            {
                itemsCount = indexMax + 1;
                topIndex = 0;
            }

            // ListBoxの幅を表示項目数にあわせる
            this.ThumbnailListBox.Width = itemWidth * itemsCount + 18; // TODO: 余裕が必要？

            // 表示項目先頭指定
            _thumbnailListPanel.SetHorizontalOffset(topIndex);

            // 選択
            this.ThumbnailListBox.SelectedIndex = index;

            // ##
            ////Debug.WriteLine(topIndex + " / " + this.ThumbnailListBox.Items.Count);

            // アライメント更新
            ThumbnailListBox_UpdateAlignment();
        }


        #region ThunbnailList event func


        private void ThumbnailListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBoxPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            _thumbnailListPanel = sender as VirtualizingStackPanel;
            DartyThumbnailList();
        }

        // TODO: 何度も来るのでいいかんじにする
        private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                this.ThumbnailListBox.SelectedIndex = BookOperation.Current.Index;
                return;
            }

            ThumbnailListBox_UpdateAlignment();
        }

        private void ThumbnailListBox_UpdateAlignment()
        {
            // 端の表示調整
            if (this.ThumbnailListBox.Width > this.Root.ActualWidth)
            {
                if (this.ThumbnailListBox.SelectedIndex <= 0)
                {
                    this.ThumbnailListBox.HorizontalAlignment = _VM.IsSliderDirectionReversed ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                }
                else if (this.ThumbnailListBox.SelectedIndex >= this.ThumbnailListBox.Items.Count - 1)
                {
                    this.ThumbnailListBox.HorizontalAlignment = _VM.IsSliderDirectionReversed ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                }
                else
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        // リストボックスのドラッグ機能を無効化する
        private void ThumbnailListBox_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ThumbnailListBox.IsMouseCaptured)
            {
                this.ThumbnailListBox.ReleaseMouseCapture();
            }
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right);
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBoxPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 決定
            if (e.Key == Key.Return)
                _VM.BookHub.JumpPage(this.ThumbnailListBox.SelectedItem as Page);
            // 左右スクロールは自前で実装
            else if (e.Key == Key.Right)
                ThumbnailListBox_MoveSelectedIndex(+1);
            else if (e.Key == Key.Left)
                ThumbnailListBox_MoveSelectedIndex(-1);

            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return);
        }

        //
        private void ThumbnailListBox_MoveSelectedIndex(int delta)
        {
            if (_thumbnailListPanel == null || this.ThumbnailListBox.SelectedIndex < 0) return;

            if (_thumbnailListPanel.FlowDirection == FlowDirection.RightToLeft)
                delta = -delta;

            int index = this.ThumbnailListBox.SelectedIndex + delta;
            if (index < 0)
                index = 0;
            if (index >= this.ThumbnailListBox.Items.Count)
                index = this.ThumbnailListBox.Items.Count - 1;

            this.ThumbnailListBox.SelectedIndex = index;
            this.ThumbnailListBox.ScrollIntoView(this.ThumbnailListBox.SelectedItem);
        }


        // 履歴項目決定
        private void ThumbnailListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _VM.BookHub.JumpPage(page);
                e.Handled = true;
            }
        }


        // スクロールしたらサムネ更新
        private void ThumbnailList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_thumbnailListPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnailList(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // サムネ更新。表示されているページのサムネの読み込み要求
        public void LoadThumbnailList(int direction)
        {
            if (!this.Root.IsVisible) return;

            if (_thumbnailListPanel != null)
            {
                _VM.RequestThumbnail((int)_thumbnailListPanel.HorizontalOffset, (int)_thumbnailListPanel.ViewportWidth, 2, direction);
            }
        }

        private void ThumbnailListArea_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadThumbnailList(1);
        }

        private void ThumbnailListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int count = MouseInputHelper.DeltaCount(e);
            int delta = e.Delta < 0 ? +count : -count;
            if (_VM.IsSliderDirectionReversed) delta = -delta;
            ThumbnailListBox_MoveSelectedIndex(delta);
            e.Handled = true;
        }

        #endregion
    }

}
