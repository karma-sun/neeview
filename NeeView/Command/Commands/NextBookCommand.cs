namespace NeeView
{
    public class NextBookCommand : CommandElement
    {
        public NextBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
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
