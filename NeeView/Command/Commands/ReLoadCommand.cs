namespace NeeView
{
    public class ReLoadCommand : CommandElement
    {
        public ReLoadCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.MouseGesture = "UD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHub.Current.CanReload();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHub.Current.RequestReLoad(this);
        }
    }
}
