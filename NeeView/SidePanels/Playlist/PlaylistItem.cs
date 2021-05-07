using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using NeeView.IO;
using System;

namespace NeeView
{
    public class PlaylistItem : BindableBase, IHasPage, IHasName
    {
        private PlaylistSourceItem _item;
        private string _place;
        private Page _archivePage;
        private bool? _isArchive;


        public PlaylistItem(string path)
        {
            _item = new PlaylistSourceItem(path);
        }

        public PlaylistItem(string path, string name)
        {
            _item = new PlaylistSourceItem(path, name);
        }

        public PlaylistItem(PlaylistSourceItem item)
        {
            _item = new PlaylistSourceItem(item.Path, item.Name);
        }


        public string Path
        {
            get { return _item.Path; }
            private set
            {
                if (_item.Path != value)
                {
                    _item.Path = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public string Name
        {
            get { return _item.Name; }
            set 
            {
                var oldName = _item.Name;
                _item.Name = value;
                if (_item.Name != oldName)
                {
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsNameChanged => _item.IsNameChanged;

        public string Place
        {
            get
            {
                if (_place is null)
                {
                    if (FileIO.Exists(Path))
                    {
                        _place = LoosePath.GetDirectoryName(Path);
                    }
                    else
                    {
                        _place = ArchiveEntryUtility.GetExistEntryName(Path);
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
            get
            {
                if (_isArchive is null)
                {
                    var targetPath = Path;
                    if (FileShortcut.IsShortcut(Path))
                    {
                        targetPath = new FileShortcut(Path).TargetPath ?? Path;
                    }
                    _isArchive = ArchiverManager.Current.IsSupported(targetPath) || System.IO.Directory.Exists(targetPath);
                }
                return _isArchive.Value;
            }
        }

        public Page ArchivePage
        {
            get
            {
                if (_archivePage == null)
                {
                    _archivePage = new Page("", new ArchiveContent(Path));
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

        public PlaylistSourceItem ToPlaylistItem()
        {
            return new PlaylistSourceItem(Path, Name);
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
