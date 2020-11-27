namespace NeeView
{
    public class ViewRotateRightCommand : CommandElement
    {
        public ViewRotateRightCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateRight;
            this.Note = Properties.Resources.CommandViewRotateRightNote;
            this.IsShowMessage = false;
            
            // ViewRotateLeft
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ViewRotateRight((ViewRotateCommandParameter)e.Parameter);
        }
    }
}
