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
            return new Binding(nameof(BookshelfFolderList.Current.IsFolderTreeVisible)) { Source = BookshelfFolderList.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisibleFolderTree ? Properties.Resources.CommandToggleVisibleFoldersTreeOff : Properties.Resources.CommandToggleVisibleFoldersTreeOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            SidePanel.Current.ToggleVisibleFolderTree(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
