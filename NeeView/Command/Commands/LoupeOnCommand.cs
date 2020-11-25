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

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponentProvider.Current.GetViewController(sender).SetLoupeMode(true);
        }
    }
}
