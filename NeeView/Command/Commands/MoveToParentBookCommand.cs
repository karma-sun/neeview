namespace NeeView
{
    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandMoveToParentBook;
            this.Note = Properties.Resources.CommandMoveToParentBookNote;
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
