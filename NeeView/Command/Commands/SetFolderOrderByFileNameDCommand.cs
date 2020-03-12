using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileNameDCommand : CommandElement
    {
        public SetFolderOrderByFileNameDCommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileNameDescending);
        }
    }
}
