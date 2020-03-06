namespace NeeView
{
    public class ToggleBackgroundCommand : CommandElement
    {
        public ToggleBackgroundCommand() : base(CommandType.ToggleBackground)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleBackground;
            this.Note = Properties.Resources.CommandToggleBackgroundNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvasBrush.Current.Background.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = ContentCanvasBrush.Current.Background.GetToggle();
        }
    }
}
