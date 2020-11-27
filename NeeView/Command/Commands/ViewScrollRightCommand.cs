namespace NeeView
{
    public class ViewScrollRightCommand : CommandElement
    {
        public ViewScrollRightCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollRight;
            this.Note = Properties.Resources.CommandViewScrollRightNote;
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ScrollRight((ViewScrollCommandParameter)e.Parameter);
        }
    }
}
