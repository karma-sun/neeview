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
            return new Binding(nameof(ViewConfig.AllowStretchScaleUp)) { Source = Config.Current.View };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return this.Text + (Config.Current.View.AllowStretchScaleUp ? " OFF" : " ");
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
                Config.Current.View.AllowStretchScaleUp = Convert.ToBoolean(args[0]);
            }
            else
            {
                Config.Current.View.AllowStretchScaleUp = !Config.Current.View.AllowStretchScaleUp;
            }
        }
    }
}
