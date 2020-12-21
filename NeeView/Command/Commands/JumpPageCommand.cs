using System;

namespace NeeView
{
    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                var number = Convert.ToInt32(e.Args[0]);
                BookOperation.Current.JumpPage(this, number);
            }
            else
            {
                BookOperation.Current.JumpPageAs(this);
            }
        }
    }
}
