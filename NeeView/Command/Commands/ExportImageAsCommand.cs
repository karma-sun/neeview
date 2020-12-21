using NeeLaboratory;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ExportImageAsCommand : CommandElement
    {
        public ExportImageAsCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.ShortCutKey = "Ctrl+S";
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportImageAsCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.ExportDialog((ExportImageAsCommandParameter)e.Parameter);
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
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder;
            set => SetProperty(ref _exportFolder, value);
        }

        [DataMember]
        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, MathUtility.Clamp(value, 5, 100));
        }
    }
}
