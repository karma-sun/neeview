using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NeeView
{
    public static class ExternalAppCollectionUtility
    {
        /// <summary>
        /// 「外部アプリで開く」メニュー作成
        /// </summary>
        /// <param name="isEnabled">メニューの有効/無効</param>
        /// <param name="command">実行コマンド</param>
        /// <param name="OpenExternalAppDialogCommand">設定コマンド</param>
        public static MenuItem CreateExternalAppItem(bool isEnabled, ICommand command, ICommand OpenExternalAppDialogCommand)
        {
            MenuItem subItem = new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_OpenExternalApp, IsEnabled = isEnabled };

            if (Config.Current.System.ExternalAppCollection.Any())
            {
                for (int i = 0; i < Config.Current.System.ExternalAppCollection.Count; ++i)
                {
                    var folder = Config.Current.System.ExternalAppCollection[i];
                    var header = new TextBlock(new Run(folder.DispName));
                    subItem.Items.Add(new MenuItem() { Header = header, ToolTip = folder.Command, Command = command, CommandParameter = folder });
                }
            }
            else
            {
                subItem.Items.Add(new MenuItem() { Header = Properties.Resources.WordItemNone, IsEnabled = false });
            }

            subItem.Items.Add(new Separator());
            subItem.Items.Add(new MenuItem() { Header = Properties.Resources.BookshelfItem_Menu_ExternalAppOption, Command = OpenExternalAppDialogCommand });

            return subItem;
        }


    }


}
