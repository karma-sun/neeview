using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateLeftCommand : CommandElement
    {
        public ToggleIsAutoRotateLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsAutoRotateLeft;
            this.MenuText = Properties.Resources.CommandToggleIsAutoRotateLeftMenu;
            this.Note = Properties.Resources.CommandToggleIsAutoRotateLeftNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateLeft)) { Source = ViewComponentProvider.Current.GetViewComponent().ContentCanvas };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return ViewComponentProvider.Current.GetViewController(sender).GetAutoRotateLeft() ? Properties.Resources.CommandToggleIsAutoRotateLeftOff : Properties.Resources.CommandToggleIsAutoRotateLeftOn;
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
                ViewComponentProvider.Current.GetViewController(sender).SetAutoRotateLeft(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ViewComponentProvider.Current.GetViewController(sender).ToggleAutoRotateLeft();
            }
        }
    }
}
