namespace NeeView
{
    public class NextScrollPageCommand : CommandElement
    {
        public NextScrollPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextScrollPage;
            this.Note = Properties.Resources.CommandNextScrollPageNote;
            this.ShortCutKey = "WheelDown";
            this.IsShowMessage = false;
            this.PairPartner = "PrevScrollPage";

            // PrevScrollPage
            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.NextScrollPage((ScrollPageCommandParameter)param);
        }
    }
}
