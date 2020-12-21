namespace NeeView
{
    public class ViewRotateRightCommand : CommandElement
    {
        public ViewRotateRightCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
            
            // ViewRotateLeft
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ViewRotateRight((ViewRotateCommandParameter)e.Parameter);
        }
    }
}
