using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleHistoryListCommand : CommandElement
    {
        public ToggleVisibleHistoryListCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.ShortCutKey = "H";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleHistoryList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleHistoryList ? Properties.Resources.ToggleVisibleHistoryListCommand_Off : Properties.Resources.ToggleVisibleHistoryListCommand_On;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleHistoryList(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleHistoryList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
