namespace NeeView
{
    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // "ToggleStretchMode"
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)e.Parameter).ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewController.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)e.Parameter);
        }
    }
}
