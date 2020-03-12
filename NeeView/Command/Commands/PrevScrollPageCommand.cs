namespace NeeView
{
    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevScrollPage;
            this.Note = Properties.Resources.CommandPrevScrollPageNote;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = "NextScrollPage";

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.PrevScrollPage((ScrollPageCommandParameter)param);
        }
    }
}
