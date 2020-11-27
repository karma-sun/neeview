using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformToSizeCommand : CommandElement
    {
        public SetStretchModeUniformToSizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToSize;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToSizeNote;
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToSize);
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return this.Text + (ViewComponent.Current.ViewController.TestStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)e.Parameter).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.SetStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)e.Parameter).IsToggle);
        }
    }
}
