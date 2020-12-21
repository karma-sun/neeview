namespace NeeView
{
    public class ToggleBookOrderCommand : CommandElement
    {
        public ToggleBookOrderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookOrder;
            this.IsShowMessage = true;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }
}
