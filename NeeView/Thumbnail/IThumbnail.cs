using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// サムネイル用インターフェイス
    /// </summary>
    public interface IThumbnail
    {
        ImageSource ImageSource { get; }
        double Width { get; }
        double Height { get; }

        bool IsUniqueImage { get; }
        bool IsNormalImage { get; }
        Brush Background { get; }
    }
}
