namespace NeeView
{
    public class NextOnePageCommand : CommandElement
    {
        public NextOnePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextOnePage;
            this.Note = Properties.Resources.CommandNextOnePageNote;
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
