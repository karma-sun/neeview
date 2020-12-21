namespace NeeView
{
    public class ViewFlipVerticalOffCommand : CommandElement
    {
        public ViewFlipVerticalOffCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.FlipVertical(false);
        }
    }
}
