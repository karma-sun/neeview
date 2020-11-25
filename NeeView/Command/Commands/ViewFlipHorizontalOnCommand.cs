namespace NeeView
{
    public class ViewFlipHorizontalOnCommand : CommandElement
    {
        public ViewFlipHorizontalOnCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipHorizontalOn;
            this.Note = Properties.Resources.CommandViewFlipHorizontalOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponentProvider.Current.GetViewController(sender).FlipHorizontal(true);
        }
    }
}
