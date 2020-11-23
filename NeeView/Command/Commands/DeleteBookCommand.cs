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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanDeleteBook();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.DeleteBook();
        }
    }
}
