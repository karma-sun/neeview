using System.Windows;

namespace NeeView.Windows
{
    public interface IDpiScaleProvider
    {
        DpiScale GetDpiScale();
    }

}
