using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookmarkListCommand : CommandElement
    {
        public ToggleVisibleBookmarkListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleBookmarkList;
            this.MenuText = Properties.Resources.CommandToggleVisibleBookmarkListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleBookmarkListNote;
            this.ShortCutKey = "D";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleBookmarkList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleBookmarkList ? Properties.Resources.CommandToggleVisibleBookmarkListOff : Properties.Resources.CommandToggleVisibleBookmarkListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleBookmarkList(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleBookmarkList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
