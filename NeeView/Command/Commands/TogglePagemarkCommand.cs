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

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.IsMarked() ? Properties.Resources.CommandTogglePagemarkOff : Properties.Resources.CommandTogglePagemarkOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanPagemark();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.TogglePagemark();
        }
    }
}
