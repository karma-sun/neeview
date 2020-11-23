using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleGrid;
            this.MenuText = Properties.Resources.CommandToggleGridMenu;
            this.Note = Properties.Resources.CommandToggleGridNote;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageGridConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageGrid };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageGrid.IsEnabled ? Properties.Resources.CommandToggleGridOff : Properties.Resources.CommandToggleGridOn;
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
