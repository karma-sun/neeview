using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleLastPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleLastPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleLastPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleLastPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage));
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage ? Properties.Resources.CommandToggleIsSupportedSingleLastPageOff : Properties.Resources.CommandToggleIsSupportedSingleLastPageOn;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsSupportedSingleLastPage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedSingleLastPage();
            }
        }
    }
}
