namespace NeeView
{
    public class PrevOnePageCommand : CommandElement
    {
        public PrevOnePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.MouseGesture = "LR";
            this.IsShowMessage = false;
            this.PairPartner = "NextOnePage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.PrevOnePage(this);
        }
    }
}
