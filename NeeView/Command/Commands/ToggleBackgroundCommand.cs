namespace NeeView
{
    public class ToggleBackgroundCommand : CommandElement
    {
        public ToggleBackgroundCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Effect;
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
