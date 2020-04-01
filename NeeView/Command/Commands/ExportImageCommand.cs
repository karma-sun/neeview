using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ExportImageCommand : CommandElement
    {
        public ExportImageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImage;
            this.MenuText = Properties.Resources.CommandExportImageMenu;
            this.Note = Properties.Resources.CommandExportImageNote;
            this.ShortCutKey = "Shift+Ctrl+S";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ExportImageCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.Export((ExportImageCommandParameter)param);
        }
    }


    [DataContract]
    public class ExportImageCommandParameter : CommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterExportMode")]
        public ExportImageMode Mode { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterExportHasBackground")]
        public bool HasBackground { get; set; }

        [DataMember]
        [PropertyPath("@ParamCommandParameterExportFolder", FileDialogType = FileDialogType.Directory)]
        public string ExportFolder { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterExportFileNameMode")]
        public ExportImageFileNameMode FileNameMode { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterExportFileFormat", Tips = "@ParamCommandParameterExportFileFormat")]
        public ExportImageFormat FileFormat { get; set; }

        [DataMember]
        [PropertyRange("@ParamCommandParameterExportImageQualityLevel", 5, 100, TickFrequency = 5, Tips = "@ParamCommandParameterExportImageQualityLevelTips")]
        public int QualityLevel { get; set; } = 80;

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ExportImageCommandParameter;
            if (target == null) return false;
            return this == target || (this.Mode == target.Mode &&
                this.HasBackground == target.HasBackground &&
                this.ExportFolder == target.ExportFolder &&
                this.FileNameMode == target.FileNameMode &&
                this.FileFormat == target.FileFormat &&
                this.QualityLevel == target.QualityLevel);
        }
    }
}
