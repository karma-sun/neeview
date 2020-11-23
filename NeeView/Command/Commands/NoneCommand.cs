namespace NeeView
{
    public class NoneCommand : CommandElement
    {
        public NoneCommand() : base("")
        {
            this.Group = "(none)";
            this.Text = "(none)";
        }

        public override void Execute(object sender, CommandContext e)
        {
            return;
        }
    }
}
