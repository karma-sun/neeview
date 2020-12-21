using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookmarkListCommand : CommandElement
    {
        public ToggleVisibleBookmarkListCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.ShortCutKey = "D";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleBookmarkList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleBookmarkList ? Properties.Resources.ToggleVisibleBookmarkListCommand_Off : Properties.Resources.ToggleVisibleBookmarkListCommand_On;
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
