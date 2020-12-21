using NeeLaboratory;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScaleDown((ViewScaleCommandParameter)e.Parameter);
        }
    }

}
