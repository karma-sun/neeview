namespace NeeView
{
    public class ViewScrollDownCommand : CommandElement
    {
        public ViewScrollDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollDown;
            this.Note = Properties.Resources.CommandViewScrollDownNote;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            DragTransformControl.Current.ScrollDown((ViewScrollCommandParameter)param);
        }
    }
}
