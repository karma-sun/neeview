﻿using NeeView.Windows.Property;
using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeUniformCommand : CommandElement
    {
        public SetStretchModeUniformCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.Uniform);
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return this.Text + (MainViewComponent.Current.ViewController.TestStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)e.Parameter).IsToggle) ? "" : " OFF");
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.SetStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)e.Parameter).IsToggle);
        }
    }


    /// <summary>
    /// スケールモード用設定
    /// </summary>
    public class StretchModeCommandParameter : CommandParameter
    {
        private bool _isToggle;

        // 属性に説明文
        [PropertyMember]
        public bool IsToggle
        {
            get => _isToggle;
            set => SetProperty(ref _isToggle , value);
        }
    }
}
