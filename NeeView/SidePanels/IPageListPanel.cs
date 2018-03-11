using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ページのListBoxを持つ、サムネイル更新が必要なコントロールのインターフェイス
    /// </summary>
    public interface IPageListPanel
    {
        /// <summary>
        /// IHasPage 要素を持つ ListBox
        /// </summary>
        ListBox PageListBox { get; }

        /// <summary>
        /// サムネイルが表示されているか
        /// </summary>
        bool IsThumbnailVisibled { get; }
    }
}
