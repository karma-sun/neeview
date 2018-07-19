using NeeLaboratory.Windows.Input;
using NeeLaboratory.Windows.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    /// FolderTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderTreeView : UserControl
    {
        public static string DragDropFormat = $"{Config.Current.ProcessId}.TreeViewItem";


        private FolderTreeViewModel _vm;

        public FolderTreeView()
        {
            InitializeComponent();

            _vm = new FolderTreeViewModel();

            _vm.SelectedItemChanged += ViewModel_SelectedItemChanged;

            this.Root.DataContext = _vm;
        }

        #region Commands

        private RelayCommand _addQuickAccessCommand;
        public RelayCommand AddQuickAccessCommand
        {
            get
            {
                return _addQuickAccessCommand = _addQuickAccessCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem;
                    if (item != null)
                    {
                        _vm.AddQuickAccess(item);
                    }
                }
            }
        }

        private RelayCommand _removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand = _removeCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem;
                    if (item != null)
                    {
                        _vm.Remove(item);
                    }
                }
            }
        }

        private RelayCommand _RefreshFolderCommand;
        public RelayCommand RefreshFolderCommand
        {
            get
            {
                return _RefreshFolderCommand = _RefreshFolderCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    _vm.RefreshFolder();
                }
            }
        }

        private RelayCommand _OpenExplorerCommand;
        public RelayCommand OpenExplorerCommand
        {
            get
            {
                return _OpenExplorerCommand = _OpenExplorerCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    var item = this.TreeView.SelectedItem as DirectoryNode;
                    if (item != null)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", item.Path);
                    }
                }
            }
        }




        #endregion

        public bool FocusSelectedItem()
        {
            if (this.TreeView.SelectedItem == null)
            {
                _vm.SelectRootQuickAccess();
            }

            return this.TreeView.Focus();
        }


        private void ViewModel_SelectedItemChanged(object sender, EventArgs e)
        {
            this.TreeView.Focus();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
#if false
            // scroll into view
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();
                e.Handled = true;
            }
#endif
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }


        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem viewItem)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TreeViewItem viewItem))
            {
                return;
            }

            if (e.Key == Key.Return)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Decide(viewItem.DataContext);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (viewItem.IsSelected)
                {
                    _vm.Remove(viewItem.DataContext);
                }
                e.Handled = true;
            }
        }

        private void TreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(sender is TreeViewItem viewItem))
            {
                return;
            }

            if (!viewItem.IsSelected)
            {
                return;
            }

            var contextMenu = viewItem.ContextMenu;
            contextMenu.Items.Clear();

            switch (viewItem.DataContext)
            {
                case RootQuickAccessNode rootQuickAccess:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuAddCurrentQuickAccess, Command = AddQuickAccessCommand });
                    break;

                case QuickAccessNode quickAccess:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuRemoveQuickAccess, Command = RemoveCommand });
                    break;

                case RootDirectoryNode rootFolder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuRefreshFolder, Command = RefreshFolderCommand });
                    break;

                case DirectoryNode folder:
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuExplorer, Command = OpenExplorerCommand });
                    contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.FolderTreeMenuAddQuickAccess, Command = AddQuickAccessCommand });
                    break;

                default:
                    e.Handled = true;
                    break;
            }
        }

        #region DragDrop

        private DependencyObject _lastDropTarget;

        private void DragStartBehavior_DragBegin(object sender, MouseEventArgs e)
        {
            _lastDropTarget = null;
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            ////Debug.WriteLine($"DragEnter: {sender}");
            TreeView_PreviewDragOver(sender, e);
            e.Handled = true;
        }

        private void TreeView_DragLeave(object sender, DragEventArgs e)
        {
            ////Debug.WriteLine($"DragLeave : {sender}");
            ////e.Handled = true;
        }

        private void TreeView_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DragDropFormat))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var element = PointToViewItem(this.TreeView, e.GetPosition(this.TreeView));

            if (element is TreeViewItem viewItem)
            {
                var item = e.Data.GetData(DragDropFormat);

                if (viewItem.DataContext is QuickAccessNode quickAccessTarget)
                {
                    if (item is QuickAccessNode quicklAccess)
                    {
                        e.Effects = DragDropEffects.Move;
                        e.Handled = true;
                        return;
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        private TreeViewItem PointToViewItem(TreeView treeView, Point point)
        {
            var element = VisualTreeHelper.HitTest(treeView, point)?.VisualHit;

            if (!(element is TreeViewItem))
            {
                element = VisualTreeUtility.GetParentElement<TreeViewItem>(element) ?? _lastDropTarget;
            }

#if false
            // debug dump
            if (element is TreeViewItem item)
            {
                var leftTop = item.TranslatePoint(new Point(0, 0), treeView);
                var size = new Size(item.ActualWidth, item.ActualHeight);
                Debug.WriteLine($"{point} -> {item.DataContext}: {leftTop},{size}");
            }
#endif

            _lastDropTarget = element;

            return _lastDropTarget as TreeViewItem;
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            var element = PointToViewItem(this.TreeView, e.GetPosition(this.TreeView));

            if (element is TreeViewItem viewItem)
            {
                var item = e.Data.GetData(DragDropFormat);

                switch (viewItem.DataContext)
                {
                    case QuickAccessNode quickAccessTarget:
                        if (item is QuickAccessNode quicklAccess)
                        {
                            _vm.MoveQuickAccess(quicklAccess, quickAccessTarget);
                            e.Handled = true;
                            return;
                        }
                        break;
                }
            }

            e.Handled = true;
        }

        #endregion


    }



}
