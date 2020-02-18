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
            dialog.FileName = _model.CreateFileName(ExportImageFileNameMode.Default);
            dialog.CanSelectFormat = _model.Mode == ExportImageMode.View;

            var result = dialog.ShowDialog(owner);
            if (result == true)
            {
                _model.Export(dialog.FileName, true);
            }

            return result;
        }
    }
}