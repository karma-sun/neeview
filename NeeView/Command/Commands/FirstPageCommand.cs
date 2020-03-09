namespace NeeView
{
    public class FirstPageCommand : CommandElement
    {
        public FirstPageCommand(string name) : base(name)
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

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.FirstPage();
        }
    }
}
