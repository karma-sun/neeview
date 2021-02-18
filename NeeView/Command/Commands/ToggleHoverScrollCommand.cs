using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHoverScrollCommand : CommandElement
    {
        public ToggleHoverScrollCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MouseConfig.IsHoverScroll)) { Source = Config.Current.Mouse, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Mouse.IsHoverScroll ? Properties.Resources.ToggleHoverScrollCommand_Off : Properties.Resources.ToggleHoverScrollCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.Mouse.IsHoverScroll = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.Mouse.IsHoverScroll = !Config.Current.Mouse.IsHoverScroll;
            }
        }
    }
}
