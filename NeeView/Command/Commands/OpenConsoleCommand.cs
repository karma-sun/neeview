namespace NeeView
{
    public class OpenConsoleCommand : CommandElement
    {
        public OpenConsoleCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenConsole;
            this.MenuText = Properties.Resources.CommandOpenConsoleMenu;
            this.Note = Properties.Resources.CommandOpenConsoleNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.OpenConsoleWindow();
        }
    }
}
