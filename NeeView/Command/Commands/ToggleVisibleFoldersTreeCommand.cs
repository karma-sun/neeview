using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFoldersTreeCommand : CommandElement
    {
        public ToggleVisibleFoldersTreeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookshelfConfig.IsFolderTreeVisible)) { Source = Config.Current.Bookshelf, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleBookshelfFolderTree ? Properties.Resources.ToggleVisibleFoldersTreeCommand_Off : Properties.Resources.ToggleVisibleFoldersTreeCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.IsVisibleBookshelfFolderTree = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleBookshelfFolderTree(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
