using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsRecursiveFolderCommand : CommandElement
    {
        public ToggleIsRecursiveFolderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsRecursiveFolder;
            this.Note = Properties.Resources.CommandToggleIsRecursiveFolderNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder));
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder ? Properties.Resources.CommandToggleIsRecursiveFolderOff : Properties.Resources.CommandToggleIsRecursiveFolderOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsRecursiveFolder(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsRecursiveFolder();
            }
        }
    }
}
