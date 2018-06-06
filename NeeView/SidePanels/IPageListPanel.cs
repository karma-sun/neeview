using System.Collections.Generic;
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
        ListBox PageCollectionListBox { get; }

        /// <summary>
        /// サムネイルが表示されているか
        /// </summary>
        bool IsThumbnailVisibled { get; }

        /// <summary>
        /// ページの収集
        /// </summary>
        IEnumerable<IHasPage> CollectPageList(IEnumerable<object> objs);
    }
}
