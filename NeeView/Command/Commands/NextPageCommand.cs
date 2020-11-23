namespace NeeView
{
    public class NextPageCommand : CommandElement
    {
        public NextPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextPage;
            this.Note = Properties.Resources.CommandNextPageNote;
            this.ShortCutKey = "Left,LeftClick";
            this.TouchGesture = "TouchL1,TouchL2";
            this.MouseGesture = "L";
            this.IsShowMessage = false;
            this.PairPartner = "PrevPage";

            // PrevPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextPage(this);
        }
    }
}
