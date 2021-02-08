using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FileInformationViewModel : BindableBase
    {
        private FileInformation _model;
        private FileInformationSource _selectedItem;


        public FileInformationViewModel(FileInformation model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.FileInformations),
                Model_FileInformationsChanged);

            Config.Current.Information.AddPropertyChanged(nameof(InformationConfig.IsVisibleLoader),
                (s, e) => RaisePropertyChanged(nameof(LoaderVisibility)));

            Config.Current.Information.AddPropertyChanged(nameof(InformationConfig.IsVisibleBitsPerPixel),
                (s, e) => _model.Update());

            this.OpenPlace = new RelayCommand(OpenPlace_Execute);
        }


        public Visibility LoaderVisibility
        {
            get { return Config.Current.Information.IsVisibleLoader ? Visibility.Visible : Visibility.Collapsed; }
        }

        public List<FileInformationSource> FileInformations
        {
            get { return _model.FileInformations; }
        }

        public FileInformationSource SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public RelayCommand OpenPlace { get; private set; }


        private void Model_FileInformationsChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FileInformations));

            if (SelectedItem is null)
            {
                SelectedItem = _model.FileInformations.FirstOrDefault();
            }
        }

        private void OpenPlace_Execute()
        {
            if (SelectedItem is null) return;

            var place = SelectedItem.ViewContent.Page?.GetFolderOpenPlace();
            if (!string.IsNullOrWhiteSpace(place))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + place + "\"");
            }
        }

    }
}
