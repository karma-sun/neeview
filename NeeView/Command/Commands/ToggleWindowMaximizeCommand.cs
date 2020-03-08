namespace NeeView
{
    public class ToggleWindowMaximizeCommand : CommandElement
    {
        public ToggleWindowMaximizeCommand() : base("ToggleWindowMaximize")
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMaximize;
            this.MenuText = Properties.Resources.CommandToggleWindowMaximizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMaximizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.MainWindow_Maximize();
        }
    }
}
