using NeeLaboratory;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleDown;
            this.Note = Properties.Resources.CommandViewScaleDownNote;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            var parameter = (ViewScaleCommandParameter)e.Parameter;
            DragTransformControl.Current.ScaleDown(parameter.Scale, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }

}
