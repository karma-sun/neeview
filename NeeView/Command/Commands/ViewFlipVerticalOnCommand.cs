namespace NeeView
{
    public class ViewFlipVerticalOnCommand : CommandElement
    {
        public ViewFlipVerticalOnCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipVerticalOn;
            this.Note = Properties.Resources.CommandViewFlipVerticalOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewControlMediator.Current.FlipVertical(sender, true);
        }
    }
}
