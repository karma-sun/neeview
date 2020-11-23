using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedWidePageCommand : CommandElement
    {
        public ToggleIsSupportedWidePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedWidePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedWidePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage));
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage ? Properties.Resources.CommandToggleIsSupportedWidePageOff : Properties.Resources.CommandToggleIsSupportedWidePageOn;
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
                BookSettingPresenter.Current.SetIsSupportedWidePage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedWidePage();
            }
        }
    }
}
