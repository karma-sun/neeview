using NeeView.Windows.Media;
using System.Windows;


namespace NeeView
{
    /// <summary>
    /// 基本的なAutoHideBehavior補足
    /// </summary>
    public class BasicAutoHideDescription : AutoHideDescription
    {
        private FrameworkElement _target;

        public BasicAutoHideDescription(FrameworkElement target)
        {
            _target = target;
        }

        public override bool IsVisibleLocked()
        {
            var targetElement = ContextMenuWatcher.TargetElement;
            if (targetElement != null)
            {
                return VisualTreeUtility.HasParentElement(targetElement, _target, true);
            }

            var dragElement = DragDropWatcher.DragElement;
            if (dragElement != null)
            {
                return VisualTreeUtility.HasParentElement(dragElement, _target, true);
            }

            var popupElement = PopupWatcher.PopupElement;
            if (popupElement != null)
            {
                return VisualTreeUtility.HasParentElement(popupElement, _target, true);
            }

            return false;
        }
    }
}
