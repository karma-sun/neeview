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

        [MethodArgument(typeof(string), "@CommandLoadAsArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var path = args.Length > 0 ? args[0] as string : null;
            MainWindowModel.Current.LoadAs(path);
        }
    }
}
