using NeeLaboratory.Windows.Input;
using NeeView.Windows;
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

namespace NeeView.Runtime.LayoutPanel
{
    /// <summary>
    /// LayoutPanelContainer.xaml の相互作用ロジック
    /// </summary>
    public partial class LayoutPanelContainer : UserControl
    {
        private LayoutPanelContainerAdorner _adorner;
        private LayoutPanelManager _provider;


        public LayoutPanelContainer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public LayoutPanelContainer(LayoutPanelManager manager, LayoutPanel layoutPanel) : this()
        {
            _provider = manager;
            LayoutPanel = layoutPanel;
            this.Loaded += LayoutPanelContainer_Loaded;
        }


        public LayoutPanel LayoutPanel
        {
            get { return (LayoutPanel)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(LayoutPanelContainer), new PropertyMetadata(null));



        private void LayoutPanelContainer_Loaded(object sender, RoutedEventArgs e)
        {
            _adorner = _adorner ?? new LayoutPanelContainerAdorner(this);

            this.DragOver += LayoutPanelContainer_DragOver;
            this.DragLeave += LayoutPanelContainer_DragLeave;
            this.Drop += LayoutPanelContainer_Drop;
            this.AllowDrop = true;
        }

        public void Snap()
        {
            // TODO: WindowPlacement情報破棄 はダメ。復元できるように。
            LayoutPanel.WindowPlacement = WindowPlacement.None;
        }


        #region Commands

        private RelayCommand _closePanelCommand;
        public RelayCommand ClosePanelCommand
        {
            get { return _closePanelCommand = _closePanelCommand ?? new RelayCommand(ClosePanelCommand_Executed); }
        }

        private void ClosePanelCommand_Executed()
        {
            _provider.StandAlone(LayoutPanel);
            _provider.Close(LayoutPanel);
        }


        private RelayCommand _openPanelWindowCommand;
        public RelayCommand OpenPanelWindowCommand
        {
            get { return _openPanelWindowCommand = _openPanelWindowCommand ?? new RelayCommand(OpenPanelWindowCommand_Executed); }
        }

        private void OpenPanelWindowCommand_Executed()
        {
            var owner = Window.GetWindow(this);
            var point = this.PointToScreen(new Point(0.0, 0.0));

            _provider.OpenWindow(LayoutPanel, new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)ActualWidth, (int)ActualHeight));
        }

        #endregion Commands

        #region DragDrop

        private void LayoutPanelContainer_Drop(object sender, DragEventArgs e)
        {
            _adorner.Detach();

            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null) return;

            var dock = GetLayoutDockFromPosY(e.GetPosition(this).Y, this.ActualHeight);


            if (this.Parent is LayoutDockPanel dockPanel)
            {
                // 挿入位置
                var list = dockPanel.ItemsSource;
                var index = list.IndexOf(this.LayoutPanel);
                if (index < 0) throw new InvalidOperationException();

                if (list.Contains(content))
                {
                    // list内での移動
                    var oldIndex = list.IndexOf(content);
                    var newIndex = index + ((oldIndex < index) ? -1 : 0) + ((dock ==Dock.Bottom) ? 1 : 0);
                    list.Move(oldIndex, newIndex);
                }
                else
                {
                    // 管理からいったん削除
                    _provider.Remove(content);

                    // GridLengthの補正
                    var gridLength = new GridLength(LayoutPanel.GridLength.Value * 0.5, GridUnitType.Star);
                    LayoutPanel.GridLength = gridLength;
                    content.GridLength = gridLength;

                    // 登録
                    var newIndex = index + ((dock == Dock.Bottom) ? 1 : 0);
                    list.Insert(newIndex, content);
                }
            }
        }


        private void LayoutPanelContainer_DragOver(object sender, DragEventArgs e)
        {
            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));

            if (content is null || content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var dock = GetLayoutDockFromPosY(e.GetPosition(this).Y, this.ActualHeight);
            switch (dock)
            {
                case Dock.Top:
                    _adorner.Start = new Point(0, 0);
                    _adorner.End = new Point(this.ActualWidth, this.ActualHeight * 0.5);
                    break;

                case Dock.Bottom:
                    _adorner.Start = new Point(0, this.ActualHeight * 0.5);
                    _adorner.End = new Point(this.ActualWidth, this.ActualHeight);
                    break;

                default:
                    throw new NotSupportedException();
            }

            _adorner.Attach();

            e.Effects = DragDropEffects.Move;
        }

        private void LayoutPanelContainer_DragLeave(object sender, DragEventArgs e)
        {
            _adorner.Detach();
        }

        private void LayoutDockPanel_DragEnter(object sender, DragEventArgs e)
        {
            _adorner.Attach();
        }

        private static Dock GetLayoutDockFromPosY(double y, double height)
        {
            return (y < height * 0.5) ? Dock.Top : Dock.Bottom;
        }

        #endregion DragDrop
    }
}
