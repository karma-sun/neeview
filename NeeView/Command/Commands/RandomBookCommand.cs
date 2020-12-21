namespace NeeView
{
    public class RandomBookCommand : CommandElement
    {
        public RandomBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
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
