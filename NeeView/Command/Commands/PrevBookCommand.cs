namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevFolder;
            this.Note = Properties.Resources.CommandPrevFolderNote;
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            var async = BookshelfFolderList.Current.PrevFolder();
        }
    }
}
