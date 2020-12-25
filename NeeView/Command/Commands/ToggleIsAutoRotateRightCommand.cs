using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateRightCommand : CommandElement
    {
        public ToggleIsAutoRotateRightCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateRight)) { Source = MainViewComponent.Current.ContentCanvas };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetAutoRotateRight() ? Properties.Resources.ToggleIsAutoRotateRightCommand_Off : Properties.Resources.ToggleIsAutoRotateRightCommand_On;
        }
        
        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewController.SetAutoRotateRight(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewController.ToggleAutoRotateRight();
            }
        }
    }
}
