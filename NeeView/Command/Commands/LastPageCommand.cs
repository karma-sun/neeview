namespace NeeView
{
    public class LastPageCommand : CommandElement
    {
        public LastPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "Ctrl+Left";
            this.MouseGesture = "UL";
            this.IsShowMessage = true;
            this.PairPartner = "FirstPage";

            // FirstPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.LastPage(this);
        }
    }
}
