namespace NeeView
{
    public class OpenVersionWindowCommand : CommandElement
    {
        public OpenVersionWindowCommand() : base("OpenVersionWindow")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenVersionWindow;
            this.MenuText = Properties.Resources.CommandOpenVersionWindowMenu;
            this.Note = Properties.Resources.CommandOpenVersionWindowNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenVersionWindow();
        }
    }
}
