// from https://github.com/takanemu/WPFDragAndDropSample

using System.Windows;


namespace NeeView.Windows
{
    public interface IHasDragGhost
    {
        FrameworkElement GetDragGhost();
    }
}
