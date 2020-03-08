namespace NeeView
{
    public class NextFolderCommand : CommandElement
    {
        public NextFolderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolder;
            this.Note = Properties.Resources.CommandNextFolderNote;
            this.ShortCutKey = "Down";
            this.MouseGesture = "LD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookshelfFolderList.Current.NextFolder();
        }
    }
}
