using System.Collections.Generic;
using System.Diagnostics;
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
    /// ThumbnailListView.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailListView : UserControl
    {
        public ThumbnailList Source
        {
            get { return (ThumbnailList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ThumbnailList), typeof(ThumbnailListView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailListView control)
            {
                control.Initialize();
            }
        }




        //
        private ThumbnailListViewModel _vm;


        // サムネイルリストのパネルコントロール
        private VirtualizingStackPanel _listPanel;

        /// <summary>
        /// 
        /// </summary>
        public ThumbnailListView()
        {
            InitializeComponent();
        }

        //
        private void Initialize()
        {
            _vm = new ThumbnailListViewModel(this.Source);
            _vm.Model.Refleshed += (s, e) => OnPageListChanged();
            _vm.AddPropertyChanged(nameof(_vm.PageNumber), (s, e) => DartyThumbnailList());

            this.ThumbnailListBox.ManipulationBoundaryFeedback += _vm.Model.ScrollViewer_ManipulationBoundaryFeedback;

            this.Root.DataContext = _vm;
        }


        private bool _isDartyThumbnailList = true;

        //
        private void ThumbnailListArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DartyThumbnailList();
        }

        /// <summary>
        /// リスト刷新
        /// </summary>
        private void OnPageListChanged()
        {
            this.ThumbnailListBox.Items.Refresh();
            this.ThumbnailListBox.UpdateLayout();
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
            App.Current?.Dispatcher.Invoke(() => UpdateThumbnailList(_vm.PageNumber, _vm.MaxPageNumber));
        }


        //
        private void UpdateThumbnailList(int index, int indexMax)
        {
            if (_listPanel == null) return;

            if (!_vm.Model.IsEnableThumbnailList) return;

            // リストボックス項目と同期がまだ取れていなければ処理しない
            //if (indexMax + 1 != this.ThumbnailListBox.Items.Count) return;

            // ここから
            if (!_isDartyThumbnailList) return;
            _isDartyThumbnailList = false;

            // 選択
            this.ThumbnailListBox.SelectedIndex = index;


            if (_vm.Model.IsSelectedCenter)
            {
                var scrollUnit = VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox);

                // 項目の幅 取得
                double itemWidth = GetItemWidth();
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
                var horizontalOffset = scrollUnit == ScrollUnit.Item ? topIndex : topIndex * itemWidth;
                _listPanel.SetHorizontalOffset(horizontalOffset);
            }
            else
            {
                this.ThumbnailListBox.Width = double.NaN;
                this.ThumbnailListBox.ScrollIntoView(this.ThumbnailListBox.SelectedItem);
            }

            // ##
            ////Debug.WriteLine(topIndex + " / " + this.ThumbnailListBox.Items.Count);

            // アライメント更新
            ThumbnailListBox_UpdateAlignment();
        }


        //
        private double GetItemWidth()
        {
            if (_listPanel == null || _listPanel.Children.Count <= 0) return 0.0;

            return (_listPanel.Children[0] as ListBoxItem).ActualWidth;
        }


        #region ThunbnailList event func


        private void ThumbnailListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBoxPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            _listPanel = sender as VirtualizingStackPanel;
            DartyThumbnailList();
        }

        // TODO: 何度も来るのでいいかんじにする
        private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                this.ThumbnailListBox.SelectedIndex = _vm.PageNumber;
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
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (this.ThumbnailListBox.SelectedIndex >= this.ThumbnailListBox.Items.Count - 1)
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Right;
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
                _vm.Model.BookOperation.JumpPage(this.ThumbnailListBox.SelectedItem as Page);
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
            if (_listPanel == null || this.ThumbnailListBox.SelectedIndex < 0) return;

            if (_listPanel.FlowDirection == FlowDirection.RightToLeft)
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
                _vm.Model.BookOperation.JumpPage(page);
                e.Handled = true;
            }
        }


        // スクロールしたらサムネ更新
        private void ThumbnailList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_listPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnailList(e.HorizontalChange < 0 ? -1 : +1);
            }
        }


        // サムネ更新。表示されているページのサムネの読み込み要求
        public void LoadThumbnailList(int direction)
        {
            if (!this.Root.IsVisible) return;
            if (_listPanel == null || !this.ThumbnailListBox.IsVisible || _listPanel.Children.Count <= 0) return;

            var scrollUnit = VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox);

            int start;
            int count;

            if (scrollUnit == ScrollUnit.Item)
            {
                start = (int)_listPanel.HorizontalOffset;
                count = (int)_listPanel.ViewportWidth;
            }
            else if (scrollUnit == ScrollUnit.Pixel)
            {
                var itemWidth = (_listPanel.Children[0] as ListBoxItem).ActualWidth;
                if (itemWidth <= 0.0) return; // 項目の準備ができていない？
                start = (int)(_listPanel.HorizontalOffset / itemWidth);
                count = (int)(_listPanel.ViewportWidth / itemWidth) + 1;
            }
            else
            {
                return;
            }

            // タイミングにより計算値が不正になることがある対策
            // 再現性が低い
            if (count < 0)
            {
                Debug.WriteLine($"Error Value!: {count}");
                return;
            }

            _vm.RequestThumbnail(start, count, 2, direction);
        }


        //
        private void ThumbnailListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) return;
            LoadThumbnailList(1);
        }



        private void ThumbnailListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int count = MouseInputHelper.DeltaCount(e);
            int delta = e.Delta < 0 ? +count : -count;
            if (PageSlider.Current.IsSliderDirectionReversed) delta = -delta;
            ThumbnailListBox_MoveSelectedIndex(delta);
            e.Handled = true;
        }

        #endregion
    }

}
