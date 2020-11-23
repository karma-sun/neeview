using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleResizeFilterCommand : CommandElement
    {
        public ToggleResizeFilterCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleResizeFilter;
            this.MenuText = Properties.Resources.CommandToggleResizeFilterMenu;
            this.Note = Properties.Resources.CommandToggleResizeFilterNote;
            this.ShortCutKey = "Ctrl+R";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageResizeFilterConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageResizeFilter };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageResizeFilter.IsEnabled ? Properties.Resources.CommandToggleResizeFilterOff : Properties.Resources.CommandToggleResizeFilterOn;
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
                Config.Current.ImageResizeFilter.IsEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.ImageResizeFilter.IsEnabled = !Config.Current.ImageResizeFilter.IsEnabled;
            }
        }
    }
}
