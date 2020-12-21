using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Effect;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageGridConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageGrid };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageGrid.IsEnabled ? Properties.Resources.ToggleGridCommand_Off : Properties.Resources.ToggleGridCommand_On;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageGrid.IsEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.ImageGrid.IsEnabled = !Config.Current.ImageGrid.IsEnabled;
            }
        }
    }
}
