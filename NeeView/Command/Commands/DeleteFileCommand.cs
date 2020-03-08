namespace NeeView
{
    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand() : base("DeleteFile")
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteFile;
            this.MenuText = Properties.Resources.CommandDeleteFileMenu;
            this.Note = Properties.Resources.CommandDeleteFileNote;
            this.ShortCutKey = "Delete";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanDeleteFile();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookOperation.Current.DeleteFileAsync();
        }
    }
}
