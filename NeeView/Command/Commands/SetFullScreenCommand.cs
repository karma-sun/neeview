namespace NeeView
{
    public class SetFullScreenCommand : CommandElement
    {
        public SetFullScreenCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandSetFullScreen;
            this.Note = Properties.Resources.CommandSetFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.SetFullScreen(sender, true);
        }
    }
}
