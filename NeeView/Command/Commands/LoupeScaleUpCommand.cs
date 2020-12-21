namespace NeeView
{
    public class LoupeScaleUpCommand : CommandElement
    {
        public LoupeScaleUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetLoupeMode();

        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.LoupeZoomIn();
        }
    }
}
