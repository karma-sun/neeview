using NeeLaboratory;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ExportImageAsCommand : CommandElement
    {
        public ExportImageAsCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImageDialog;
            this.MenuText = Properties.Resources.CommandExportImageDialogMenu;
            this.Note = Properties.Resources.CommandExportImageDialogNote;
            this.ShortCutKey = "Ctrl+S";
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportImageAsCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.ExportDialog((ExportImageAsCommandParameter)param);
        }
    }


    /// <summary>
    /// ExportImageAs Command Parameter
    /// </summary>
    [DataContract]
    public class ExportImageAsCommandParameter : CommandParameter
    {
        private string _exportFolder;
        private int _qualityLevel = 80;

        [DataMember]
        [PropertyPath("@ParamCommandParameterExportDefaultFolder", Tips = "@ParamCommandParameterExportDefaultFolderTips", FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder;
            set => SetProperty(ref _exportFolder, value);
        }

        [DataMember]
        [PropertyRange("@ParamCommandParameterExportImageQualityLevel", 5, 100, TickFrequency = 5, Tips = "@ParamCommandParameterExportImageQualityLevelTips")]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, MathUtility.Clamp(value, 5, 100));
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ExportImageAsCommandParameter;
            if (target == null) return false;
            return this == target || (this.ExportFolder == target.ExportFolder && this.QualityLevel == target.QualityLevel);
        }
    }
}
