using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFileInfoCommand : CommandElement
    {
        public ToggleVisibleFileInfoCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.ShortCutKey = "I";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleFileInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleFileInfo ? Properties.Resources.ToggleVisibleFileInfoCommand_Off : Properties.Resources.ToggleVisibleFileInfoCommand_On;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleFileInfo(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleFileInfo(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
