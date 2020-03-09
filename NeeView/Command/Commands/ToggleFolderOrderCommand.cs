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

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }
}
