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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.Export((ExportImageCommandParameter)e.Parameter);
        }
    }


    [DataContract]
    public class ExportImageCommandParameter : CommandParameter
    {
        private ExportImageMode _mode;
        private bool _hasBackground;
        private string _exportFolder;
        private ExportImageFileNameMode _fileNameMode;
        private ExportImageFormat _fileFormat;
        private int _qualityLevel = 80;

        [DataMember]
        [PropertyMember]
        public ExportImageMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        [DataMember]
        [PropertyMember]
        public bool HasBackground
        {
            get => _hasBackground;
            set => SetProperty(ref _hasBackground, value);
        }

        [DataMember]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string ExportFolder
        {
            get => _exportFolder;
            set => SetProperty(ref _exportFolder, value);
        }

        [DataMember]
        [PropertyMember]
        public ExportImageFileNameMode FileNameMode
        {
            get => _fileNameMode;
            set => SetProperty(ref _fileNameMode, value);
        }

        [DataMember]
        [PropertyMember]
        public ExportImageFormat FileFormat
        {
            get => _fileFormat;
            set => SetProperty(ref _fileFormat, value);
        }

        [DataMember]
        [PropertyRange(5, 100, TickFrequency = 5)]
        public int QualityLevel
        {
            get => _qualityLevel;
            set => SetProperty(ref _qualityLevel, value);
        }
    }
}
