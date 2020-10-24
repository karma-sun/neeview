using System.Windows;

namespace NeeView.Windows
{
    public interface IDpiProvider
    {
        DpiScale Dpi { get; }
    }

}
