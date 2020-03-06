using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowEnlargeCommand : CommandElement
    {
        public ToggleStretchAllowEnlargeCommand() : base(CommandType.ToggleStretchAllowEnlarge)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchAllowEnlarge;
            this.Note = Properties.Resources.CommandToggleStretchAllowEnlargeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.AllowEnlarge)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.AllowEnlarge ? " OFF" : " ");
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.AllowEnlarge = !ContentCanvas.Current.AllowEnlarge;
        }
    }
}
