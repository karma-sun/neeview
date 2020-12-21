using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformToFillCommand : CommandElement
    {
        public SetStretchModeUniformToFillCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.IsShowMessage = true;

            // "SetStretchModeUniform"
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToFill);
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return this.Text + (MainViewComponent.Current.ViewController.TestStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)e.Parameter).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.SetStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)e.Parameter).IsToggle);
        }
    }
}
