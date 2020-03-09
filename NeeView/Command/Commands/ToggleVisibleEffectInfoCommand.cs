using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleEffectInfoCommand : CommandElement
    {
        public ToggleVisibleEffectInfoCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleEffectInfo;
            this.MenuText = Properties.Resources.CommandToggleVisibleEffectInfoMenu;
            this.Note = Properties.Resources.CommandToggleVisibleEffectInfoNote;
            this.ShortCutKey = "E";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleEffectInfo)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return SidePanel.Current.IsVisibleEffectInfo ? Properties.Resources.CommandToggleVisibleEffectInfoOff : Properties.Resources.CommandToggleVisibleEffectInfoOn;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            SidePanel.Current.ToggleVisibleEffectInfo(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
