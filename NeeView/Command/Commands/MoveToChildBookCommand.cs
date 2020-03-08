namespace NeeView
{
    public class MoveToChildBookCommand : CommandElement
    {
        public MoveToChildBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandMoveToChildBook;
            this.Note = Properties.Resources.CommandMoveToChildBookNote;
            this.ShortCutKey = "Alt+Down";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanMoveToChildBook();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.MoveToChildBook();
        }
    }
}
