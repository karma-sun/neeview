namespace NeeView
{
    public class FirstPageCommand : CommandElement
    {
        public FirstPageCommand() : base("FirstPage")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandFirstPage;
            this.Note = Properties.Resources.CommandFirstPageNote;
            this.ShortCutKey = "Ctrl+Right";
            this.MouseGesture = "UR";
            this.IsShowMessage = true;
            this.PairPartner = "LastPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.FirstPage();
        }
    }
}
