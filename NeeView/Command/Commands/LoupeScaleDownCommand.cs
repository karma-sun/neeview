namespace NeeView
{
    public class LoupeScaleDownCommand : CommandElement
    {
        public LoupeScaleDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleDown;
            this.Note = Properties.Resources.CommandLoupeScaleDownNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return ViewComponent.Current.ViewController.GetLoupeMode();
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.LoupeZoomOut();
        }
    }
}
