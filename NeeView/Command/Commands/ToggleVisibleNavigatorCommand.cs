using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleNavigatorCommand : CommandElement
    {
        public ToggleVisibleNavigatorCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleNavigator;
            this.MenuText = Properties.Resources.CommandToggleVisibleNavigatorMenu;
            this.Note = Properties.Resources.CommandToggleVisibleNavigatorNote;
            this.ShortCutKey = "N";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleNavigator)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleNavigator ? Properties.Resources.CommandToggleVisibleNavigatorOff : Properties.Resources.CommandToggleVisibleNavigatorOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleNavigator(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleNavigator(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
