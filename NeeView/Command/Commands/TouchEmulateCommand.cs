namespace NeeView
{
    public class TouchEmulateCommand : CommandElement
    {
        public TouchEmulateCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandTouchEmulate;
            this.Note = Properties.Resources.CommandTouchEmulateNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.TouchInputEmutrate(sender);
        }
    }
}
