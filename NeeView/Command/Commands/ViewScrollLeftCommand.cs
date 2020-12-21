namespace NeeView
{
    public class ViewScrollLeftCommand : CommandElement
    {
        public ViewScrollLeftCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollLeft((ViewScrollCommandParameter)e.Parameter);
        }
    }
}
