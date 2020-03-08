namespace NeeView
{
    public class LoupeScaleUpCommand : CommandElement
    {
        public LoupeScaleUpCommand() : base("LoupeScaleUp")
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleUp;
            this.Note = Properties.Resources.CommandLoupeScaleUpNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MouseInput.Current.IsLoupeMode;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.Loupe.LoupeZoomIn();
        }
    }
}
