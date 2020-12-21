namespace NeeView
{
    public class PrevPageCommand : CommandElement
    {
        public PrevPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "Right,RightClick";
            this.TouchGesture = "TouchR1,TouchR2";
            this.MouseGesture = "R";
            this.IsShowMessage = false;
            this.PairPartner = "NextPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.PrevPage(this);
        }
    }
}
