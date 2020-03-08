namespace NeeView
{
    public class DeleteBookCommand : CommandElement
    {
        public DeleteBookCommand() : base("DeleteBook")
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteBook;
            this.MenuText = Properties.Resources.CommandDeleteBookMenu;
            this.Note = Properties.Resources.CommandDeleteBookNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanDeleteBook();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.DeleteBook();
        }
    }
}
