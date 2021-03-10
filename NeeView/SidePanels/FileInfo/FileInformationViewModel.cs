using NeeLaboratory;
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

            MoreMenuDescription = new FileInformationMoreMenuDescription();
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


        #region MoreMenu

        public FileInformationMoreMenuDescription MoreMenuDescription { get; }

        public class FileInformationMoreMenuDescription : MoreMenuDescription
        {
            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_File, new Binding(nameof(InformationConfig.IsVisibleFile)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_Image, new Binding(nameof(InformationConfig.IsVisibleImage)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_Description, new Binding(nameof(InformationConfig.IsVisibleDescription)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_Origin, new Binding(nameof(InformationConfig.IsVisibleOrigin)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_Camera, new Binding(nameof(InformationConfig.IsVisibleCamera)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_AdvancedPhoto, new Binding(nameof(InformationConfig.IsVisibleAdvancedPhoto)) { Source = Config.Current.Information }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.InformationGroup_Gps, new Binding(nameof(InformationConfig.IsVisibleGps)) { Source = Config.Current.Information }));
                return menu;
            }
        }

        #endregion MoreMenu


        private void Model_FileInformationsChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(FileInformations));

            if (SelectedItem is null)
            {
                SelectedItem = _model.GetMainFileInformation();
            }
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled;
        }

        public void MoveSelectedItem(int delta)
        {
            if (FileInformations is null) return;

            var index = SelectedItem is null ? 0 : FileInformations.IndexOf(SelectedItem);
            index = MathUtility.Clamp(index + delta, 0, FileInformations.Count - 1);
            if (index >= 0)
            {
                SelectedItem = FileInformations[index];
            }
        }
    }
}
