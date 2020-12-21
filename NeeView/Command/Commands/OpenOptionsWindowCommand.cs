namespace NeeView
{
    public class OpenOptionsWindowCommand : CommandElement
    {
        public OpenOptionsWindowCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingWindow();
        }
    }
}
