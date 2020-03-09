namespace NeeView
{
    public class LoadAsCommand : CommandElement
    {
        public LoadAsCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandLoadAs;
            this.MenuText = Properties.Resources.CommandLoadAsMenu;
            this.Note = Properties.Resources.CommandLoadAsNote;
            this.ShortCutKey = "Ctrl+O";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MainWindowModel.Current.LoadAs(arg as string);
        }
    }
}
