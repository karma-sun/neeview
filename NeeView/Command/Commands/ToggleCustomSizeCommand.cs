using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleCustomSizeCommand : CommandElement
    {
        public ToggleCustomSizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleCustomSize;
            this.MenuText = Properties.Resources.CommandToggleCustomSizeMenu;
            this.Note = Properties.Resources.CommandToggleCustomSizeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageCustomSizeConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageCustomSize };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageCustomSize.IsEnabled ? Properties.Resources.CommandToggleCustomSizeOff : Properties.Resources.CommandToggleCustomSizeOn;
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
                Config.Current.ImageCustomSize.IsEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.ImageCustomSize.IsEnabled = !Config.Current.ImageCustomSize.IsEnabled;
            }
        }
    }
}
