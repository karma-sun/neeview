using System;
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
            return new Binding(nameof(SystemConfig.IsFileWriteAccessEnabled)) { Source = Config.Current.System, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.System.IsFileWriteAccessEnabled ? Properties.Resources.CommandTogglePermitFileCommandOff : Properties.Resources.CommandTogglePermitFileCommandOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                Config.Current.System.IsFileWriteAccessEnabled = Convert.ToBoolean(args[0]);
            }
            else
            {
                Config.Current.System.IsFileWriteAccessEnabled = !Config.Current.System.IsFileWriteAccessEnabled;
            }
        }
    }
}
