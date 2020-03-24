using System;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePageListPlacementCommand : CommandElement
    {
        public TogglePageListPlacementCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandTogglePageListPlacement;
            this.MenuText = Properties.Resources.CommandTogglePageListPlacementMenu;
            this.Note = Properties.Resources.CommandTogglePageListPlacementNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookshelfPanelConfig.IsPageListDocked)) { Source = Config.Current.Layout.Bookshelf };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Layout.Bookshelf.IsPageListDocked ? Properties.Resources.CommandTogglePageListPlacementPanel : Properties.Resources.CommandTogglePageListPlacementBookshelf;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                Config.Current.Layout.Bookshelf.IsPageListDocked = Convert.ToBoolean(args[0]);
            }
            else
            {
                Config.Current.Layout.Bookshelf.IsPageListDocked = !Config.Current.Layout.Bookshelf.IsPageListDocked;
            }
        }
    }
}
