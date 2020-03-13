using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleCustomSizeCommand : CommandElement
    {
        public ToggleCustomSizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleCustomSize;
            this.MenuText = Properties.Resources.CommandToggleCustomSizeMenu;
            this.Note = Properties.Resources.CommandToggleCustomSizeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PictureProfile.Current.CustomSize.IsEnabled)) { Mode = BindingMode.OneWay, Source = PictureProfile.Current.CustomSize };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return PictureProfile.Current.CustomSize.IsEnabled ? Properties.Resources.CommandToggleCustomSizeOff : Properties.Resources.CommandToggleCustomSizeOn;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                PictureProfile.Current.CustomSize.IsEnabled = Convert.ToBoolean(args[0]);
            }
            else
            {
                PictureProfile.Current.CustomSize.IsEnabled = !PictureProfile.Current.CustomSize.IsEnabled;
            }
        }
    }
}
