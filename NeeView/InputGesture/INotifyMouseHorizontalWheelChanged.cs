using System.Windows.Input;

namespace NeeView
{
    public interface INotifyMouseHorizontalWheelChanged
    {
        event MouseWheelEventHandler MouseHorizontalWheelChanged;
    }
}
