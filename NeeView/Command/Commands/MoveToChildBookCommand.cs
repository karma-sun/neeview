namespace NeeView
{
    public class MoveToChildBookCommand : CommandElement
    {
        public MoveToChildBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandMoveToChildBook;
            this.Note = Properties.Resources.CommandMoveToChildBookNote;
            this.ShortCutKey = "Alt+Down";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanMoveToChildBook();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.MoveToChildBook(this);
        }
    }
}
