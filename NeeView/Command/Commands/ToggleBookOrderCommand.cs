namespace NeeView
{
    public class ToggleBookOrderCommand : CommandElement
    {
        public ToggleBookOrderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandToggleFolderOrder;
            this.Note = Properties.Resources.CommandToggleFolderOrderNote;
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
