namespace NeeView
{
    public class UnloadCommand : CommandElement
    {
        public UnloadCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandUnload;
            this.MenuText = Properties.Resources.CommandUnloadMenu;
            this.Note = Properties.Resources.CommandUnloadNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHub.Current.CanUnload();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHub.Current.RequestUnload(this, true);
        }
    }
}
