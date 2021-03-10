using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    public class FileInformationContentViewModel : BindableBase
    {
        private Dictionary<InformationKey, FileInformationRecord> _database;
        private FileInformationSource _source;
        private CollectionViewSource _collectionViewSource;
        private FileInformationRecord _selectedItem;
        private bool _IsVisibleImage;
        private bool _isVisibleMetadata;

        public FileInformationContentViewModel()
        {
            _database = FileInformationSource.CreatePropertiesTemplate().ToDictionary(e => e.Key, e => e);

            _collectionViewSource = new CollectionViewSource();
            _collectionViewSource.Source = _database.Values;
            _collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FileInformationRecord.Group)) { Converter = new EnumToAliasNameConverter() });
            _collectionViewSource.Filter += CollectionViewSource_Filter;

            Config.Current.Information.PropertyChanged += Information_PropertyChanged;
        }


        public FileInformationSource Source
        {
            get { return _source; }
            set
            {
                if (SetProperty(ref _source, value))
                {
                    UpdateDatabase();
                }
            }
        }

        public CollectionViewSource CollectionViewSource
        {
            get { return _collectionViewSource; }
            set { SetProperty(ref _collectionViewSource, value); }
        }

        public FileInformationRecord SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public bool IsVisibleImage
        {
            get { return _IsVisibleImage; }
            set
            {
                if (SetProperty(ref _IsVisibleImage, value))
                {
                    UpdateFilter();
                }
            }
        }

        public bool IsVisibleMetadata
        {
            get { return _isVisibleMetadata; }
            set
            {
                if (SetProperty(ref _isVisibleMetadata, value))
                {
                    UpdateFilter();
                }
            }
        }


        private void Information_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case nameof(InformationConfig.IsVisibleFile):
                case nameof(InformationConfig.IsVisibleImage):
                case nameof(InformationConfig.IsVisibleDescription):
                case nameof(InformationConfig.IsVisibleOrigin):
                case nameof(InformationConfig.IsVisibleCamera):
                case nameof(InformationConfig.IsVisibleAdvancedPhoto):
                case nameof(InformationConfig.IsVisibleGps):
                    UpdateFilter();
                    break;
                case nameof(InformationConfig.DateTimeFormat):
                    UpdateFilter();
                    break;
            }
        }

        private void UpdateDatabase()
        {
            if (_source != null)
            {
                foreach (var item in _source.Properties)
                {
                    _database[item.Key].Value = item.Value;
                }

                IsVisibleImage = _source.PictureInfo != null;
                IsVisibleMetadata = _source.Metadata != null;
            }
        }

        private void UpdateFilter()
        {
            var selectedItem = SelectedItem;
            _collectionViewSource.View?.Refresh();
            SelectedItem = selectedItem;
        }


        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is FileInformationRecord record)
            {
                var category = record.Group.ToInformationCategory();
                if (category == InformationCategory.Image && !IsVisibleImage)
                {
                    e.Accepted = false;
                }
                else if (category == InformationCategory.Metadata && !IsVisibleMetadata)
                {
                    e.Accepted = false;
                }
                else
                {
                    e.Accepted = Config.Current.Information.IsVisibleGroup(record.Group);
                }
            }
            else
            {
                e.Accepted = false;
            }
        }

    }
}
