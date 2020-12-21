namespace NeeView
{
    public class NextOnePageCommand : CommandElement
    {
        public NextOnePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.MouseGesture = "RL";
            this.IsShowMessage = false;
            this.PairPartner = "PrevOnePage";

            // PrevOnePage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextOnePage(this);
        }
    }
}
