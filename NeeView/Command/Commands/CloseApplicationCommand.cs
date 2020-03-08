namespace NeeView
{
    public class CloseApplicationCommand : CommandElement
    {
        public CloseApplicationCommand() : base("CloseApplication")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandCloseApplication;
            this.MenuText = Properties.Resources.CommandCloseApplicationMenu;
            this.Note = Properties.Resources.CommandCloseApplicationNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.Close();
        }
    }
}
