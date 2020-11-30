namespace NeeView
{
    public class ToggleWindowMaximizeCommand : CommandElement
    {
        public ToggleWindowMaximizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMaximize;
            this.MenuText = Properties.Resources.CommandToggleWindowMaximizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMaximizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ToggleWindowMaximize(sender);
        }
    }
}
