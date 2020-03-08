namespace NeeView
{
    public class PrevPagemarkInBookCommand : CommandElement
    {
        public PrevPagemarkInBookCommand() : base("PrevPagemarkInBook")
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemarkInBook;
            this.Note = Properties.Resources.CommandPrevPagemarkInBookNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePagemarkCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanPrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }
    }
}
