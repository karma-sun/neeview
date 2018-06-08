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
    /// ViewModelとBehaviorの橋渡し処理
    /// </summary>
    public sealed class DragStartDescription
    {
        /// <summary>
        /// ドラッグ開始イベント
        /// </summary>
        public event EventHandler<MouseEventArgs> DragStart;

        /// <summary>
        /// ドラッグ終了イベント
        /// </summary>
        public event EventHandler<MouseEventArgs> DragEnd;

        /// <summary>
        /// ドラッグ開始処理呼び出し
        /// </summary>
        public void OnDragStart(object sender, MouseEventArgs e)
        {
            this.DragStart?.Invoke(sender, e);
        }

        /// <summary>
        /// ドラッグ終了処理呼び出し
        /// </summary>
        public void OnDragEnd(object sender, MouseEventArgs e)
        {
            this.DragEnd?.Invoke(sender, e);
        }
    }


    /// <summary>
    /// ドラッグ対象オブジェクト用ビヘイビア
    /// <see cref="http://b.starwing.net/?p=131"/>
    /// </summary>
    public class DragStartBehavior : Behavior<FrameworkElement>
    {
        private Point _origin;
        private bool _isButtonDown;
        private IInputElement _dragItem;
        private Point _dragStartPos;
        private DragAdorner _dragGhost;

        /// <summary>
        /// ドラッグアンドドロップ操作の効果
        /// </summary>
        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects), typeof(DragStartBehavior), new UIPropertyMetadata(DragDropEffects.All));

        /// <summary>
        /// ドラッグされるデータ
        /// </summary>
        public object DragDropData
        {
            get { return GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグされるデータを識別する文字列(任意)
        /// </summary>
        public string DragDropFormat
        {
            get { return (string)GetValue(DragDropFormatProperty); }
            set { SetValue(DragDropFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDropFormatProperty =
            DependencyProperty.Register("DragDropFormat", typeof(string), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ先
        /// </summary>
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(DragStartBehavior), new PropertyMetadata(null));


        /// <summary>
        /// ドラッグ有効
        /// </summary>
        public bool IsDragEnable
        {
            get { return (bool)GetValue(IsDragEnableProperty); }
            set { SetValue(IsDragEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool), typeof(DragStartBehavior), new UIPropertyMetadata(true));


        /// <summary>
        /// ドラッグイベント処理セット
        /// </summary>
        public DragStartDescription Description
        {
            get { return (DragStartDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DragStartDescription), typeof(DragStartBehavior), new PropertyMetadata(null));



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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;

            if (sender is IInputElement)
            {
                // マウスダウンされたアイテムを記憶
                _dragItem = sender as IInputElement;
                // マウスダウン時の座標を取得
                _dragStartPos = e.GetPosition(_dragItem);
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown)
            {
                return;
            }
            var point = e.GetPosition(this.AssociatedObject);

            if (CheckDistance(point, _origin) && _dragGhost == null)
            {
                // アクティブWindowの直下のContentに対して、Adornerを付加する
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                this.Description?.OnDragStart(sender, e);

                if (window != null)
                {
                    var root = window.Content as UIElement;
                    var layer = AdornerLayer.GetAdornerLayer(root);
                    _dragGhost = new DragAdorner(root, (UIElement)sender, 0.5, _dragStartPos);
                    layer.Add(_dragGhost);
                    var dataObject = this.DragDropFormat != null ? new DataObject(this.DragDropFormat, this.DragDropData) : this.DragDropData;
                    DragDrop.DoDragDrop(this.AssociatedObject, dataObject, this.AllowedEffects);
                    layer.Remove(_dragGhost);
                }
                else
                {
                    DragDrop.DoDragDrop(this.AssociatedObject, this.DragDropData, this.AllowedEffects);
                }
                _isButtonDown = false;
                e.Handled = true;
                _dragGhost = null;
                _dragItem = null;

                this.Description?.OnDragEnd(sender, e);
            }
        }

        /// <summary>
        /// マウスボタンリリース処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        /// <summary>
        /// 座標検査
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// ゴーストの移動処理
        /// Window全体に、ゴーストが移動するタイプのドラッグを想定している
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (_dragGhost != null)
            {
                var point = CursorInfo.GetNowPosition((Visual)_dragItem);
                _dragGhost.LeftOffset = point.X;
                _dragGhost.TopOffset = point.Y;
            }

            if (this.Target != null)
            {
                AutoScroll(this.Target, e);
            }
        }

        /// <summary>
        /// ドラッグがターゲットの外にある時に自動スクロールさせる
        /// </summary>
        /// <param name="container"></param>
        /// <param name="e"></param>
        private void AutoScroll(FrameworkElement container, QueryContinueDragEventArgs e)
        {
            ScrollViewer scrollViewer = VisualTreeUtility.FindVisualChild<ScrollViewer>(container);
            if (scrollViewer == null)
            {
                return;
            }

            var root = (FrameworkElement)Window.GetWindow(container).Content;
            var cursor = CursorInfo.GetNowPosition(root);

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
}
