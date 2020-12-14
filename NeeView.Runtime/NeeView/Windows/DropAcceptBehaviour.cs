// from https://github.com/takanemu/WPFDragAndDropSample

using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Windows
{
    /// <summary>
    /// ViewModelとBehaviorの橋渡し処理
    /// </summary>
    public sealed class DropAcceptDescription
    {
        /// <summary>
        /// ドラッグオーバーイベント
        /// </summary>
        public event EventHandler<DragEventArgs> DragOver;

        /// <summary>
        /// ドロップイベント
        /// </summary>
        public event EventHandler<DragEventArgs> DragDrop;

        /// <summary>
        /// ドラッグオーバー処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDragOver(DragEventArgs dragEventArgs)
        {
            this.DragOver?.Invoke(this, dragEventArgs);
        }

        /// <summary>
        /// ドロップ処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDrop(DragEventArgs dragEventArgs)
        {
            this.DragDrop?.Invoke(this, dragEventArgs);
        }
    }

    
    /// <summary>
    /// ドロップ対象オブジェクト用ビヘイビア
    /// </summary>
    public class DragAcceptBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// ドロップイベント処理セット
        /// </summary>
        public DropAcceptDescription Description
        {
            get { return (DropAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDropFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DropAcceptDescription), typeof(DragAcceptBehavior), new PropertyMetadata(null));



        /// <summary>
        /// 初期化
        /// </summary>
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragEnter += DragOverHandler;
            this.AssociatedObject.PreviewDragOver += DragOverHandler;
            this.AssociatedObject.Drop += DropHandler;
            base.OnAttached();
        }

        /// <summary>
        /// 後始末
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragEnter -= DragOverHandler;
            this.AssociatedObject.PreviewDragOver -= DragOverHandler;
            this.AssociatedObject.Drop -= DropHandler;
            base.OnDetaching();
        }

        /// <summary>
        /// ドラッグオーバー処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragOverHandler(object sender, DragEventArgs e)
        {
            var desc = this.Description;
            if (desc == null)
            {
                return;
            }
            desc.OnDragOver(e);
        }

        /// <summary>
        /// ドロップ処理
        /// </summary>
        private void DropHandler(object sender, DragEventArgs e)
        {
            var desc = this.Description;
            if (desc == null)
            {
                return;
            }
            desc.OnDrop(e);
        }
    }
}

