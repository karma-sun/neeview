using System;

namespace NeeView
{
    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandJumpPage;
            this.Note = Properties.Resources.CommandJumpPageNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandJumpPageArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                var number = Convert.ToInt32(args[0]);
                BookOperation.Current.JumpPage(this, number);
            }
            else
            {
                BookOperation.Current.JumpPageAs(this);
            }
        }
    }
}
