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

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var async = BookshelfFolderList.Current.RandomFolder();
        }
    }

}
