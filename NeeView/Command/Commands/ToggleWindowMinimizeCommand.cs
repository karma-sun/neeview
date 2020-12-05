namespace NeeView
{
    public class ToggleWindowMinimizeCommand : CommandElement
    {
        public ToggleWindowMinimizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMinimize;
            this.MenuText = Properties.Resources.CommandToggleWindowMinimizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMinimizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ToggleWindowMinimize(sender);
        }
    }
}
