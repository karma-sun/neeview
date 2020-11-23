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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageDotKeep.IsEnabled ? Properties.Resources.CommandToggleIsEnabledNearestNeighborOff : Properties.Resources.CommandToggleIsEnabledNearestNeighborOn;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageDotKeep.IsEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.ImageDotKeep.IsEnabled = !Config.Current.ImageDotKeep.IsEnabled;
            }
        }
    }
}
