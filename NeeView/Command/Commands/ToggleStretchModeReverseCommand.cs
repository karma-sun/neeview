namespace NeeView
{
    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchModeReverse;
            this.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // "ToggleStretchMode"
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return ViewComponent.Current.ViewController.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)e.Parameter).ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.View.StretchMode = ViewComponent.Current.ViewController.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)e.Parameter);
        }
    }
}
