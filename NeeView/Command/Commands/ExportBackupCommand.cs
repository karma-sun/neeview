namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }
        
        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ExportDataPresenter.Current.Export();
        }
    }
}
