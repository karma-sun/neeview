namespace NeeView
{
    public class ViewRotateRightCommand : CommandElement
    {
        public ViewRotateRightCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateRight;
            this.Note = Properties.Resources.CommandViewRotateRightNote;
            this.IsShowMessage = false;
            
            // ViewRotateLeft
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter() { Angle = 45 });
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.ViewRotateRight((ViewRotateCommandParameter)param);
        }
    }
}
