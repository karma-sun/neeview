// from https://archive.codeplex.com/?p=uhimaniavwp
// Liceise: Ms-PL

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OpenSourceControls
{
    #region VirtualizingWrapPanel
    /// <summary>
    /// 子要素を仮想化する <see cref="System.Windows.Controls.WrapPanel"/>。
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region ItemSize

        #region ItemWidth

        /// <summary>
        /// <see cref="ItemWidth"/> 依存関係プロパティの識別子。
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                "ItemWidth",
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                ),
                new ValidateValueCallback(VirtualizingWrapPanel.IsWidthHeightValid)
            );

        /// <summary>
        /// VirtualizingWrapPanel 内に含まれているすべての項目の幅を
        /// 指定する値を取得、または設定する。
        /// </summary>
        [TypeConverter(typeof(LengthConverter)), Category("共通")]
        public double ItemWidth
        {
            get { return (double)this.GetValue(ItemWidthProperty); }
            set { this.SetValue(ItemWidthProperty, value); }
        }

        #endregion

        #region ItemHeight

        /// <summary>
        /// <see cref="ItemHeight"/> 依存関係プロパティの識別子。
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                "ItemHeight",
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                ),
                new ValidateValueCallback(VirtualizingWrapPanel.IsWidthHeightValid)
            );

        /// <summary>
        /// VirtualizingWrapPanel 内に含まれているすべての項目の高さを
        /// 指定する値を取得、または設定する。
        /// </summary>
        [TypeConverter(typeof(LengthConverter)), Category("共通")]
        public double ItemHeight
        {
            get { return (double)this.GetValue(ItemHeightProperty); }
            set { this.SetValue(ItemHeightProperty, value); }
        }

        #endregion

        #region IsWidthHeightValid
        /// <summary>
        /// <see cref="ItemWidth"/>, <see cref="ItemHeight"/> に設定された値が
        /// 有効かどうかを検証するコールバック。
        /// </summary>
        /// <param name="value">プロパティに設定された値。</param>
        /// <returns>値が有効な場合は true、無効な場合は false。</returns>
        private static bool IsWidthHeightValid(object value)
        {
            var d = (double)value;
            return double.IsNaN(d) || ((d >= 0) && !double.IsPositiveInfinity(d));
        }
        #endregion

        #endregion

        #region Orientation

        /// <summary>
        /// <see cref="Orientation"/> 依存関係プロパティの識別子。
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            WrapPanel.OrientationProperty.AddOwner(
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(VirtualizingWrapPanel.OnOrientationChanged)
                )
            );

        /// <summary>
        /// 子コンテンツが配置される方向を指定する値を取得、または設定する。
        /// </summary>
        [Category("共通")]
        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// <see cref="Orientation"/> 依存関係プロパティが変更されたときに呼び出されるコールバック。
        /// </summary>
        /// <param name="d">プロパティの値が変更された <see cref="System.Windows.DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベントデータ。</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as VirtualizingWrapPanel;
            panel.offset = default(Point);
            panel.InvalidateMeasure();
        }

        #endregion

        #region MeasureOverride, ArrangeOverride

        // 配置幅調整用スケール
        private double layoutScaleX = 1.0;

        /// <summary>
        /// 指定したインデックスのアイテムの位置およびサイズを記憶するディクショナリ。
        /// </summary>
        private Dictionary<int, Rect> containerLayouts = new Dictionary<int, Rect>();

        /// <summary>
        /// 子要素に必要なレイアウトのサイズを測定し、パネルのサイズを決定する。
        /// </summary>
        /// <param name="availableSize">子要素に与えることができる使用可能なサイズ。</param>
        /// <returns>レイアウト時にこのパネルが必要とするサイズ。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            ////var sw = Stopwatch.StartNew();

            this.containerLayouts.Clear();

            var isAutoWidth = double.IsNaN(this.ItemWidth);
            var isAutoHeight = double.IsNaN(this.ItemHeight);
            var childAvailable = new Size(isAutoWidth ? double.PositiveInfinity : this.ItemWidth, isAutoHeight ? double.PositiveInfinity : this.ItemHeight);
            var isHorizontal = this.Orientation == Orientation.Horizontal;

            var childrenCount = this.InternalChildren.Count;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (itemsControl != null)
                childrenCount = itemsControl.Items.Count;

            var generator = new ChildGenerator(this);

            var x = 0.0;
            var y = 0.0;
            var lineSize = default(Size);
            var maxSize = default(Size);
            var lastChildSize = default(Size);

            for (int i = 0; i < childrenCount; i++)
            {
                var childSize = this.ContainerSizeForIndex(i);

                // ビューポートとの交差判定用に仮サイズで x, y を調整
                var isWrapped = isHorizontal ?
                    lineSize.Width + childSize.Width > availableSize.Width :
                    lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    x = isHorizontal ? 0 : x + lineSize.Width;
                    y = isHorizontal ? y + lineSize.Height : 0;
                }

                // 子要素がビューポート内であれば子要素を生成しサイズを再計測
                var itemRect = new Rect(x, y, childSize.Width, childSize.Height);
                var viewportRect = new Rect(this.offset, availableSize);
                if (itemRect.IntersectsWith(viewportRect))
                {
                    var child = generator.GetOrCreateChild(i);
                    child.Measure(childAvailable);
                    childSize = this.ContainerSizeForIndex(i);
                }

                // 確定したサイズを記憶
                this.containerLayouts[i] = new Rect(x, y, childSize.Width, childSize.Height);
                lastChildSize = childSize;

                // lineSize, maxSize を計算
                isWrapped = isHorizontal ?
                    lineSize.Width + childSize.Width > availableSize.Width :
                    lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    maxSize.Width = isHorizontal ? Math.Max(lineSize.Width, maxSize.Width) : maxSize.Width + lineSize.Width;
                    maxSize.Height = isHorizontal ? maxSize.Height + lineSize.Height : Math.Max(lineSize.Height, maxSize.Height);
                    lineSize = childSize;

                    isWrapped = isHorizontal ?
                        childSize.Width > availableSize.Width :
                        childSize.Height > availableSize.Height;
                    if (isWrapped)
                    {
                        maxSize.Width = isHorizontal ? Math.Max(childSize.Width, maxSize.Width) : maxSize.Width + childSize.Width;
                        maxSize.Height = isHorizontal ? maxSize.Height + childSize.Height : Math.Max(childSize.Height, maxSize.Height);
                        lineSize = default(Size);
                    }
                }
                else
                {
                    lineSize.Width = isHorizontal ? lineSize.Width + childSize.Width : Math.Max(childSize.Width, lineSize.Width);
                    lineSize.Height = isHorizontal ? Math.Max(childSize.Height, lineSize.Height) : lineSize.Height + childSize.Height;
                }

                x = isHorizontal ? lineSize.Width : maxSize.Width;
                y = isHorizontal ? maxSize.Height : lineSize.Height;
            }

            maxSize.Width = isHorizontal ? Math.Max(lineSize.Width, maxSize.Width) : maxSize.Width + lineSize.Width;
            maxSize.Height = isHorizontal ? maxSize.Height + lineSize.Height : Math.Max(lineSize.Height, maxSize.Height);

            this.extent = maxSize;
            this.viewport = availableSize;

            generator.CleanupChildren();
            generator.Dispose();

            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateScrollInfo();

            // 配置幅調整用スケール
            // NOTE: セルサイズが同じ場合のみ正しい値になる
            layoutScaleX = lastChildSize.Width > 0.0 && availableSize.Width > lastChildSize.Width ? Math.Max(availableSize.Width / (lastChildSize.Width * Math.Floor(availableSize.Width / lastChildSize.Width)), 1.0) : 1.0;

            ////sw.Stop();
            ////Debug.WriteLine($"MeasureOverride: {sw.ElapsedMilliseconds}ms");

            return maxSize;
        }

        #region ChildGenerator
        /// <summary>
        /// <see cref="VirtualizingWrapPanel"/> のアイテムを管理する。
        /// </summary>
        private class ChildGenerator : IDisposable
        {
            #region fields

            /// <summary>
            /// アイテムを生成する対象の <see cref="VirtualizingWrapPanel"/>。
            /// </summary>
            private VirtualizingWrapPanel owner;

            /// <summary>
            /// <see cref="owner"/> の <see cref="System.Windows.Controls.ItemContainerGenerator"/>。
            /// </summary>
            private IItemContainerGenerator generator;

            /// <summary>
            /// <see cref="generator"/> の生成プロセスの有効期間を追跡するオブジェクト。
            /// </summary>
            private IDisposable generatorTracker;

            /// <summary>
            /// 表示範囲内にある最初の要素のインデックス。
            /// </summary>
            private int firstGeneratedIndex;

            /// <summary>
            /// 表示範囲内にある最後の要素のインデックス。
            /// </summary>
            private int lastGeneratedIndex;

            /// <summary>
            /// 次に生成される要素の <see cref="System.Windows.Controls.Panel.InternalChildren"/> 内のインデックス。
            /// </summary>
            private int currentGenerateIndex;

            /// <summary>
            /// Dispose用
            /// </summary>
            private bool disposedValue = false;

            #endregion

            #region _ctor

            /// <summary>
            /// <see cref="ChildGenerator"/> の新しいインスタンスを生成する。
            /// </summary>
            /// <param name="owner">アイテムを生成する対象の <see cref="VirtualizingWrapPanel"/>。</param>
            public ChildGenerator(VirtualizingWrapPanel owner)
            {
                this.owner = owner;

                // ItemContainerGenerator 取得前に InternalChildren にアクセスしないと null になる
                var childrenCount = owner.InternalChildren.Count;
                this.generator = owner.ItemContainerGenerator;
            }

            #endregion

            #region IDisposable Support

            /// <summary>
            /// アイテムの生成を終了する。
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    if (disposing)
                    {
                        // nop.
                    }

                    if (this.generatorTracker != null)
                    {
                        this.generatorTracker.Dispose();
                    }

                    this.disposedValue = true;
                }
            }

            /// <summary>
            /// <see cref="ChildGenerator"/> のインスタンスを破棄する。
            /// </summary>
            ~ChildGenerator()
            {
                Dispose(false);
            }

            /// <summary>
            /// アイテムの生成を終了する。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion

            #region GetOrCreateChild

            /// <summary>
            /// アイテムの生成を開始する。
            /// </summary>
            /// <param name="index">アイテムのインデックス。</param>
            private void BeginGenerate(int index)
            {
                this.firstGeneratedIndex = index;
                var startPos = this.generator.GeneratorPositionFromIndex(index);
                this.currentGenerateIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
                this.generatorTracker = this.generator.StartAt(startPos, GeneratorDirection.Forward, true);
            }

            /// <summary>
            /// 必要に応じてアイテムを生成し、指定したインデックスのアイテムを取得する。
            /// </summary>
            /// <param name="index">取得するアイテムのインデックス。</param>
            /// <returns>指定したインデックスのアイテム。</returns>
            public UIElement GetOrCreateChild(int index)
            {
                if (this.generator == null)
                    return this.owner.InternalChildren[index];

                if (this.generatorTracker == null)
                    this.BeginGenerate(index);

                bool newlyRealized;
                var child = this.generator.GenerateNext(out newlyRealized) as UIElement;
                if (newlyRealized)
                {
                    if (this.currentGenerateIndex >= this.owner.InternalChildren.Count)
                        this.owner.AddInternalChild(child);
                    else
                        this.owner.InsertInternalChild(this.currentGenerateIndex, child);

                    this.generator.PrepareItemContainer(child);
                }

                this.lastGeneratedIndex = index;
                this.currentGenerateIndex++;

                return child;
            }

            #endregion

            #region CleanupChildren
            /// <summary>
            /// 表示範囲外のアイテムを削除する。
            /// </summary>
            public void CleanupChildren()
            {
                if (this.generator == null)
                    return;

                var children = this.owner.InternalChildren;

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var childPos = new GeneratorPosition(i, 0);
                    var index = generator.IndexFromGeneratorPosition(childPos);
                    if (index < this.firstGeneratedIndex || index > this.lastGeneratedIndex)
                    {
                        this.generator.Remove(childPos, 1);
                        this.owner.RemoveInternalChildRange(i, 1);
                    }
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// 子要素を配置し、パネルのサイズを決定する。
        /// </summary>
        /// <param name="finalSize">パネル自体と子要素を配置するために使用する親の末尾の領域。</param>
        /// <returns>使用する実際のサイズ。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            ////var sw = Stopwatch.StartNew();

            foreach (UIElement child in this.InternalChildren)
            {
                var gen = this.ItemContainerGenerator as ItemContainerGenerator;
                var index = (gen != null) ? gen.IndexFromContainer(child) : this.InternalChildren.IndexOf(child);
                if (this.containerLayouts.ContainsKey(index))
                {
                    var layout = this.containerLayouts[index];
                    ////layout.Offset(this.offset.X * -1, this.offset.Y * -1);
                    var diffX = layout.X * (layoutScaleX - 1.0);
                    layout.Offset(this.offset.X * -1 + diffX, this.offset.Y * -1);
                    child.Arrange(layout);
                }
            }

            ////Debug.WriteLine($"{VerticalOffset}, {VerticalOffset + ViewportHeight}, {ExtentHeight}");
            // スクロールビューでの終端の表示位置をあわせる
            if (ExtentHeight > ViewportHeight && VerticalOffset + ViewportHeight > ExtentHeight)
            {
                this.SetVerticalOffset(VerticalOffset);
            }

            ////sw.Stop();
            ////Debug.WriteLine($"ArrangeOverride: {sw.ElapsedMilliseconds}ms");

            return finalSize;
        }

        #endregion

        #region ContainerSizeForIndex

        /// <summary>
        /// 直前にレイアウトした要素のサイズ。
        /// </summary>
        /// <remarks>
        /// <see cref="System.Windows.DataTemplate"/> 使用時、全要素のサイズが一致することを前提に、
        /// 要素のサイズの推定に使用する。
        /// </remarks>
        private Size prevSize = new Size(16, 16);

        /// <summary>
        /// 指定したインデックスに対するアイテムのサイズを、実際にアイテムを生成せずに推定する。
        /// </summary>
        /// <param name="index">アイテムのインデックス。</param>
        /// <returns>指定したインデックスに対するアイテムの推定サイズ。</returns>
        private Size ContainerSizeForIndex(int index)
        {
            var getSize = new Func<int, Size>(idx =>
            {
                UIElement item = null;
                var itemsOwner = ItemsControl.GetItemsOwner(this);
                var generator = this.ItemContainerGenerator as ItemContainerGenerator;

                if (itemsOwner == null || generator == null)
                {
                    // VirtualizingWrapPanel 単体で使用されている場合、自身のアイテムを返す
                    if (this.InternalChildren.Count > idx)
                        item = this.InternalChildren[idx];
                }
                else
                {
                    // generator がアイテムを未生成の場合、Items が使えればそちらを使う
                    if (generator.ContainerFromIndex(idx) != null)
                        item = generator.ContainerFromIndex(idx) as UIElement;
                    else if (itemsOwner.Items.Count > idx)
                        item = itemsOwner.Items[idx] as UIElement;
                }

                if (item != null)
                {
                    // アイテムのサイズが測定済みであればそのサイズを返す
                    if (item.IsMeasureValid)
                        return item.DesiredSize;

                    // アイテムのサイズが未測定の場合、推奨値を使う
                    var i = item as FrameworkElement;
                    if (i != null)
                        return new Size(i.Width, i.Height); // NOTE: NaNになることがある
                }

                // 前回の測定値があればそちらを使う
                if (this.containerLayouts.ContainsKey(idx))
                    return this.containerLayouts[idx].Size;

                // 有効なサイズが取得できなかった場合、直前のアイテムのサイズを返す
                return this.prevSize;
            });

            var size = getSize(index);

            // 有効なサイズが取得できなかった場合、直前のアイテムのサイズを返す
            if (double.IsNaN(size.Width) || double.IsNaN(size.Height))
            {
                return this.prevSize;
            }

            // ItemWidth, ItemHeight が指定されていれば調整する
            if (!double.IsNaN(this.ItemWidth))
                size.Width = this.ItemWidth;
            if (!double.IsNaN(this.ItemHeight))
                size.Height = this.ItemHeight;


            return this.prevSize = size;
        }

        #endregion

        #region OnItemsChanged
        /// <summary>
        /// このパネルの <see cref="System.Windows.Controls.ItemsControl"/> に関連付けられている
        /// <see cref="System.Windows.Controls.ItemsControl.Items"/> コレクションが変更されたときに
        /// 呼び出されるコールバック。
        /// </summary>
        /// <param name="sender">イベントを発生させた <see cref="System.Object"/></param>
        /// <param name="args">イベントデータ。</param>
        /// <remarks>
        /// <see cref="System.Windows.Controls.ItemsControl.Items"/> が変更された際
        /// <see cref="System.Windows.Controls.Panel.InternalChildren"/> にも反映する。
        /// </remarks>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        }
        #endregion

        #region BringIndexIntoView
        protected override void BringIndexIntoView(int index)
        {
            if (this.containerLayouts.ContainsKey(index))
            {
                var layout = this.containerLayouts[index];

                var bottom = layout.Y + layout.Height;
                if (bottom < VerticalOffset + ViewportHeight)
                {
                    SetVerticalOffset(bottom - ViewportHeight);
                }

                var top = layout.Y;
                if (VerticalOffset < top)
                {
                    SetVerticalOffset(top);
                }
            }
        }
        #endregion

        #region IScrollInfo Members

        #region Extent

        /// <summary>
        /// エクステントのサイズ。
        /// </summary>
        private Size extent = default(Size);

        /// <summary>
        /// エクステントの縦幅を取得する。
        /// </summary>
        public double ExtentHeight
        {
            get { return this.extent.Height; }
        }

        /// <summary>
        /// エクステントの横幅を取得する。
        /// </summary>
        public double ExtentWidth
        {
            get { return this.extent.Width; }
        }

        #endregion Extent

        #region Viewport

        /// <summary>
        /// ビューポートのサイズ。
        /// </summary>
        private Size viewport = default(Size);

        /// <summary>
        /// このコンテンツに対するビューポートの縦幅を取得する。
        /// </summary>
        public double ViewportHeight
        {
            get { return this.viewport.Height; }
        }

        /// <summary>
        /// このコンテンツに対するビューポートの横幅を取得する。
        /// </summary>
        public double ViewportWidth
        {
            get { return this.viewport.Width; }
        }

        #endregion

        #region Offset

        /// <summary>
        /// スクロールしたコンテンツのオフセット。
        /// </summary>
        private Point offset;

        /// <summary>
        /// スクロールしたコンテンツの水平オフセットを取得する。
        /// </summary>
        public double HorizontalOffset
        {
            get { return this.offset.X; }
        }

        /// <summary>
        /// スクロールしたコンテンツの垂直オフセットを取得する。
        /// </summary>
        public double VerticalOffset
        {
            get { return this.offset.Y; }
        }

        #endregion

        #region ScrollOwner
        /// <summary>
        /// スクロール動作を制御する <see cref="System.Windows.Controls.ScrollViewer"/> 要素を
        /// 取得、または設定する。
        /// </summary>
        public ScrollViewer ScrollOwner { get; set; }
        #endregion

        #region CanHorizontallyScroll
        /// <summary>
        /// 水平軸のスクロールが可能かどうかを示す値を取得、または設定する。
        /// </summary>
        public bool CanHorizontallyScroll { get; set; }
        #endregion

        #region CanVerticallyScroll
        /// <summary>
        /// 垂直軸のスクロールが可能かどうかを示す値を取得、または設定する。
        /// </summary>
        public bool CanVerticallyScroll { get; set; }
        #endregion

        #region LineUp
        /// <summary>
        /// コンテンツ内を 1 論理単位ずつ上にスクロールする。
        /// </summary>
        public void LineUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - SystemParameters.ScrollHeight);
        }
        #endregion

        #region LineDown
        /// <summary>
        /// コンテンツ内を 1 論理単位ずつ下にスクロールする。
        /// </summary>
        public void LineDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + SystemParameters.ScrollHeight);
        }
        #endregion

        #region LineLeft
        /// <summary>
        /// コンテンツ内を 1 論理単位ずつ左にスクロールする。
        /// </summary>
        public void LineLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - SystemParameters.ScrollWidth);
        }
        #endregion

        #region LineRight
        /// <summary>
        /// コンテンツ内を 1 論理単位ずつ右にスクロールする。
        /// </summary>
        public void LineRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + SystemParameters.ScrollWidth);
        }
        #endregion

        #region PageUp
        /// <summary>
        /// コンテンツ内を 1 ページずつ上にスクロールする。
        /// </summary>
        public void PageUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this.viewport.Height);
        }
        #endregion

        #region PageDown
        /// <summary>
        /// コンテンツ内を 1 ページずつ下にスクロールする。
        /// </summary>
        public void PageDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this.viewport.Height);
        }
        #endregion

        #region PageLeft
        /// <summary>
        /// コンテンツ内を 1 ページずつ左にスクロールする。
        /// </summary>
        public void PageLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - this.viewport.Width);
        }
        #endregion

        #region PageRight
        /// <summary>
        /// コンテンツ内を 1 ページずつ右にスクロールする。
        /// </summary>
        public void PageRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + this.viewport.Width);
        }
        #endregion

        #region MouseWheelUp
        /// <summary>
        /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を上にスクロールする。
        /// </summary>
        public void MouseWheelUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines);
        }
        #endregion

        #region MouseWheelDown
        /// <summary>
        /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を下にスクロールする。
        /// </summary>
        public void MouseWheelDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines);
        }
        #endregion

        #region MouseWheelLeft
        /// <summary>
        /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を左にスクロールする。
        /// </summary>
        public void MouseWheelLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines);
        }
        #endregion

        #region MouseWheelRight
        /// <summary>
        /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を右にスクロールする。
        /// </summary>
        public void MouseWheelRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines);
        }
        #endregion

        #region MakeVisible
        /// <summary>
        /// <see cref="System.Windows.Media.Visual"/> オブジェクトの座標空間が表示されるまで、
        /// コンテンツを強制的にスクロールする。
        /// </summary>
        /// <param name="visual">表示可能になる <see cref="System.Windows.Media.Visual"/>。</param>
        /// <param name="rectangle">表示する座標空間を識別する外接する四角形。</param>
        /// <returns>表示される <see cref="System.Windows.Rect"/>。</returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var idx = this.InternalChildren.IndexOf(visual as UIElement);

            var generator = this.ItemContainerGenerator as IItemContainerGenerator;
            if (generator != null)
            {
                var pos = new GeneratorPosition(idx, 0);
                idx = generator.IndexFromGeneratorPosition(pos);
            }

            if (idx < 0)
                return Rect.Empty;

            if (!this.containerLayouts.ContainsKey(idx))
                return Rect.Empty;

            var layout = this.containerLayouts[idx];

            if (this.HorizontalOffset + this.ViewportWidth < layout.X + layout.Width)
                this.SetHorizontalOffset(layout.X + layout.Width - this.ViewportWidth);
            if (layout.X < this.HorizontalOffset)
                this.SetHorizontalOffset(layout.X);

            if (this.VerticalOffset + this.ViewportHeight < layout.Y + layout.Height)
                this.SetVerticalOffset(layout.Y + layout.Height - this.ViewportHeight);
            if (layout.Y < this.VerticalOffset)
                this.SetVerticalOffset(layout.Y);

            layout.Width = Math.Min(this.ViewportWidth, layout.Width);
            layout.Height = Math.Min(this.ViewportHeight, layout.Height);

            return layout;
        }
        #endregion

        #region SetHorizontalOffset
        /// <summary>
        /// 水平オフセットの値を設定する。
        /// </summary>
        /// <param name="offset">包含するビューポートからのコンテンツの水平方向オフセットの程度。</param>
        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || this.ViewportWidth >= this.ExtentWidth)
            {
                offset = 0;
            }
            else
            {
                if (offset + this.ViewportWidth >= this.ExtentWidth)
                    offset = this.ExtentWidth - this.ViewportWidth;
            }

            this.offset.X = offset;

            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateScrollInfo();

            this.InvalidateMeasure();
        }
        #endregion

        #region SetVerticalOffset
        /// <summary>
        /// 垂直オフセットの値を設定する。
        /// </summary>
        /// <param name="offset">包含するビューポートからの垂直方向オフセットの程度。</param>
        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || this.ViewportHeight >= this.ExtentHeight)
            {
                offset = 0;
            }
            else
            {
                if (offset + this.ViewportHeight >= this.ExtentHeight)
                    offset = this.ExtentHeight - this.ViewportHeight;
            }

            this.offset.Y = offset;

            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateScrollInfo();

            this.InvalidateMeasure();
        }
        #endregion

        #endregion
    }
    #endregion
}
