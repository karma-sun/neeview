namespace NeeView
{
    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollUp;
            this.Note = Properties.Resources.CommandViewScrollUpNote;
            this.IsShowMessage = false;
            
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            DragTransformControl.Current.ScrollUp((ViewScrollCommandParameter)param);
        }
    }
}
