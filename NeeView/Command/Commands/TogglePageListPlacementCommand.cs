using System.Windows.Data;


namespace NeeView
{
    public class TogglePageListPlacementCommand : CommandElement
    {
        public TogglePageListPlacementCommand() : base(CommandType.TogglePageListPlacement)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandTogglePageListPlacement;
            this.MenuText = Properties.Resources.CommandTogglePageListPlacementMenu;
            this.Note = Properties.Resources.CommandTogglePageListPlacementNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PageListPlacementService.Current.IsPlacedInBookshelf)) { Source = PageListPlacementService.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return PageListPlacementService.Current.IsPlacedInBookshelf ? Properties.Resources.CommandTogglePageListPlacementPanel : Properties.Resources.CommandTogglePageListPlacementBookshelf;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PageListPlacementService.Current.IsPlacedInBookshelf = !PageListPlacementService.Current.IsPlacedInBookshelf;
        }
    }
}
