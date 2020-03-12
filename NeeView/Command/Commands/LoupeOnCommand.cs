namespace NeeView
{
    public class LoupeOnCommand : CommandElement
    {
        public LoupeOnCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeOn;
            this.Note = Properties.Resources.CommandLoupeOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MouseInput.Current.IsLoupeMode = true;
        }
    }
}
