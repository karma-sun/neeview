using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowReduceCommand : CommandElement
    {
        public ToggleStretchAllowReduceCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchAllowReduce;
            this.Note = Properties.Resources.CommandToggleStretchAllowReduceNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.AllowReduce)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return this.Text + (ContentCanvas.Current.AllowReduce ? " OFF" : "");
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
                ContentCanvas.Current.AllowReduce = Convert.ToBoolean(args[0]);
            }
            else
            {
                ContentCanvas.Current.AllowReduce = !ContentCanvas.Current.AllowReduce;
            }
        }
    }
}
