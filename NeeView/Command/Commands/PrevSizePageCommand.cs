namespace NeeView
{
    public class PrevSizePageCommand : CommandElement
    {
        public PrevSizePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevSizePage;
            this.Note = Properties.Resources.CommandPrevSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = "NextSizePage";

            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter() { Size = 10 });
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.PrevSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }
}
