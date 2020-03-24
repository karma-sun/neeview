using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleNearestNeighborCommand : CommandElement
    {
        public ToggleNearestNeighborCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleIsEnabledNearestNeighbor;
            this.MenuText = Properties.Resources.CommandToggleIsEnabledNearestNeighborMenu;
            this.Note = Properties.Resources.CommandToggleIsEnabledNearestNeighborNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageDotKeepConfig.IsEnabled)) { Source = Config.Current.ImageDotKeep };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.ImageDotKeep.IsEnabled ? Properties.Resources.CommandToggleIsEnabledNearestNeighborOff : Properties.Resources.CommandToggleIsEnabledNearestNeighborOn;
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
                Config.Current.ImageDotKeep.IsEnabled = Convert.ToBoolean(args[0]);
            }
            else
            {
                Config.Current.ImageDotKeep.IsEnabled = !Config.Current.ImageDotKeep.IsEnabled;
            }
        }
    }
}
