namespace NeeView
{
    public class DeleteBookCommand : CommandElement
    {
        public DeleteBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteBook;
            this.MenuText = Properties.Resources.CommandDeleteBookMenu;
            this.Note = Properties.Resources.CommandDeleteBookNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanDeleteBook();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.DeleteBook();
        }
    }
}
