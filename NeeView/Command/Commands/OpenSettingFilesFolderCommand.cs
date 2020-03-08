namespace NeeView
{
    public class OpenSettingFilesFolderCommand : CommandElement
    {
        public OpenSettingFilesFolderCommand() : base("OpenSettingFilesFolder")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingFilesFolder;
            this.Note = Properties.Resources.CommandOpenSettingFilesFolderNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenSettingFilesFolder();
        }
    }
}
