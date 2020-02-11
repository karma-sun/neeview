using NeeView.Windows;
using System;
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
    /// AddressBarView
    /// .xaml の相互作用ロジック
    /// </summary>
    public partial class AddressBarView : UserControl
    {
        public static string DragDropFormat = $"{Config.Current.ProcessId}.BookAddress";

        public AddressBar Source
        {
            get { return (AddressBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(AddressBar), typeof(AddressBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AddressBarView)?.Initialize();
        }


        //
        public event DependencyPropertyChangedEventHandler IsAddressTextBoxFocusedChanged;

        /// <summary>
        /// constructor
        /// </summary>
        public AddressBarView()
        {
            InitializeComponent();

            this.AddressTextBox.IsKeyboardFocusedChanged += AddressTextBox_IsKeyboardFocusedChanged;
        }

        private void AddressTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsAddressTextBoxFocusedChanged?.Invoke(sender, e);
        }

        private AddressBarViewModel _vm;

        //
        public void Initialize()
        {
            _vm = new AddressBarViewModel(this.Source);
            this.Root.DataContext = _vm;
        }

        // アドレスバー入力
        private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _vm.Model.Address = this.AddressTextBox.Text;
            }

            // 単キーのショートカット無効
            KeyExGesture.AllowSingleKey = false;
            //e.Handled = true;
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        private void PageSortModeButton_Click(object sender, RoutedEventArgs e)
        {
            PageSortModePopup.IsOpen = true;
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            this.BookPopup.IsOpen = true;
        }

        #region DragDrop

        private DragDropGoast _goast = new DragDropGoast();
        private bool _isButtonDown;
        private Point _buttonDownPos;

        private void BookButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_vm.Model.IsBookEnabled)
            {
                return;
            }

            var element = sender as UIElement;
            _buttonDownPos = e.GetPosition(element);
            _isButtonDown = true;
        }

        private void BookButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private void BookButton_MouseMove(object sender, MouseEventArgs e)
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

                if (!_vm.Model.IsBookEnabled)
                {
                    return;
                }

                var data = new DataObject(new QueryPath(_vm.Model.Address));

                _goast.Attach(element, new Point(24, 24));
                DragDropWatcher.SetDragElement(sender, element);
                DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                DragDropWatcher.SetDragElement(sender, null);
                _goast.Detach();
            }
        }

        private void BookButton_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            _goast.QueryContinueDrag(sender, e);
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, (UIElement)sender);
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, null);
        }

        #endregion
    }
}
