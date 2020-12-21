namespace NeeView
{
    public class ViewScrollRightCommand : CommandElement
    {
        public ViewScrollRightCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollRight((ViewScrollCommandParameter)e.Parameter);
        }
    }
}
