using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFoldersTreeCommand : CommandElement
    {
        public ToggleVisibleFoldersTreeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleFoldersTree;
            this.MenuText = Properties.Resources.CommandToggleVisibleFoldersTreeMenu;
            this.Note = Properties.Resources.CommandToggleVisibleFoldersTreeNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookshelfConfig.IsFolderTreeVisible)) { Source = Config.Current.Bookshelf, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisibleFolderTree ? Properties.Resources.CommandToggleVisibleFoldersTreeOff : Properties.Resources.CommandToggleVisibleFoldersTreeOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanel.Current.IsVisibleFolderTree = Convert.ToBoolean(args[0]);
            }
            else
            {
                SidePanel.Current.ToggleVisibleFolderTree(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
