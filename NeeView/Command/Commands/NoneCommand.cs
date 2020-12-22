namespace NeeView
{
    public class NoneCommand : CommandElement
    {
        public NoneCommand() : base("")
        {
            this.Group = Properties.Resources.CommandGroup_None;
        }

        public override void Execute(object sender, CommandContext e)
        {
            return;
        }
    }
}
