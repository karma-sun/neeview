using NeeView.Effects;
using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleEffectCommand : CommandElement
    {
        public ToggleEffectCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Effect;
            this.ShortCutKey = "Ctrl+E";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageEffectConfig.IsEnabled)) { Mode = BindingMode.OneWay, Source = Config.Current.ImageEffect };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.ImageEffect.IsEnabled ? Properties.Resources.ToggleEffectCommand_Off : Properties.Resources.ToggleEffectCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
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
