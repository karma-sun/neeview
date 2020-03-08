using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByEntryTimeACommand : CommandElement
    {
        public SetFolderOrderByEntryTimeACommand() : base("SetFolderOrderByEntryTimeA")
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByEntryTimeA;
            this.Note = Properties.Resources.CommandSetFolderOrderByEntryTimeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTime);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTime);
        }
    }
}
