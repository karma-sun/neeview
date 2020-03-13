namespace NeeView
{
    public class ToggleBackgroundCommand : CommandElement
    {
        public ToggleBackgroundCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleBackground;
            this.Note = Properties.Resources.CommandToggleBackgroundNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvasBrush.Current.Background.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvasBrush.Current.Background = ContentCanvasBrush.Current.Background.GetToggle();
        }
    }
}
