namespace NeeView
{
    public class ToggleFolderOrderCommand : CommandElement
    {
        public ToggleFolderOrderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandToggleFolderOrder;
            this.Note = Properties.Resources.CommandToggleFolderOrderNote;
            this.IsShowMessage = true;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }
}
