namespace NeeView
{
    public class ViewFlipVerticalOnCommand : CommandElement
    {
        public ViewFlipVerticalOnCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.FlipVertical(true);
        }
    }
}
