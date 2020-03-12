using System.Windows.Data;


namespace NeeView
{
    public class TogglePermitFileCommandCommand : CommandElement
    {
        public TogglePermitFileCommandCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandTogglePermitFileCommand;
            this.MenuText = Properties.Resources.CommandTogglePermitFileCommandMenu;
            this.Note = Properties.Resources.CommandTogglePermitFileCommandNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FileIOProfile.Current.IsEnabled)) { Source = FileIOProfile.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return FileIOProfile.Current.IsEnabled ? Properties.Resources.CommandTogglePermitFileCommandOff : Properties.Resources.CommandTogglePermitFileCommandOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            FileIOProfile.Current.IsEnabled = !FileIOProfile.Current.IsEnabled;
        }
    }
}
