using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using NeeView.Windows;

namespace NeeView
{
    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListView : UserControl
    {
        #region Fields

        private volatile bool _requestSearchBoxFocus;

        private FolderListViewModel _vm;

        private int _busyCounter;

        #endregion


        #region Constructors

        public FolderListView()
        {
            InitializeComponent();
        }

        public FolderListView(FolderList model) : this()
        {
            this.FolderTree.Model = new BookshelfFolderTreeModel(model);

            _vm = new FolderListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += FolderList_SearchBoxFocus;
            model.FolderTreeFocus += FolderList_FolderTreeFocus;
            model.BusyChanged += FolderList_BusyChanged;
        }

        #endregion


        public bool IsRenaming => _vm.Model.IsRenaming || this.FolderTree.IsRenaming;

        public bool IsSearchBoxFocused => this.SearchBox.IsKeyboardFocusWithin;


        /// <summary>
        /// フォルダーツリーへのフォーカス要求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_FolderTreeFocus(object sender, System.IO.ErrorEventArgs e)
        {
            if (!_vm.Model.IsFolderTreeVisible) return;

            this.FolderTree.FocusSelectedItem();
        }


        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_SearchBoxFocus(object sender, EventArgs e)
        {
            if (!_vm.Model.IsFolderSearchBoxVisible) return;

            if (!_requestSearchBoxFocus)
            {
                _requestSearchBoxFocus = true;
                var task = FocustSearchBoxAsync(); // 非同期
            }
        }


        /// <summary>
        /// リスト更新中
        /// </summary>
        private void FolderList_BusyChanged(object sender, BusyChangedEventArgs e)
        {
            _busyCounter += e.IsBusy ? +1 : -1;
            if (_busyCounter <= 0)
            {
                this.FolderListBox.IsHitTestVisible = true;
                this.FolderListBox.BeginAnimation(UIElement.OpacityProperty, null);
                this.FolderListBox.Opacity = 1.0;

                this.BusyFadeContent.Content = null;
                _busyCounter = 0;
            }
            else if (_busyCounter > 0 && this.BusyFadeContent.Content == null)
            {
                this.FolderListBox.IsHitTestVisible = false;
                this.FolderListBox.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(1.0) });

                this.BusyFadeContent.Content = new BusyFadeView();
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカスをあわせる。
        /// </summary>
        /// <returns></returns>
        private async Task FocustSearchBoxAsync()
        {
            // 表示が間に合わない場合があるので繰り返しトライする
            while (_requestSearchBoxFocus && _vm.Model.IsFolderSearchBoxVisible)
            {
                var searchBox = this.SearchBox;
                if (searchBox != null && searchBox.IsLoaded && searchBox.IsVisible && this.IsVisible)
                {
                    searchBox.Focus();
                    var isFocused = searchBox.IsKeyboardFocusWithin;
                    //Debug.WriteLine($"Focus: {isFocused}");
                    if (isFocused) break;
                }

                //Debug.WriteLine($"Focus: ready...");
                await Task.Delay(100);
            }

            _requestSearchBoxFocus = false;
            //Debug.WriteLine($"Focus: done.");
        }



        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPrevButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderNextButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        //
        private void FolderListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// 単キーのショートカット無効
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        /// <summary>
        /// 検索ボックスでのキー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;

            if (e.Key == Key.Enter)
            {
                _vm.Model.RequestSearchPlace(false);
            }
        }


        /// <summary>
        /// SearchBox: キーボードフォーカス変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"SBF.K: {this.SearchBox.IsKeyboardFocusWithin}");

            // リストのフォーカス更新を停止
            _vm.SetListFocusEnabled(!this.SearchBox.IsKeyboardFocusWithin);

            // パネル表示状態を更新
            SidePanelFrameView.Current?.UpdateVisibility();

            // フォーカス解除で履歴登録
            if (!this.SearchBox.IsKeyboardFocusWithin)
            {
                _vm.UpdateSearchHistory();
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                _vm.Model.AreaWidth = e.NewSize.Width;
            }
            if (e.HeightChanged)
            {
                _vm.Model.AreaHeight = e.NewSize.Height;
            }
        }

        private void MoreButton_Checked(object sender, RoutedEventArgs e)
        {
            _vm.UpdateMoreMenu();
        }

        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoreButton.IsChecked = !MoreButton.IsChecked;
            e.Handled = true;
        }

        public void Refresh()
        {
            _vm.FolderListBox?.Refresh();
        }


        #region DragDrop

        private DragDropGoast _goast = new DragDropGoast();
        private bool _isButtonDown;
        private Point _buttonDownPos;

        private void PlaceIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            _buttonDownPos = e.GetPosition(element);
            _isButtonDown = true;
        }

        private void PlaceIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private void PlaceIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isButtonDown)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                _isButtonDown = false;
                return;
            }

            var element = sender as UIElement;

            var pos = e.GetPosition(element);
            if (DragDropHelper.IsDragDistance(pos, _buttonDownPos))
            {
                _isButtonDown = false;

                if (_vm.Model.Place == null)
                {
                    return;
                }
                if (_vm.Model.Place.Scheme != QueryScheme.File && _vm.Model.Place.Scheme != QueryScheme.Bookmark)
                {
                    return;
                }

                var data = new DataObject(_vm.Model.Place);

                _goast.Attach(this.PlaceBar, new Point(24, 24));
                DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                _goast.Detach();
            }
        }

        private void PlaceIcon_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            _goast.QueryContinueDrag(sender, e);
        }

        #endregion
    }


    #region Converters

    public class PathToPlaceIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value as QueryPath);
        }

        public static ImageSource Convert(QueryPath path)
        {
            if (path != null)
            {
                if (path.Path == null)
                {
                    return path.Scheme.ToImage();
                }
                else if (path.Scheme == QueryScheme.Bookmark)
                {
                    return path.Scheme.ToImage();
                }
                else if (path.Scheme == QueryScheme.Pagemark)
                {
                    return path.Scheme.ToImage();
                }
                else if (path.Search != null)
                {
                    return MainWindow.Current.Resources["ic_search_24px"] as ImageSource;
                }
                else if (path.Scheme == QueryScheme.File && PlaylistArchive.IsSupportExtension(path.SimplePath))
                {
                    return MainWindow.Current.Resources["ic_playlist"] as ImageSource;
                }
            }

            return FileIconCollection.Current.CreateDefaultFolderIcon(16.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(FolderCollection), typeof(Visibility))]
    public class FolderCollectionToFolderRecursiveVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderCollection collection)
            {
                return (collection.FolderParameter.IsFolderRecursive == true) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion Converters
}
