using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileNameDCommand : CommandElement
    {
        public SetBookOrderByFileNameDCommand(string name) : base(name)
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

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileNameDescending);
        }
    }
}
