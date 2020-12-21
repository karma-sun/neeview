namespace NeeView
{
    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
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
