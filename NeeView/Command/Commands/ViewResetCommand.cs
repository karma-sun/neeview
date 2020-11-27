namespace NeeView
{
    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewReset;
            this.Note = Properties.Resources.CommandViewResetNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ResetContentSizeAndTransform();
        }
    }
}
