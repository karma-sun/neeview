using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowEnlargeCommand : CommandElement
    {
        public ToggleStretchAllowEnlargeCommand(string name) : base(name)
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

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return this.Text + (ContentCanvas.Current.AllowEnlarge ? " OFF" : " ");
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ContentCanvas.Current.AllowEnlarge = !ContentCanvas.Current.AllowEnlarge;
        }
    }
}
