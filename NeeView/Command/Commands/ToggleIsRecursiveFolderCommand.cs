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

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder ? Properties.Resources.CommandToggleIsRecursiveFolderOff : Properties.Resources.CommandToggleIsRecursiveFolderOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                BookSettingPresenter.Current.SetIsRecursiveFolder(Convert.ToBoolean(args[0]));
            }
            else
            {
                BookSettingPresenter.Current.ToggleIsRecursiveFolder();
            }
        }
    }
}
