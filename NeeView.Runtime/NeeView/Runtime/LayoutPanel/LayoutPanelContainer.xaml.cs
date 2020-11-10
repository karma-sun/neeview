using NeeLaboratory.Windows.Input;
using NeeView.Windows;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
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

namespace NeeView.Runtime.LayoutPanel
{
    /// <summary>
    /// LayoutPanelContainer.xaml の相互作用ロジック
    /// </summary>
    public partial class LayoutPanelContainer : UserControl
    {
        private LayoutPanelContainerAdorner _adorner;
        private LayoutPanelManager _manager;


        public LayoutPanelContainer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public LayoutPanelContainer(LayoutPanelManager manager, LayoutPanel layoutPanel) : this()
        {
            _manager = manager;
            LayoutPanel = layoutPanel;

            this.FloatingMenuItem.Header = manager.Resources["Floating"];
            this.DockingMenuItem.Header = manager.Resources["Docking"];
            this.CloseMenuItem.Header = manager.Resources["Close"];

            this.Loaded += LayoutPanelContainer_Loaded;
        }


        public LayoutPanel LayoutPanel
        {
            get { return (LayoutPanel)GetValue(LayoutPanelProperty); }
            set { SetValue(LayoutPanelProperty, value); }
        }

        public static readonly DependencyProperty LayoutPanelProperty =
            DependencyProperty.Register("LayoutPanel", typeof(LayoutPanel), typeof(LayoutPanelContainer), new PropertyMetadata(null));



        public IDragDropDescriptor DragDropDescriptor
        {
            get { return (IDragDropDescriptor)GetValue(DescriptorProperty); }
            set { SetValue(DescriptorProperty, value); }
        }

        public static readonly DependencyProperty DescriptorProperty =
            DependencyProperty.Register("Descriptor", typeof(IDragDropDescriptor), typeof(LayoutPanelContainer), new PropertyMetadata(null));





        private void LayoutPanelContainer_Loaded(object sender, RoutedEventArgs e)
        {
            _adorner = _adorner ?? new LayoutPanelContainerAdorner(this);

            InitializeDrop();
            //this.PreviewDragOver += LayoutPanelContainer_PreviewDragOver;
            this.PreviewDragOver += LayoutPanelContainer_DragOver;
            this.PreviewDragEnter += LayoutPanelContainer_DragEnter;
            //this.PreviewDragLeave += LayoutPanelContainer_PreviewDragLeave;
            this.PreviewDragLeave += LayoutPanelContainer_DragLeave;
            this.PreviewDrop += LayoutPanelContainer_Drop;
            this.AllowDrop = true;
            //this.PreviewMouseMove += LayoutPanelContainer_PreviewMouseMove;
        }


        private void LayoutPanelContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ///Debug.WriteLine($"PMV: {e.GetPosition(this)}");
        }

        public void Snap()
        {
            // TODO: WindowPlacement情報破棄 はダメ。復元できるように。
            LayoutPanel.WindowPlacement = WindowPlacement.None;
        }


        private void OpenWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            var point = this.PointToScreen(new Point(0.0, 0.0));
            _manager.OpenWindow(LayoutPanel, new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)ActualWidth, (int)ActualHeight));
        }

        private void ClosePanelCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _manager.StandAlone(LayoutPanel);
            _manager.Close(LayoutPanel);
        }

        #region DragDrop

        private void DragBegin(object sender, DragStartEventArgs e)
        {
            _manager.RaiseDragBegin();
        }

        private void DragEnd(object sender, EventArgs e)
        {
            _manager.RaiseDragEnd();
        }

        //private DelayValue<bool> _isGhostVisible;

        private void InitializeDrop()
        {
            //_isGhostVisible = new DelayValue<bool>(false);
            //_isGhostVisible.ValueChanged += IsGhostVisible_ValueChanged;
        }

#if false
        private void IsGhostVisible_ValueChanged(object sender, EventArgs e)
        {
            if (_isGhostVisible.Value)
            {
                _adorner.Attach();
            }
            else
            {
                _adorner.Detach();
            }
        }
#endif

        private void LayoutPanelContainer_Drop(object sender, DragEventArgs e)
        {
            _adorner.Detach();
            ////isGhostVisible.SetValue(false, 0.0);

            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null) return;

            e.Handled = true;

            if (content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

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
                    var newIndex = index + ((oldIndex < index) ? -1 : 0) + ((dock == Dock.Bottom) ? 1 : 0);
                    list.Move(oldIndex, newIndex);
                }
                else
                {
                    // 管理からいったん削除
                    _manager.Remove(content);

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
            if (content is null) return;

            e.Handled = true;

            if (content == this.LayoutPanel)
            {
                e.Effects = DragDropEffects.None;
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
            ////_isGhostVisible.SetValue(true);

            e.Effects = DragDropEffects.Move;

            __DumpDragEvent(sender, e);
        }



        private void LayoutPanelContainer_PreviewDragOver(object sender, DragEventArgs e)
        {
            //__DumpDragEvent(sender, e);
        }

        private void LayoutPanelContainer_PreviewDragLeave(object sender, DragEventArgs e)
        {
            //__DumpDragEvent(sender, e);
        }

        private void LayoutPanelContainer_DragLeave(object sender, DragEventArgs e)
        {
            var content = (LayoutPanel)e.Data.GetData(typeof(LayoutPanel));
            if (content is null) return;

            __DumpDragEvent(sender, e);

#if false
            var element = (FrameworkElement)e.OriginalSource;

            while (element != null)
            {
                Debug.WriteLine($":: {element}");
                element = element.Parent as FrameworkElement;
            }
#endif

#if false
            var pos = e.GetPosition(this);
            Debug.WriteLine($"{pos}");
            if (pos.X < 0 || pos.Y < 0 || pos.X >= this.ActualWidth || pos.Y >= this.ActualHeight)
            {
                Debug.WriteLine($"{pos}: Detach");
            }
#endif
            _adorner.Detach();
            ////_isGhostVisible.SetValue(false, 0.0);
            e.Handled = true;
        }

        private void LayoutPanelContainer_DragEnter(object sender, DragEventArgs e)
        {
            LayoutPanelContainer_DragOver(sender, e);

            /*
            __DumpDragEvent(sender, e);
            _isGhostVisible.SetValue(true);
            */
        }

        private void __DumpDragEvent(object sender, DragEventArgs e)
        {
#if false
            const int callerFrameIndex = 1;
            System.Diagnostics.StackFrame callerFrame = new System.Diagnostics.StackFrame(callerFrameIndex);
            System.Reflection.MethodBase callerMethod = callerFrame.GetMethod();

            Debug.WriteLine($"{callerMethod.Name}: {e.OriginalSource},{e.OriginalSource.GetHashCode()}");
#endif
        }

        private static Dock GetLayoutDockFromPosY(double y, double height)
        {
            return (y < height * 0.5) ? Dock.Top : Dock.Bottom;
        }

        #endregion DragDrop

    }

    public interface IDragDropDescriptor
    {
        void DragBegin();
        void DragEnd();
    }
}
