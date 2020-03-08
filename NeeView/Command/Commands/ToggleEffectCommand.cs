using NeeView.Effects;
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
            return new Binding(nameof(ImageEffect.Current.IsEnabled)) { Mode = BindingMode.OneWay, Source = ImageEffect.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ImageEffect.Current.IsEnabled ? Properties.Resources.CommandToggleEffectOff : Properties.Resources.CommandToggleEffectOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ImageEffect.Current.IsEnabled = !ImageEffect.Current.IsEnabled;
        }
    }
}
