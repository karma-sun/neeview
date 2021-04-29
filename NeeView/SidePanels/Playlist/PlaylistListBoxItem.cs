using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;

namespace NeeView
{
    public class PlaylistListBoxItem : BindableBase, IHasPage, IHasName
    {
        public PlaylistListBoxItem(string path)
        {
            Path = path;
        }

        public PlaylistListBoxItem(PlaylistItem item)
        {
            Path = item.Path;
            Name = item.Name;
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            private set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(nameof(DispName));
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                var newName = (string.IsNullOrWhiteSpace(value) || value == LoosePath.GetFileName(Path)) ? null : value;
                if (SetProperty(ref _name, newName))
                {
                    RaisePropertyChanged(nameof(DispName));
                }
            }
        }

        public string DispName
        {
            get { return _name ?? LoosePath.GetFileName(_path); }
        }


        private string _place;
        public string Place
        {
            get
            {
                if (_place is null)
                {
                    if (FileIO.Exists(_path))
                    {
                        _place = LoosePath.GetDirectoryName(_path);
                    }
                    else
                    {
                        _place = ArchiveEntryUtility.GetExistEntryName(_path);
                    }
                }
                return _place;
            }
        }

        public string DispPlace
        {
            get { return SidePanelProfile.GetDecoratePlaceName(Place); }
        }

        public bool IsArchive
        {
            get { return ArchiverManager.Current.IsSupported(_path) || System.IO.Directory.Exists(_path); }
        }


        private Page _archivePage;

        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page("", new ArchiveContent(_path));
                    _archivePage.Thumbnail.IsCacheEnabled = true;
                    _archivePage.Thumbnail.Touched += Thumbnail_Touched;
                }
                return _archivePage;
            }
        }

        private void Thumbnail_Touched(object sender, EventArgs e)
        {
            var thumbnail = (Thumbnail)sender;
            BookThumbnailPool.Current.Add(thumbnail);
        }

        public void UpdateDispPlace()
        {
            RaisePropertyChanged(nameof(DispPlace));
        }

        public Page GetPage()
        {
            return ArchivePage;
        }

        public PlaylistItem ToPlaylistItem()
        {
            return new PlaylistItem(Path, Name);
        }

        public override string ToString()
        {
            return DispName ?? base.ToString();
        }
    }
}
