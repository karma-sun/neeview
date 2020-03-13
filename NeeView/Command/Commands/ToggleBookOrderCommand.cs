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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }
}
