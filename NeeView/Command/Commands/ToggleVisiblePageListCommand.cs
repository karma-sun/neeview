using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePageListCommand : CommandElement
    {
        public ToggleVisiblePageListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisiblePageList;
            this.MenuText = Properties.Resources.CommandToggleVisiblePageListMenu;
            this.Note = Properties.Resources.CommandToggleVisiblePageListNote;
            this.ShortCutKey = "P";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisiblePageList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisiblePageList ? Properties.Resources.CommandToggleVisiblePageListOff : Properties.Resources.CommandToggleVisiblePageListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisiblePageList(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisiblePageList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
