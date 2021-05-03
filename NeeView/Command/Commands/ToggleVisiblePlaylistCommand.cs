using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePlaylistCommand : CommandElement
    {
        public ToggleVisiblePlaylistCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.ShortCutKey = "M";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePlaylist)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisiblePlaylist ? Properties.Resources.ToggleVisiblePlaylistCommand_Off : Properties.Resources.ToggleVisiblePlaylistCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisiblePlaylist(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePlaylist(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
