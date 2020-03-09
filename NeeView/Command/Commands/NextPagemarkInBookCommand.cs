namespace NeeView
{
    public class NextPagemarkInBookCommand : CommandElement
    {
        public NextPagemarkInBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemarkInBook;
            this.Note = Properties.Resources.CommandNextPagemarkInBookNote;
            this.IsShowMessage = false;

            // PrevPagemarkInBook
            this.ParameterSource = new CommandParameterSource(new MovePagemarkCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanNextPagemarkInPlace((MovePagemarkCommandParameter)param);
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.NextPagemarkInPlace((MovePagemarkCommandParameter)param);
        }
    }
}
