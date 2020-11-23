using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByRandomCommand : CommandElement
    {
        public SetBookOrderByRandomCommand(string name) : base(name)
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

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Random);
        }
    }
}
