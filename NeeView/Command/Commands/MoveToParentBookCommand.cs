namespace NeeView
{
    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand() : base(CommandType.MoveToParentBook)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandMoveToParentBook;
            this.Note = Properties.Resources.CommandMoveToParentBookNote;
            this.ShortCutKey = "Alt+Up";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHub.Current.CanLoadParent();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHub.Current.RequestLoadParent();
        }
    }
}
