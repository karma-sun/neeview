namespace NeeView
{
    public class CloseApplicationCommand : CommandElement
    {
        public CloseApplicationCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindow.Current.Close();
        }
    }
}
