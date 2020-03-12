using System;
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

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return this.Text + (ContentCanvas.Current.AllowEnlarge ? " OFF" : " ");
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                ContentCanvas.Current.AllowEnlarge = Convert.ToBoolean(args[0]);
            }
            else
            {
                ContentCanvas.Current.AllowEnlarge = !ContentCanvas.Current.AllowEnlarge;
            }
        }
    }
}
