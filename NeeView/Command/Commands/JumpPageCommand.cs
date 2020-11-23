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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@CommandJumpPageArgument")]
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
