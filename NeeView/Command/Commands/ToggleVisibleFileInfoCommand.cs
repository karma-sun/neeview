using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleFileInfoCommand : CommandElement
    {
        public ToggleVisibleFileInfoCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleFileInfo;
            this.MenuText = Properties.Resources.CommandToggleVisibleFileInfoMenu;
            this.Note = Properties.Resources.CommandToggleVisibleFileInfoNote;
            this.ShortCutKey = "I";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleFileInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanelFrame.Current.IsVisibleFileInfo ? Properties.Resources.CommandToggleVisibleFileInfoOff : Properties.Resources.CommandToggleVisibleFileInfoOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleFileInfo(Convert.ToBoolean(args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleFileInfo(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
