namespace NeeView
{
    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteFile;
            this.MenuText = Properties.Resources.CommandDeleteFileMenu;
            this.Note = Properties.Resources.CommandDeleteFileNote;
            this.ShortCutKey = "Delete";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanDeleteFile();
        }

        public override void Execute(object sender, CommandContext e)
        {
            var async = BookOperation.Current.DeleteFileAsync();
        }
    }
}
