namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandPrevFolder;
            this.Note = Properties.Resources.CommandPrevFolderNote;
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            var async = BookshelfFolderList.Current.PrevFolder();
        }
    }
}
