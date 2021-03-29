using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public static class ThemeTools
    {
        /// <summary>
        /// TextBox の EditContextMenu にスタイルを適用する (未使用)
        /// </summary>
        /// <remarks>
        /// from https://stackoverflow.com/questions/30940939/wpf-default-textbox-contextmenu-styling
        /// </remarks>
        /// <param name="element">設定するリソースのエレメント</param>
        public static void InitializeEditorContextMenuStyle(FrameworkElement element)
        {
            var presentationFrameworkAssembly = typeof(Application).Assembly;
            var contextMenuStyle = element.FindResource(typeof(ContextMenu)) as Style;
            var editorContextMenuType = Type.GetType("System.Windows.Documents.TextEditorContextMenu+EditorContextMenu, " + presentationFrameworkAssembly);

            if (editorContextMenuType != null)
            {
                var editorContextMenuStyle = new Style(editorContextMenuType, contextMenuStyle);
                element.Resources.Add(editorContextMenuType, editorContextMenuStyle);
            }

            var menuItemStyle = element.FindResource(typeof(MenuItem)) as Style;
            var editorMenuItemType = Type.GetType("System.Windows.Documents.TextEditorContextMenu+EditorMenuItem, " + presentationFrameworkAssembly);

            if (editorMenuItemType != null)
            {
                var editorContextMenuStyle = new Style(editorMenuItemType, menuItemStyle);
                element.Resources.Add(editorMenuItemType, editorContextMenuStyle);
            }
        }
    }
}
