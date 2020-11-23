namespace NeeView
{
    public class NextBookCommand : CommandElement
    {
        public NextBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandNextFolder;
            this.Note = Properties.Resources.CommandNextFolderNote;
            this.ShortCutKey = "Down";
            this.MouseGesture = "LD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            var async = BookshelfFolderList.Current.NextFolder();
        }
    }
}
