using System;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePagemarkCommand : CommandElement
    {
        public TogglePagemarkCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Pagemark;
            this.ShortCutKey = "Ctrl+M";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsPagemark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.IsMarked() ? Properties.Resources.TogglePagemarkCommand_Off : Properties.Resources.TogglePagemarkCommand_On;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanPagemark();
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.SetPagemark(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookOperation.Current.TogglePagemark();
            }
        }
    }
}
