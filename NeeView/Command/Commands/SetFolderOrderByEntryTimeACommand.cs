using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByEntryTimeACommand : CommandElement
    {
        public SetFolderOrderByEntryTimeACommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTime);
        }
    }
}
