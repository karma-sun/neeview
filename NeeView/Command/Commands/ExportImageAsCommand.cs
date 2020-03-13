namespace NeeView
{
    public class ExportImageAsCommand : CommandElement
    {
        public ExportImageAsCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImageDialog;
            this.MenuText = Properties.Resources.CommandExportImageDialogMenu;
            this.Note = Properties.Resources.CommandExportImageDialogNote;
            this.ShortCutKey = "Ctrl+S";
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportImageDialogCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.ExportDialog((ExportImageDialogCommandParameter)param);
        }
    }
}
