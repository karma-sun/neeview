using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookmark;
            this.Text = Properties.Resources.CommandToggleBookmark;
            this.MenuText = Properties.Resources.CommandToggleBookmarkMenu;
            this.Note = Properties.Resources.CommandToggleBookmarkNote;
            this.ShortCutKey = "Ctrl+D";
            this.IsShowMessage = true;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsBookmark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.IsBookmark ? Properties.Resources.CommandToggleBookmarkOff : Properties.Resources.CommandToggleBookmarkOn;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanBookmark();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.ToggleBookmark();
        }
    }
}
