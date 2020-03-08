using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideThumbnailListCommand : CommandElement
    {
        public ToggleHideThumbnailListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFilmStrip;
            this.Text = Properties.Resources.CommandToggleHideThumbnailList;
            this.MenuText = Properties.Resources.CommandToggleHideThumbnailListMenu;
            this.Note = Properties.Resources.CommandToggleHideThumbnailListNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ThumbnailList.Current.IsHideThumbnailList)) { Source = ThumbnailList.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsHideThumbnailList ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsEnableThumbnailList;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ThumbnailList.Current.ToggleHideThumbnailList();
        }
    }
}
