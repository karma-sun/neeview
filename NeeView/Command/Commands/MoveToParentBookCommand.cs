namespace NeeView
{
    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandMoveToParentBook;
            this.Note = Properties.Resources.CommandMoveToParentBookNote;
            this.ShortCutKey = "Alt+Up";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookHub.Current.CanLoadParent();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookHub.Current.RequestLoadParent();
        }
    }
}
