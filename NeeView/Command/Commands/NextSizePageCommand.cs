using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NextSizePageCommand : CommandElement
    {
        public NextSizePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.IsShowMessage = false;
            this.PairPartner = "PrevSizePage";

            // PrevSizePage
            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextSizePage(this, ((MoveSizePageCommandParameter)e.Parameter).Size);
        }
    }

}
