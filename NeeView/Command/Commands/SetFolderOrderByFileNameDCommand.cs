using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileNameDCommand : CommandElement
    {
        public SetFolderOrderByFileNameDCommand() : base(CommandType.SetFolderOrderByFileNameD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileNameD;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileNameDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileNameDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileNameDescending);
        }
    }
}
