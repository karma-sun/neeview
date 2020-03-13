using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformToVerticalCommand : CommandElement
    {
        public SetStretchModeUniformToVerticalCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToVertical;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToVerticalNote;
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToVertical);
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)param).IsToggle);
        }
    }
}
