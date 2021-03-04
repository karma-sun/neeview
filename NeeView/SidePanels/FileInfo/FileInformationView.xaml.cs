using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
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
    /// FileInformationView.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInformationView : UserControl
    {
        public static string DragDropFormat = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(), nameof(FileInformationView));

        #region RoutedCommand

        public static readonly RoutedCommand OpenExplorerCommand = new RoutedCommand(nameof(OpenExplorerCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenExternalAppCommand = new RoutedCommand(nameof(OpenExternalAppCommand), typeof(FileInformationView));
        public static readonly RoutedCommand CopyCommand = new RoutedCommand(nameof(CopyCommand), typeof(FileInformationView));
        public static readonly RoutedCommand CopyToFolderCommand = new RoutedCommand(nameof(CopyToFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand MoveToFolderCommand = new RoutedCommand(nameof(MoveToFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenDestinationFolderCommand = new RoutedCommand(nameof(OpenDestinationFolderCommand), typeof(FileInformationView));
        public static readonly RoutedCommand OpenExternalAppDialogCommand = new RoutedCommand(nameof(OpenExternalAppDialogCommand), typeof(FileInformationView));
        public static readonly RoutedCommand PagemarkCommand = new RoutedCommand(nameof(PagemarkCommand), typeof(FileInformationView));

        private InformationPageCommandResource _commandResource = new InformationPageCommandResource();

        private static void InitializeCommandStatic()
        {
            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            PagemarkCommand.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
        }

        private void InitializeCommand()
        {
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExplorerCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(CopyToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(MoveToFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenDestinationFolderCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(OpenExternalAppDialogCommand));
            this.ThumbnailListBox.CommandBindings.Add(_commandResource.CreateCommandBinding(PagemarkCommand));
        }

        #endregion RoutedCommand


        private FileInformationViewModel _vm;
        private bool _isFocusRequest;


        static FileInformationView()
        {
            InitializeCommandStatic();
        }

        public FileInformationView()
        {
            InitializeComponent();
            InitializeCommand();

            this.ThumbnailListBox.ContextMenuOpening += ThumbnailListBoxItem_ContextMenuOpening;
        }

        public FileInformationView(FileInformation model) : this()
        {
            _vm = new FileInformationViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += FileInformationView_IsVisibleChanged;

            // タッチスクロール操作の終端挙動抑制
            this.ScrollView.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
        }


        private void FileInformationView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }

        private void ThumbnailListBoxItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var container = sender as ListBoxItem;
            if (container == null)
            {
                return;
            }

            var item = (container.Content as FileInformationSource)?.ViewContent?.Page;
            if (item == null)
            {
                return;
            }

            var contextMenu = container.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            var listBox = this.ThumbnailListBox;
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Pagemark, Command = PagemarkCommand, IsChecked = _commandResource.Pagemark_IsChecked(listBox) });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Explorer, Command = OpenExplorerCommand });
            contextMenu.Items.Add(ExternalAppCollectionUtility.CreateExternalAppItem(_commandResource.OpenExternalApp_CanExecute(listBox), OpenExternalAppCommand, OpenExternalAppDialogCommand));
            contextMenu.Items.Add(new MenuItem() { Header = Properties.Resources.PageListItem_Menu_Copy, Command = CopyCommand });
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_CopyToFolder, _commandResource.CopyToFolder_CanExecute(listBox), CopyToFolderCommand, OpenDestinationFolderCommand));
            contextMenu.Items.Add(DestinationFolderCollectionUtility.CreateDestinationFolderItem(Properties.Resources.PageListItem_Menu_MoveToFolder, _commandResource.MoveToFolder_CanExecute(listBox), MoveToFolderCommand, OpenDestinationFolderCommand));
        }

        #region DragDrop

        private async Task DragStartBehavior_DragBeginAsync(object sender, Windows.DragStartEventArgs e, CancellationToken token)
        {
            var pages = this.ThumbnailListBox.SelectedItems.Cast<FileInformationSource>().Select(x => x.ViewContent?.Page).Where(x => x != null).ToList();
            if (!pages.Any())
            {
                e.Cancel = true;
                return;
            }

            var isSuccess = await Task.Run(() => ClipboardUtility.SetData(e.Data, pages, new CopyFileCommandParameter() { MultiPagePolicy = MultiPagePolicy.All }, token));
            if (!isSuccess)
            {
                e.Cancel = true;
                return;
            }

            // 全てのファイルがファイルシステムであった場合のみ
            if (pages.All(p => p.Entry.IsFileSystem))
            {
                // 右クリックドラッグでファイル移動を許可
                if (Config.Current.System.IsFileWriteAccessEnabled && e.MouseEventArgs.RightButton == MouseButtonState.Pressed)
                {
                    e.AllowedEffects |= DragDropEffects.Move;
                }

                // TODO: ドラッグ終了時にファイル移動の整合性を取る必要がある。
                // しっかり実装するならページのファイルシステムの監視が必要になる。ファイルの追加削除が自動的にページに反映するように。

                // ひとまずドラッグ完了後のページ削除を限定的に行う。
                e.DragEndAction = () => BookOperation.Current.ValidateRemoveFile(pages);
            }
        }

        #endregion

        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoreButton.IsChecked = !MoreButton.IsChecked;
            e.Handled = true;
        }

        private void MoreButton_Checked(object sender, RoutedEventArgs e)
        {
            ContextMenuWatcher.SetTargetElement((UIElement)sender);
        }
    }

    public class InformationPageCommandResource : PageCommandResource
    {
        protected override Page GetSelectedPage(object sender)
        {
            var listBox = sender as ListBox;
            return (listBox.SelectedItem as FileInformationSource)?.ViewContent?.Page;
        }

        protected override List<Page> GetSelectedPages(object sender)
        {
            var listBox = sender as ListBox;
            return listBox.SelectedItems
                .Cast<FileInformationSource>()
                .Select(e => e.ViewContent?.Page)
                .Where(e => e != null)
                .ToList();
        }
    }


}
