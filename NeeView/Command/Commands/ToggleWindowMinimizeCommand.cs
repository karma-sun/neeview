namespace NeeView
{
    public class ToggleWindowMinimizeCommand : CommandElement
    {
        public ToggleWindowMinimizeCommand() : base("ToggleWindowMinimize")
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMinimize;
            this.MenuText = Properties.Resources.CommandToggleWindowMinimizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMinimizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.MainWindow_Minimize();
        }
    }
}
