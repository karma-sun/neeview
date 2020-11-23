using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformToFillCommand : CommandElement
    {
        public SetStretchModeUniformToFillCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToFill;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToFillNote;
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
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)e.Parameter).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)e.Parameter).IsToggle);
        }
    }
}
