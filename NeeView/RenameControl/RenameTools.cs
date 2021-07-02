using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public static class RenameTools
    {
        /// <summary>
        ///  要素の所属するウィンドウの RenameManager を取得する
        /// </summary>
        /// <param name="element">要素</param>
        /// <returns>RenameManager</returns>
        public static RenameManager GetRenameManager(UIElement element)
        {
            RenameManager renameMabager = null;

            var window = Window.GetWindow(element);
            if (window is IHasRenameManager hasRenameManager)
            {
                renameMabager = hasRenameManager.GetRenameManager();
            }

            Debug.Assert(renameMabager != null);
            return renameMabager;
        }

        /// <summary>
        /// リネームコントロールを閉じたあとにフォーカスを戻す
        /// </summary>
        /// <param name="element">フォーカス要素</param>
        /// <param name="isFocused">リネームコントロールにフォーカスがあるか</param>
        /// <returns>フォーカスされたか</returns>
        public static bool RestoreFocus(UIElement element, bool isFocused)
        {
            if (element is null) return false;
            if (!isFocused) return false;

            return FocusTools.FocusIfWindowActived(element);
        }
    }
}
