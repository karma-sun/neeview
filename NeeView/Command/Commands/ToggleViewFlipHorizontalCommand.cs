using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipHorizontal)) { Source = MainViewComponent.Current.DragTransform, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewController.FlipHorizontal(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewController.ToggleFlipHorizontal();
            }
        }
    }
}
