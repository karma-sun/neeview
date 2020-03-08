namespace NeeView
{
    public class LoupeOffCommand : CommandElement
    {
        public LoupeOffCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeOff;
            this.Note = Properties.Resources.CommandLoupeOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.IsLoupeMode = false;
        }
    }
}
