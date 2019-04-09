using System;
using System.Collections.Generic;
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
    /// ThumbnailListView.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailListView : UserControl
    {
        #region DependencyProperties

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

        #endregion

        #region Fields

        // フィルムストリップのパネルコントロール
        private VirtualizingStackPanel _listPanel;

        private ThumbnailListViewModel _vm;
        private bool _isDartyThumbnailList = true;

        /// <summary>
        /// サムネイル更新要求を拒否する
        /// </summary>
        private bool _isFreezed;

        /// <summary>
        /// サムネイル更新回数
        /// </summary>
        private volatile int _thumbnailRequestCount;

        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();

        #endregion

        #region Constructors

        public ThumbnailListView()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            _vm = new ThumbnailListViewModel(this.Source);
            _vm.Model.BookChanging += ThumbnailList_BookChanging;
            _vm.Model.BookChanged += ThumbnailList_BookChanged;
            _vm.Model.ViewItemsChanged += ViewModel_ViewItemsChanged;
            _vm.Model.AddPropertyChanged(nameof(_vm.Model.SelectedIndex), ViewModel_SelectedIdexChanged);

            this.ThumbnailListBox.ManipulationBoundaryFeedback += _vm.Model.ScrollViewer_ManipulationBoundaryFeedback;

            this.Root.DataContext = _vm;
        }

        private void ThumbnailList_BookChanging(object sender, EventArgs e)
        {
            _isFreezed = true;
        }

        private void ThumbnailList_BookChanged(object sender, BookChangedEventArgs e)
        {
            // NOTE: 変更が ThumbnailListBox に反映されるまで遅延
            // HACK: Control.UpdateLayout()で即時確定させる？
            AppDispatcher.BeginInvoke(() =>
            {
                ////Debug.WriteLine("> Ensure thumbnail update.");
                _isFreezed = false;
                LoadThumbnailList(+1);
            });
        }


        private void ViewModel_SelectedIdexChanged(object sender, PropertyChangedEventArgs e)
        {
            // NOTE: 選択が ThumbnailListBox に反映されるまで遅延
            // HACK: Control.UpdateLayout()で即時確定させる？
            AppDispatcher.BeginInvoke(() => DartyThumbnailList());
        }

        private void ViewModel_ViewItemsChanged(object sender, ViewItemsChangedEventArgs e)
        {
            UpdateViewItems(e.ViewItems, e.Direction);
        }

        private void UpdateViewItems(List<Page> items, int direction)
        {
            if (_vm == null) return;
            if (!this.ThumbnailListBox.IsLoaded) return;
            if (_vm.Model.Items == null) return;
            if (_vm.Model.IsItemsDarty) return;
            if (!this.IsVisible) return;

            if (items.Count == 1)
            {
                ScrollIntoView(items.First());
            }
            else if (direction < 0)
            {
                ScrollIntoView(items.First());
            }
            else if (direction > 0)
            {
                ScrollIntoView(items.Last());
            }
            else
            {
                foreach (var item in items)
                {
                    ScrollIntoView(item);
                    // NOTE: ScrollIntoView結果を反映させるため
                    this.ThumbnailListBox.UpdateLayout();
                }
            }
        }

        private void ScrollIntoView(object item)
        {
            //// Debug.WriteLine($"> ScrollInoView: {item}");
            this.ThumbnailListBox.ScrollIntoView(item);
        }

        private void ThumbnailListArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DartyThumbnailList();
        }

        public void DartyThumbnailList(bool isUpdateNow = false)
        {
            if (_vm == null) return;

            _isDartyThumbnailList = true;

            if (isUpdateNow || this.Root.IsVisible)
            {
                UpdateThumbnailList();
            }
        }

        public void UpdateThumbnailList()
        {
            UpdateThumbnailList(_vm.Model.SelectedIndex, _vm.Model.PageSelector.MaxIndex);
        }

        private void UpdateThumbnailList(int index, int indexMax)
        {
            if (_listPanel == null) return;

            if (!_vm.Model.IsEnableThumbnailList) return;

            if (!_isDartyThumbnailList) return;
            _isDartyThumbnailList = false;

            if (_vm.Model.IsSelectedCenter)
            {
                var scrollUnit = VirtualizingStackPanel.GetScrollUnit(this.ThumbnailListBox);

                // 項目の幅 取得
                double itemWidth = GetItemWidth();
                if (itemWidth <= 0.0) return;

                // 表示領域の幅
                double panelWidth = this.Root.ActualWidth;

                // 表示項目数を計算 (なるべく奇数)
                int itemsCount = (int)(panelWidth / itemWidth) / 2 * 2 + 3;
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
                this.ThumbnailListBox.UpdateLayout();
                ScrollIntoView(this.ThumbnailListBox.SelectedItem);
            }

            ////Debug.WriteLine(topIndex + " / " + this.ThumbnailListBox.Items.Count);

            // アライメント更新
            ThumbnailListBox_UpdateAlignment();
        }

        private double GetItemWidth()
        {
            if (_listPanel == null || _listPanel.Children.Count <= 0) return 0.0;

            return (_listPanel.Children[0] as ListBoxItem).ActualWidth;
        }

        // サムネ更新。表示されているページのサムネの読み込み要求
        private void LoadThumbnailList(int direction)
        {
            if (_vm == null) return;
            if (_isFreezed) return;

            if (!this.Root.IsVisible || !this.ThumbnailListBox.IsVisible || _listPanel == null || _listPanel.Children.Count <= 0)
            {
                _vm.CancelThumbnailRequest();
                return;
            }

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
                var itemWidth = GetItemWidth();
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

            _thumbnailRequestCount++;
            _vm.RequestThumbnail(start, count, 2, direction);
        }

        private void MoveSelectedIndex(int delta)
        {
            if (_listPanel == null || _vm.Model.SelectedIndex < 0) return;

            if (_listPanel.FlowDirection == FlowDirection.RightToLeft)
                delta = -delta;

            _vm.MoveSelectedIndex(delta);
        }

        #endregion

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
            LoadThumbnailList(+1);
        }

        private void ThumbnailListBox_UpdateAlignment()
        {
            // 端の表示調整
            if (this.ThumbnailListBox.Width > this.Root.ActualWidth)
            {
                if (_vm.Model.SelectedIndex <= 0)
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (_vm.Model.SelectedIndex >= this.ThumbnailListBox.Items.Count - 1)
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
                MouseInputHelper.ReleaseMouseCapture(this, this.ThumbnailListBox);
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
                BookOperation.Current.JumpPage(this.ThumbnailListBox.SelectedItem as Page);
            // 左右スクロールは自前で実装
            else if (e.Key == Key.Right)
                MoveSelectedIndex(+1);
            else if (e.Key == Key.Left)
                MoveSelectedIndex(-1);

            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return);
        }


        private void ThumbnailListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ////if (!(bool)e.NewValue) return;
            LoadThumbnailList(+1);
        }

        private void ThumbnailListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = -_mouseWheelDelta.NotchCount(e);
            if (delta != 0)
            {
                if (PageSlider.Current.IsSliderDirectionReversed) delta = -delta;
                MoveSelectedIndex(delta);
            }
            e.Handled = true;
        }

        private void ThumbnailListBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (_vm == null) return;
            _vm.Model.IsItemsDarty = false;
        }

        // スクロールしたらサムネ更新
        private void ThumbnailList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_listPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnailList(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // 履歴項目決定
        private void ThumbnailListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                BookOperation.Current.JumpPage(page);
            }
        }

        #endregion

    }

}
