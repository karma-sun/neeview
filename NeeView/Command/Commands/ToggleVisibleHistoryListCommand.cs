using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleHistoryListCommand : CommandElement
    {
        public ToggleVisibleHistoryListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleHistoryList;
            this.MenuText = Properties.Resources.CommandToggleVisibleHistoryListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleHistoryListNote;
            this.ShortCutKey = "H";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleHistoryList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisibleHistoryList ? Properties.Resources.CommandToggleVisibleHistoryListOff : Properties.Resources.CommandToggleVisibleHistoryListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanel.Current.SetVisibleHistoryList(Convert.ToBoolean(args[0]), true);
            }
            else
            {
                SidePanel.Current.ToggleVisibleHistoryList(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
