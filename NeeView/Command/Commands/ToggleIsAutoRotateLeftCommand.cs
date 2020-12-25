using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateLeftCommand : CommandElement
    {
        public ToggleIsAutoRotateLeftCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateLeft)) { Source = MainViewComponent.Current.ContentCanvas };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetAutoRotateLeft() ? Properties.Resources.ToggleIsAutoRotateLeftCommand_Off : Properties.Resources.ToggleIsAutoRotateLeftCommand_On;
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
                MainViewComponent.Current.ViewController.SetAutoRotateLeft(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewController.ToggleAutoRotateLeft();
            }
        }
    }
}
