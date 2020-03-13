﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedDividePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedDividePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage));
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage ? Properties.Resources.CommandToggleIsSupportedDividePageOff : Properties.Resources.CommandToggleIsSupportedDividePageOn;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsSupportedDividePage(Convert.ToBoolean(args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsSupportedDividePage();
            }
        }
    }
}