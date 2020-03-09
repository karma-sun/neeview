namespace NeeView
{
    public class ExportImageCommand : CommandElement
    {
        public ExportImageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImage;
            this.MenuText = Properties.Resources.CommandExportImageMenu;
            this.Note = Properties.Resources.CommandExportImageNote;
            this.ShortCutKey = "Shift+Ctrl+S";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ExportImageCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.Export((ExportImageCommandParameter)param);
        }
    }
}
