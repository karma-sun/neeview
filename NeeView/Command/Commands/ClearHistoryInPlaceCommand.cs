namespace NeeView
{
    public class ClearHistoryInPlaceCommand : CommandElement
    {
        public ClearHistoryInPlaceCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandClearHistoryInPlace;
            this.Note = Properties.Resources.CommandClearHistoryInPlaceNote;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookshelfFolderList.Current.Place != null;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.ClearHistory();
        }
    }
}
