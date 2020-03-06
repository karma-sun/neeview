using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByPathACommand : CommandElement
    {
        public SetFolderOrderByPathACommand() : base(CommandType.SetFolderOrderByPathA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByPathA;
            this.Note = Properties.Resources.CommandSetFolderOrderByPathANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Path);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Path);
        }
    }
}
