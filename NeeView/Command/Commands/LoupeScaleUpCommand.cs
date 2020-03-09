namespace NeeView
{
    public class LoupeScaleUpCommand : CommandElement
    {
        public LoupeScaleUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleUp;
            this.Note = Properties.Resources.CommandLoupeScaleUpNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return MouseInput.Current.IsLoupeMode;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MouseInput.Current.Loupe.LoupeZoomIn();
        }
    }
}
