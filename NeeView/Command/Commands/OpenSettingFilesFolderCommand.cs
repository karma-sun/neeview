namespace NeeView
{
    public class OpenSettingFilesFolderCommand : CommandElement
    {
        public OpenSettingFilesFolderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingFilesFolder();
        }
    }
}
