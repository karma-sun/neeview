namespace NeeView
{
    public class RandomBookCommand : CommandElement
    {
        public RandomBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandRandomFolder;
            this.Note = Properties.Resources.CommandRandomFolderNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            var async = BookshelfFolderList.Current.RandomFolder();
        }
    }

}
