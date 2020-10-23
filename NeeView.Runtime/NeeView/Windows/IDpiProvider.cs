using System.Windows;

namespace NeeView.Runtime
{
    public interface IDpiProvider
    {
        DpiScale Dpi { get; }
    }

}
