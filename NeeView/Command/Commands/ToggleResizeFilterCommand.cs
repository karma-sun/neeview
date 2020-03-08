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

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return PictureProfile.Current.IsResizeFilterEnabled ? Properties.Resources.CommandToggleResizeFilterOff : Properties.Resources.CommandToggleResizeFilterOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PictureProfile.Current.IsResizeFilterEnabled = !PictureProfile.Current.IsResizeFilterEnabled;
        }
    }
}
