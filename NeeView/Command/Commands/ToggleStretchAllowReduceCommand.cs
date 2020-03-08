using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowReduceCommand : CommandElement
    {
        public ToggleStretchAllowReduceCommand() : base("ToggleStretchAllowReduce")
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchAllowReduce;
            this.Note = Properties.Resources.CommandToggleStretchAllowReduceNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.AllowReduce)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.AllowReduce ? " OFF" : "");
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.AllowReduce = !ContentCanvas.Current.AllowReduce;
        }
    }
}
