// from https://github.com/takanemu/WPFDragAndDropSample

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Interactivity;

namespace NeeView.Windows
{
    /// <summary>
    /// ViewModelとBehaviorの橋渡し処理
    /// <see cref="http://b.starwing.net/?p=131"/>
    /// </summary>
    public sealed class DropAcceptDescription
    {
        /// <summary>
        /// ドラッグオーバーイベント
        /// </summary>
        public event Action<DragEventArgs> DragOver;

        /// <summary>
        /// ドロップイベント
        /// </summary>
        public event Action<DragEventArgs> DragDrop;

        /// <summary>
        /// ドラッグオーバー処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDragOver(DragEventArgs dragEventArgs)
        {
            this.DragOver?.Invoke(dragEventArgs);
        }

        /// <summary>
        /// ドロップ処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDrop(DragEventArgs dragEventArgs)
        {
            this.DragDrop?.Invoke(dragEventArgs);
        }
    }

    
    /// <summary>
    /// ドロップ対象オブジェクト用ビヘイビア
    /// <see cref="http://b.starwing.net/?p=131"/>
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
            this.AssociatedObject.PreviewDragOver += DragOverHandler;
            this.AssociatedObject.PreviewDrop += DropHandler;
            base.OnAttached();
        }

        /// <summary>
        /// 後始末
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragOver -= DragOverHandler;
            this.AssociatedObject.PreviewDrop -= DropHandler;
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
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropHandler(object sender, DragEventArgs e)
        {
            var desc = this.Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDrop(e);
            e.Handled = true;
        }
    }
}

