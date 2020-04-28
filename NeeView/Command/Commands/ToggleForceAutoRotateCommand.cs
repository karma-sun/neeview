using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleForceAutoRotateCommand : CommandElement
    {
        public ToggleForceAutoRotateCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleForceAutoRotate;
            this.MenuText = Properties.Resources.CommandToggleForceAutoRotateMenu;
            this.Note = Properties.Resources.CommandToggleForceAutoRotateNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ViewConfig.ForceAutoRotate)) { Source = Config.Current.View };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.View.ForceAutoRotate ? Properties.Resources.CommandToggleForceAutoRotateOff : Properties.Resources.CommandToggleForceAutoRotateOn;
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
                Config.Current.View.ForceAutoRotate = Convert.ToBoolean(args[0]);
            }
            else
            {
                Config.Current.View.ForceAutoRotate = !Config.Current.View.ForceAutoRotate;
            }
        }
    }
}
