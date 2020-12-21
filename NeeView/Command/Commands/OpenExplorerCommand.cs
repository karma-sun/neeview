namespace NeeView
{
    public class OpenExplorerCommand : CommandElement
    {
        public OpenExplorerCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.OpenFilePlace();
        }
    }
}
