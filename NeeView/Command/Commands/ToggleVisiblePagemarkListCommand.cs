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
            return new Binding(nameof(SidePanel.IsVisiblePagemarkList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisiblePagemarkList ? Properties.Resources.CommandToggleVisiblePagemarkListOff : Properties.Resources.CommandToggleVisiblePagemarkListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanel.Current.IsVisiblePagemarkList = Convert.ToBoolean(args[0]);
            }
            else
            {
                SidePanel.Current.ToggleVisiblePagemarkList(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
