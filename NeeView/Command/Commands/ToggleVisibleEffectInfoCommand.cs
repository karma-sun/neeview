using System;
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
            return new Binding(nameof(SidePanelFrame.IsVisibleEffectInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleEffectInfo ? Properties.Resources.CommandToggleVisibleEffectInfoOff : Properties.Resources.CommandToggleVisibleEffectInfoOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleEffectInfo(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleEffectInfo(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
