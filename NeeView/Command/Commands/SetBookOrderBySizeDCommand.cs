using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderBySizeDCommand : CommandElement
    {
        public SetBookOrderBySizeDCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.SizeDescending);
        }
        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.SizeDescending);
        }
    }
}
