using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByEntryTimeACommand : CommandElement
    {
        public SetBookOrderByEntryTimeACommand(string name) : base(name)
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
