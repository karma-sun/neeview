namespace NeeView
{
    public class PrevOnePageCommand : CommandElement
    {
        public PrevOnePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevOnePage;
            this.Note = Properties.Resources.CommandPrevOnePageNote;
            this.MouseGesture = "LR";
            this.IsShowMessage = false;
            this.PairPartner = "NextOnePage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.PrevOnePage();
        }
    }
}
