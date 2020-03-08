namespace NeeView
{
    public class PrevPageCommand : CommandElement
    {
        public PrevPageCommand() : base("PrevPage")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevPage;
            this.Note = Properties.Resources.CommandPrevPageNote;
            this.ShortCutKey = "Right,RightClick";
            this.TouchGesture = "TouchR1,TouchR2";
            this.MouseGesture = "R";
            this.IsShowMessage = false;
            this.PairPartner = "NextPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevPage();
        }
    }
}
