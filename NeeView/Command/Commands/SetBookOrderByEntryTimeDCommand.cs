using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByEntryTimeDCommand : CommandElement
    {
        public SetBookOrderByEntryTimeDCommand(string name) : base(name)
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

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTimeDescending);
        }
    }
}
