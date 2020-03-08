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

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.IsAutoRotateRight ? Properties.Resources.CommandToggleIsAutoRotateRightOff : Properties.Resources.CommandToggleIsAutoRotateRightOn;
        }
        
        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.IsAutoRotateRight = !ContentCanvas.Current.IsAutoRotateRight;
        }
    }
}
