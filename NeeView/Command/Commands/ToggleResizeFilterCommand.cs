using System.Windows.Data;


namespace NeeView
{
    public class ToggleResizeFilterCommand : CommandElement
    {
        public ToggleResizeFilterCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleResizeFilter;
            this.MenuText = Properties.Resources.CommandToggleResizeFilterMenu;
            this.Note = Properties.Resources.CommandToggleResizeFilterNote;
            this.ShortCutKey = "Ctrl+R";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PictureProfile.Current.IsResizeFilterEnabled)) { Mode = BindingMode.OneWay, Source = PictureProfile.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return PictureProfile.Current.IsResizeFilterEnabled ? Properties.Resources.CommandToggleResizeFilterOff : Properties.Resources.CommandToggleResizeFilterOn;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            PictureProfile.Current.IsResizeFilterEnabled = !PictureProfile.Current.IsResizeFilterEnabled;
        }
    }
}
