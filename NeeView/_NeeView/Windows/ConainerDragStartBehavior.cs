// from https://github.com/takanemu/WPFDragAndDropSample

using NeeLaboratory.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;


namespace NeeView.Windows
{
    /// <summary>
    /// TreeViewやListBoxに特化した<see cref="DragStartBehavior"/>
    /// </summary>
    public class ConainerDragStartBehavior<TItem> : Behavior<FrameworkElement>
        where TItem : UIElement
    {
        private Point _origin;
        private bool _isButtonDown;
        private TItem _dragItem;
        private UIElement _adornerVisual;
        private Point _dragStartPos;
        private DragAdorner _dragGhost;


        /// <summary>
        /// ドラッグ開始イベント
        /// </summary>
        public event EventHandler<DragStartEventArgs> DragBegin;

        /// <summary>
        /// ドラッグ終了イベント
        /// </summary>
        public event EventHandler DragEnd;


        /// <summary>
        /// ドラッグアンドドロップ操作の効果
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(ConainerDragStartBehavior<TItem>), new UIPropertyMetadata(DragDropEffects.All));

        /// <summary>
        /// ドラッグされるデータを識別する文字列(任意)
        /// </summary>
        public string DragDropFormat
        {
            get { return (string)GetValue(DragDropFormatProperty); }
            set { SetValue(DragDropFormatProperty, value); }
        }

        public static readonly DependencyProperty DragDropFormatProperty =
            DependencyProperty.Register("DragDropFormat", typeof(string), typeof(ConainerDragStartBehavior<TItem>), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ有効
        /// </summary>
        public bool IsDragEnable
        {
            get { return (bool)GetValue(IsDragEnableProperty); }
            set { SetValue(IsDragEnableProperty, value); }
        }

        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool), typeof(ConainerDragStartBehavior<TItem>), new UIPropertyMetadata(true));


        /// <summary>
        /// 初期化
        /// </summary>
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove += PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp += PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag += QueryContinueDragHandler;
            base.OnAttached();
        }

        /// <summary>
        /// 後始末
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove -= PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp -= PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag -= QueryContinueDragHandler;
            base.OnDetaching();
        }

        /// <summary>
        /// マウスボタン押下処理
        /// </summary>
        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }

            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;

            if (sender is UIElement element)
            {
                var hitObject = element.InputHitTest(e.GetPosition(element)) as DependencyObject;
                _dragItem = hitObject is TItem item ? item : VisualTreeUtility.GetParentElement<TItem>(hitObject);

                if (_dragItem != null)
                {
                    _adornerVisual = GetAdornerVisual(_dragItem) ?? _dragItem;
                    _dragStartPos = e.GetPosition(_adornerVisual);
                }
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown || _dragItem == null)
            {
                return;
            }
            var point = e.GetPosition(this.AssociatedObject);

            if (CheckDistance(point, _origin) && _dragGhost == null)
            {
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                var dataObject = this.DragDropFormat != null ? new DataObject(this.DragDropFormat, _dragItem) : new DataObject(_dragItem);
                var args = new DragStartEventArgs(dataObject, this.AllowedEffects, e);

                DragBegin?.Invoke(sender, args);
                if (args.Cancel)
                {
                    return;
                }

                if (window != null)
                {
                    var root = window.Content as UIElement;
                    var layer = AdornerLayer.GetAdornerLayer(root);
                    _dragGhost = new DragAdorner(root, _adornerVisual, 0.5, _dragStartPos);
                    layer.Add(_dragGhost);
                    DragDrop.DoDragDrop(this.AssociatedObject, args.Data, args.AllowedEffects);
                    layer.Remove(_dragGhost);
                }
                else
                {
                    DragDrop.DoDragDrop(this.AssociatedObject, args.Data, args.AllowedEffects);
                }
                _isButtonDown = false;
                e.Handled = true;
                _dragGhost = null;
                _dragItem = null;

                DragEnd?.Invoke(sender, null);
            }
        }

        /// <summary>
        /// マウスボタンリリース処理
        /// </summary>
        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }


        public virtual UIElement GetAdornerVisual(TItem dragItem)
        {
            return dragItem;
        }


        /// <summary>
        /// 座標検査
        /// </summary>
        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// ゴーストの移動処理
        /// Window全体に、ゴーストが移動するタイプのドラッグを想定している
        /// </summary>
        private void QueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }

            try
            {
                if (_dragGhost != null)
                {
                    var point = CursorInfo.GetNowPosition(_dragItem);
                    if (double.IsNaN(point.X))
                    {
                        Debug.WriteLine("_dragItem does not exist in virual tree.");
                        e.Action = System.Windows.DragAction.Cancel;
                        e.Handled = true;
                        return;
                    }
                    _dragGhost.LeftOffset = point.X;
                    _dragGhost.TopOffset = point.Y;
                }

                if (AllowedEffects.HasFlag(DragDropEffects.Scroll))
                {
                    AutoScroll(sender, e);
                }

                //e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// ドラッグがターゲットの外にある時に自動スクロールさせる.
        /// HACK: 自動スクロールは受け入れ側で実装すべき
        /// </summary>
        private void AutoScroll(object sender, QueryContinueDragEventArgs e)
        {
            var container = sender as FrameworkElement;
            if (container == null)
            {
                return;
            }

            ScrollViewer scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(container);
            if (scrollViewer == null)
            {
                return;
            }

            var root = (FrameworkElement)Window.GetWindow(container)?.Content;
            if (root == null)
            {
                // container does not exist in virual tree.
                return;
            }
            var cursor = CursorInfo.GetNowPosition(root);
            if (double.IsNaN(cursor.X))
            {
                return;
            }

            var point = root.TranslatePoint(cursor, container);
            double offset = _dragGhost != null ? _dragGhost.ActualHeight * 0.5 : 20.0;

            if (point.Y < 0.0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
            }
            else if (point.Y > container.ActualHeight)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
            }
        }
    }


    /// <summary>
    /// TreeView DragDropStartBehavior
    /// </summary>
    public class TreeViewDragDropStartBehavior : ConainerDragStartBehavior<TreeViewItem>
    {
        public override UIElement GetAdornerVisual(TreeViewItem dragItem)
        {
            return VisualTreeUtility.FindVisualChild<ContentPresenter>(dragItem);
        }
    }

    /// <summary>
    /// ListBox DragDropStartBehavior
    /// </summary>
    public class ListBoxDragDropStartBehavior : ConainerDragStartBehavior<ListBoxItem>
    {
        public override UIElement GetAdornerVisual(ListBoxItem dragItem)
        {
            return VisualTreeUtility.FindVisualChild<ContentPresenter>(dragItem);
        }
    }
}
