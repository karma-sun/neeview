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

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return ThumbnailList.Current.IsHideThumbnailList ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return ThumbnailList.Current.IsEnableThumbnailList;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ThumbnailList.Current.ToggleHideThumbnailList();
        }
    }
}
