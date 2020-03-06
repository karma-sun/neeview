namespace NeeView
{
    public class LoupeOnCommand : CommandElement
    {
        public LoupeOnCommand() : base(CommandType.LoupeOn)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeOn;
            this.Note = Properties.Resources.CommandLoupeOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.IsLoupeMode = true;
        }
    }
}
