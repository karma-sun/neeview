using NeeView.Windows.Property;

namespace NeeView
{
    public class NextPagemarkInBookCommand : CommandElement
    {
        public NextPagemarkInBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Pagemark;
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
