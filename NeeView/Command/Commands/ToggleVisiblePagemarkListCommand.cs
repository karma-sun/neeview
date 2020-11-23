using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePagemarkListCommand : CommandElement
    {
        public ToggleVisiblePagemarkListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisiblePagemarkList;
            this.MenuText = Properties.Resources.CommandToggleVisiblePagemarkListMenu;
            this.Note = Properties.Resources.CommandToggleVisiblePagemarkListNote;
            this.ShortCutKey = "M";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePagemarkList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisiblePagemarkList ? Properties.Resources.CommandToggleVisiblePagemarkListOff : Properties.Resources.CommandToggleVisiblePagemarkListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisiblePagemarkList(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePagemarkList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
