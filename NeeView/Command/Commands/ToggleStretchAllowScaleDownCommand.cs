using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleStretchAllowScaleDownCommand : CommandElement
    {
        public ToggleStretchAllowScaleDownCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ViewConfig.AllowStretchScaleDown)) { Source = Config.Current.View };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return this.Text + (Config.Current.View.AllowStretchScaleDown ? " OFF" : "");
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.View.AllowStretchScaleDown = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.View.AllowStretchScaleDown = !Config.Current.View.AllowStretchScaleDown;
            }
        }
    }
}
