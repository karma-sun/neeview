using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ExportImageWindowViewModel : BindableBase
    {
        private ExportImage _model;

        public ExportImageWindowViewModel(ExportImage model)
        {
            _model = model;
            _model.PropertyChanged += Model_PropertyChanged;

            UpdateDestinationFolderList();
        }


        public Dictionary<ExportImageMode, string> ExportImageModeList => AliasNameExtensions.GetAliasNameDictionary<ExportImageMode>();

        public ExportImageMode Mode
        {
            get { return _model.Mode; }
            set { _model.Mode = value; }
        }

        public bool HasBackground
        {
            get { return _model.HasBackground; }
            set { _model.HasBackground = value; }
        }

        public FrameworkElement Preview
        {
            get { return _model.Preview; }
        }

        public string ImageFormatNote
        {
            get { return _model.ImageFormatNote; }
        }


        private List<DestinationFolder> _destinationFolderList;
        public List<DestinationFolder> DestinationFolderList
        {
            get { return _destinationFolderList; }
            set { SetProperty(ref _destinationFolderList, value); }
        }


        private static DestinationFolder _lastSelectedDestinationFolder;
        private DestinationFolder _selectedDestinationFolder = _lastSelectedDestinationFolder;
        public DestinationFolder SelectedDestinationFolder
        {
            get { return _selectedDestinationFolder; }
            set
            {
                if (SetProperty(ref _selectedDestinationFolder, value))
                {
                    _lastSelectedDestinationFolder = _selectedDestinationFolder;
                }
            }
        }


        public void UpdateDestinationFolderList()
        {
            var oldSelect = _selectedDestinationFolder;

            var list = new List<DestinationFolder>();
            list.Add(new DestinationFolder(Properties.Resources.WordNone, ""));
            list.AddRange(Config.Current.System.DestinationFodlerCollection);
            DestinationFolderList = list;

            SelectedDestinationFolder = list.FirstOrDefault(e => e.Equals(oldSelect)) ?? list.First();
        }


        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.Mode):
                    RaisePropertyChanged(nameof(Mode));
                    break;

                case nameof(_model.HasBackground):
                    RaisePropertyChanged(nameof(HasBackground));
                    break;

                case nameof(_model.Preview):
                    RaisePropertyChanged(nameof(Preview));
                    break;

                case nameof(_model.ImageFormatNote):
                    RaisePropertyChanged(nameof(ImageFormatNote));
                    break;
            }
        }

        public void UpdatePreview()
        {
            _model.UpdatePreview();
        }

        public bool? ShowSelectSaveFileDialog(Window owner)
        {
            var dialog = new ExportImageSeveFileDialog();
            dialog.InitialDirectory = _model.ExportFolder;
            dialog.FileName = _model.CreateFileName(ExportImageFileNameMode.Default, ExportImageFormat.Png);
            dialog.CanSelectFormat = _model.Mode == ExportImageMode.View;

            if (SelectedDestinationFolder != null && SelectedDestinationFolder.IsValid())
            {
                dialog.InitialDirectory = SelectedDestinationFolder.Path;
            }

            var result = dialog.ShowDialog(owner);
            if (result == true)
            {
                _model.Export(dialog.FileName, true);
            }

            return result;
        }
    }
}