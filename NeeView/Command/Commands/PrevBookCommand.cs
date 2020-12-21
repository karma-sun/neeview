namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
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
