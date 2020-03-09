using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByRandomCommand : CommandElement
    {
        public SetFolderOrderByRandomCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByRandom;
            this.Note = Properties.Resources.CommandSetFolderOrderByRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Random);
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Random);
        }
    }
}
