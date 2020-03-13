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
            return new Binding(nameof(SidePanel.IsVisibleBookmarkList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisibleBookmarkList ? Properties.Resources.CommandToggleVisibleBookmarkListOff : Properties.Resources.CommandToggleVisibleBookmarkListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanel.Current.IsVisibleBookmarkList = Convert.ToBoolean(args[0]);
            }
            else
            {
                SidePanel.Current.ToggleVisibleBookmarkList(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
