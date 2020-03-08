using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeRandomCommand : CommandElement
    {
        public SetSortModeRandomCommand() : base("SetSortModeRandom")
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeRandom;
            this.Note = Properties.Resources.CommandSetSortModeRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Random);
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Random);
        }
    }
}
