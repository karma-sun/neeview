namespace NeeView
{
    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
            this.ShortCutKey = "Alt+Up";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHub.Current.CanLoadParent();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHub.Current.RequestLoadParent(this);
        }
    }
}
