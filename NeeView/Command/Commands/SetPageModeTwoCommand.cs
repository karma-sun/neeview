using System.Windows.Data;


namespace NeeView
{
    public class SetPageModeTwoCommand : CommandElement
    {
        public SetPageModeTwoCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetPageMode2;
            this.Note = Properties.Resources.CommandSetPageMode2Note;
            this.ShortCutKey = "Ctrl+2";
            this.MouseGesture = "RD";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.WidePage);
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.WidePage);
        }
    }
}
