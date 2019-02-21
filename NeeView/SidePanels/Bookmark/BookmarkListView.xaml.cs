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
    /// BookmarkListView.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkListView : UserControl
    {
        #region Fields

        private BookmarkListViewModel _vm;
        private int _busyCounter;

        #endregion

        #region Constructors

        public BookmarkListView()
        {
            InitializeComponent();
        }

        public BookmarkListView(FolderList model) : this()
        {
            this.FolderTree.Model = new FolderTreeModel(model, FolderTreeCategory.BookmarkFolder);

            _vm = new BookmarkListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.FolderTreeFocus += FolderList_FolderTreeFocus;
            model.BusyChanged += FolderList_BusyChanged;
        }

        #endregion


        public bool IsRenaming => _vm.Model.IsRenaming || this.FolderTree.IsRenaming;


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

        //
        private void BookmarkListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
    }
}
