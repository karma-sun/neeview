using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByEntryTimeDCommand : CommandElement
    {
        public SetFolderOrderByEntryTimeDCommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTimeDescending);
        }
    }
}
