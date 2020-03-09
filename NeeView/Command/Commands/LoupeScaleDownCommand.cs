namespace NeeView
{
    public class LoupeScaleDownCommand : CommandElement
    {
        public LoupeScaleDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleDown;
            this.Note = Properties.Resources.CommandLoupeScaleDownNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return MouseInput.Current.IsLoupeMode;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MouseInput.Current.Loupe.LoupeZoomOut();
        }
    }
}
