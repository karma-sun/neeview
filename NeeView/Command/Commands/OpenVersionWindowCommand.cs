namespace NeeView
{
    public class OpenVersionWindowCommand : CommandElement
    {
        public OpenVersionWindowCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }
        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.OpenVersionWindow();
        }
    }
}
