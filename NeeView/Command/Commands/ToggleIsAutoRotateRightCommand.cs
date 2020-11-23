using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateRightCommand : CommandElement
    {
        public ToggleIsAutoRotateRightCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsAutoRotateRight;
            this.MenuText = Properties.Resources.CommandToggleIsAutoRotateRightMenu;
            this.Note = Properties.Resources.CommandToggleIsAutoRotateRightNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateRight)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return ContentCanvas.Current.IsAutoRotateRight ? Properties.Resources.CommandToggleIsAutoRotateRightOff : Properties.Resources.CommandToggleIsAutoRotateRightOn;
        }
        
        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ContentCanvas.Current.IsAutoRotateRight = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                ContentCanvas.Current.IsAutoRotateRight = !ContentCanvas.Current.IsAutoRotateRight;
            }
        }
    }
}
