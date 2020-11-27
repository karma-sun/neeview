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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return ViewComponent.Current.ViewController.CanPrint();
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.Print();
        }
    }
}
