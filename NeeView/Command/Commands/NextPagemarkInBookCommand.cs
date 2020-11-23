using NeeView.Windows.Property;

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
            this.ParameterSource = new CommandParameterSource(new MovePagemarkInBookCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanNextPagemarkInPlace((MovePagemarkInBookCommandParameter)e.Parameter);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextPagemarkInPlace((MovePagemarkInBookCommandParameter)e.Parameter);
        }
    }

}
