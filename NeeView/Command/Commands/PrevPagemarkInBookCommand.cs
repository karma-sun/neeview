namespace NeeView
{
    public class PrevPagemarkInBookCommand : CommandElement
    {
        public PrevPagemarkInBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemarkInBook;
            this.Note = Properties.Resources.CommandPrevPagemarkInBookNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePagemarkCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanPrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.PrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }
    }
}
