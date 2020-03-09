using System.Windows.Data;


namespace NeeView
{
    public class TogglePagemarkCommand : CommandElement
    {
        public TogglePagemarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandTogglePagemark;
            this.MenuText = Properties.Resources.CommandTogglePagemarkMenu;
            this.Note = Properties.Resources.CommandTogglePagemarkNote;
            this.ShortCutKey = "Ctrl+M";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsPagemark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.IsMarked() ? Properties.Resources.CommandTogglePagemarkOff : Properties.Resources.CommandTogglePagemarkOn;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanPagemark();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.TogglePagemark();
        }
    }
}
