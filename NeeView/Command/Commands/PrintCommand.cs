namespace NeeView
{
    public class PrintCommand : CommandElement
    {
        public PrintCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandPrint;
            this.MenuText = Properties.Resources.CommandPrintMenu;
            this.Note = Properties.Resources.CommandPrintNote;
            this.ShortCutKey = "Ctrl+P";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.CanPrint();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            // TODO: Viewを直接呼び出さないようにする
            MainWindow.Current.Print();
        }
    }
}
