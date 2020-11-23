using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleFirstPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleFirstPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleFirstPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage));
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage ? Properties.Resources.CommandToggleIsSupportedSingleFirstPageOff : Properties.Resources.CommandToggleIsSupportedSingleFirstPageOn;
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
                BookSettingPresenter.Current.SetIsSupportedSingleFirstPage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedSingleFirstPage();
            }
        }
    }
}
