namespace NeeView
{
    public class NoneCommand : CommandElement
    {
        public NoneCommand() : base(CommandType.None)
        {
            this.Group = "(none)";
            this.Text = "(none)";
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return;
        }
    }
}
