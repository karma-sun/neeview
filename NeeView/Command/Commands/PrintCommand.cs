namespace NeeView
{
    public class PrintCommand : CommandElement
    {
        public PrintCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.ShortCutKey = "Ctrl+P";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.CanPrint();
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.Print();
        }
    }
}
