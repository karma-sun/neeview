using System.Windows;

namespace NeeView.Windows
{
    /// <summary>
    /// DragStartBehaviorのフック
    /// </summary>
    public interface IDragDropHook
    {
        /// <summary>
        /// DoDragDrop前に呼ばれる処理
        /// </summary>
        void BeginDragDrop(object sender, DependencyObject dragSource, object data, DragDropEffects allowedEffects);

        /// <summary>
        /// DoDragDrop後に呼ばれる処理
        /// </summary>
        void EndDragDrop(object sender, DependencyObject dragSource, object data, DragDropEffects allowedEffects);
    }
}
