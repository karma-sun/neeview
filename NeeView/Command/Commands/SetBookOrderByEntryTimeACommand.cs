using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByEntryTimeACommand : CommandElement
    {
        public SetBookOrderByEntryTimeACommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTime);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTime);
        }
    }
}
