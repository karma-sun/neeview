using NeeView.Effects;
using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleEffectCommand : CommandElement
    {
        public ToggleEffectCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleEffect;
            this.MenuText = Properties.Resources.CommandToggleEffectMenu;
            this.Note = Properties.Resources.CommandToggleEffectNote;
            this.ShortCutKey = "Ctrl+E";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageEffectConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageEffect };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageEffect.IsEnabled ? Properties.Resources.CommandToggleEffectOff : Properties.Resources.CommandToggleEffectOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.ImageEffect.IsEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.ImageEffect.IsEnabled = !Config.Current.ImageEffect.IsEnabled;
            }
        }
    }
}
