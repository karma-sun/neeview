namespace NeeView
{
    public class LastPageCommand : CommandElement
    {
        public LastPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandLastPage;
            this.Note = Properties.Resources.CommandLastPageNote;
            this.ShortCutKey = "Ctrl+Left";
            this.MouseGesture = "UL";
            this.IsShowMessage = true;
            this.PairPartner = "FirstPage";

            // FirstPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.LastPage(this);
        }
    }
}
