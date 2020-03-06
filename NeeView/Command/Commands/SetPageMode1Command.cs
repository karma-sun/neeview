using System.Windows.Data;


namespace NeeView
{
    public class SetPageMode1Command : CommandElement
    {
        public SetPageMode1Command() : base(CommandType.SetPageMode1)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetPageMode1;
            this.Note = Properties.Resources.CommandSetPageMode1Note;
            this.ShortCutKey = "Ctrl+1";
            this.MouseGesture = "RU";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.SinglePage);
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.SinglePage);
        }
    }
}
