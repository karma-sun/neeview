using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileNameDCommand : CommandElement
    {
        public SetBookOrderByFileNameDCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileNameDescending);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileNameDescending);
        }
    }
}
