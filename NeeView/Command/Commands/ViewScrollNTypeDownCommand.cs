namespace NeeView
{
    public class ViewScrollNTypeDownCommand : CommandElement
    {
        public ViewScrollNTypeDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollNTypeDown;
            this.Note = Properties.Resources.CommandViewScrollNTypeDownNote;
            this.IsShowMessage = false;

            // ViewScrollNTypeUpCommand
            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ScrollNTypeDown((ViewScrollNTypeCommandParameter)e.Parameter);
        }
    }

}
