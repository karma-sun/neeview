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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Background.BackgroundType.GetToggle().ToAliasName();
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = Config.Current.Background.BackgroundType.GetToggle();
        }
    }
}
