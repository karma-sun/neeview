namespace NeeView
{
    public class StretchWindowCommand : CommandElement
    {
        public StretchWindowCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandStretchWindow;
            this.MenuText = Properties.Resources.CommandStretchWindowMenu;
            this.Note = Properties.Resources.CommandStretchWindowNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.StretchWindow();
        }
    }
}