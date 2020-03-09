using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsEnabledNearestNeighborCommand : CommandElement
    {
        public ToggleIsEnabledNearestNeighborCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleIsEnabledNearestNeighbor;
            this.MenuText = Properties.Resources.CommandToggleIsEnabledNearestNeighborMenu;
            this.Note = Properties.Resources.CommandToggleIsEnabledNearestNeighborNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.IsEnabledNearestNeighbor)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return ContentCanvas.Current.IsEnabledNearestNeighbor ? Properties.Resources.CommandToggleIsEnabledNearestNeighborOff : Properties.Resources.CommandToggleIsEnabledNearestNeighborOn;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ContentCanvas.Current.IsEnabledNearestNeighbor = !ContentCanvas.Current.IsEnabledNearestNeighbor;
        }
    }
}
