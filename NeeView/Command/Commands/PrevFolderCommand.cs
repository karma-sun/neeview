namespace NeeView
{
    public class PrevFolderCommand : CommandElement
    {
        public PrevFolderCommand() : base("PrevFolder")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevFolder;
            this.Note = Properties.Resources.CommandPrevFolderNote;
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookshelfFolderList.Current.PrevFolder();
        }
    }
}
