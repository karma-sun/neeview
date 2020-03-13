using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleBookshelfCommand : CommandElement
    {
        public ToggleVisibleBookshelfCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleBookshelf;
            this.MenuText = Properties.Resources.CommandToggleVisibleBookshelfMenu;
            this.Note = Properties.Resources.CommandToggleVisibleBookshelfNote;
            this.ShortCutKey = "B";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleFolderList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return SidePanel.Current.IsVisibleFolderList ? Properties.Resources.CommandToggleVisibleBookshelfOff : Properties.Resources.CommandToggleVisibleBookshelfOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                SidePanel.Current.IsVisibleFolderList = Convert.ToBoolean(args[0]);
            }
            else
            {
                SidePanel.Current.ToggleVisibleFolderList(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
