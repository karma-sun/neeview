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

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ImageEffect.Current.IsEnabled ? Properties.Resources.CommandToggleEffectOff : Properties.Resources.CommandToggleEffectOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ImageEffect.Current.IsEnabled = !ImageEffect.Current.IsEnabled;
        }
    }
}
