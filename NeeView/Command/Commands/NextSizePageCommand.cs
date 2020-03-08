namespace NeeView
{
    public class NextSizePageCommand : CommandElement
    {
        public NextSizePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextSizePage;
            this.Note = Properties.Resources.CommandNextSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = "PrevSizePage";

            // PrevSizePage
            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter() { Size = 10 });
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }
}
