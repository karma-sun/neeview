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

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)param).IsToggle);
        }
    }
}
