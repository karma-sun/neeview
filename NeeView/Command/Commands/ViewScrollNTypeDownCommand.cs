namespace NeeView
{
    public class ViewScrollNTypeDownCommand : CommandElement
    {
        public ViewScrollNTypeDownCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            // ViewScrollNTypeUpCommand
            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollNTypeDown((ViewScrollNTypeCommandParameter)e.Parameter);
        }
    }

}
