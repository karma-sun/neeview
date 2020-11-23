namespace NeeView
{
    public class OpenSettingFilesFolderCommand : CommandElement
    {
        public OpenSettingFilesFolderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingFilesFolder;
            this.Note = Properties.Resources.CommandOpenSettingFilesFolderNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.OpenSettingFilesFolder();
        }
    }
}
