using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Media.Imaging.Metadata;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

            InitializeMoreMenu();
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


        private void Model_FileInformationsChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FileInformations));

            if (SelectedItem is null)
            {
                SelectedItem = _model.GetMainFileInformation();
            }
        }



        #region MoreMenu

        private ContextMenu _moreMenu;

        public ContextMenu MoreMenu
        {
            get { return _moreMenu; }
            set { if (_moreMenu != value) { _moreMenu = value; RaisePropertyChanged(); } }
        }

        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_File, new Binding(nameof(InformationConfig.IsVisibleFileSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_Image, new Binding(nameof(InformationConfig.IsVisibleImageSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_Description, new Binding(nameof(InformationConfig.IsVisibleDescriptionSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_Origin, new Binding(nameof(InformationConfig.IsVisibleOriginSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_Camera, new Binding(nameof(InformationConfig.IsVisibleCameraSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_AdvancedPhoto, new Binding(nameof(InformationConfig.IsVisibleAdvancedPhotoSection)) { Source = Config.Current.Information }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Information_Gps, new Binding(nameof(InformationConfig.IsVisibleGpsSection)) { Source = Config.Current.Information }));
            this.MoreMenu = menu;
        }

        private MenuItem CreateCheckMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        #endregion
    }
}
