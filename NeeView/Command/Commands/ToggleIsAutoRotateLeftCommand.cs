using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateLeftCommand : CommandElement
    {
        public ToggleIsAutoRotateLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsAutoRotateLeft;
            this.MenuText = Properties.Resources.CommandToggleIsAutoRotateLeftMenu;
            this.Note = Properties.Resources.CommandToggleIsAutoRotateLeftNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateLeft)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.IsAutoRotateLeft ? Properties.Resources.CommandToggleIsAutoRotateLeftOff : Properties.Resources.CommandToggleIsAutoRotateLeftOn;
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
                ContentCanvas.Current.IsAutoRotateLeft = Convert.ToBoolean(args[0]);
            }
            else
            {
                ContentCanvas.Current.IsAutoRotateLeft = !ContentCanvas.Current.IsAutoRotateLeft;
            }
        }
    }
}
