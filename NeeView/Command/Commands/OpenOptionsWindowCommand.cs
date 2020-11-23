namespace NeeView
{
    public class OpenOptionsWindowCommand : CommandElement
    {
        public OpenOptionsWindowCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingWindow;
            this.MenuText = Properties.Resources.CommandOpenSettingWindowMenu;
            this.Note = Properties.Resources.CommandOpenSettingWindowNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingWindow();
        }
    }
}
