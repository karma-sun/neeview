using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByEntryTimeDCommand : CommandElement
    {
        public SetFolderOrderByEntryTimeDCommand() : base("SetFolderOrderByEntryTimeD")
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByEntryTimeD;
            this.Note = Properties.Resources.CommandSetFolderOrderByEntryTimeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTimeDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTimeDescending);
        }
    }
}
