using NeeView.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DriveDirectoryNode : DirectoryNode
    {
        private DriveInfo _driveInfo;
        private bool _iconInitialized;


        public DriveDirectoryNode(DriveInfo drive, RootDirectoryNode parent) : base(drive.Name.TrimEnd(LoosePath.Separator), parent)
        {
            _driveInfo = drive;
            var async = InitializeAsync();
        }

        public string DriveName => Name + '\\';

        private string _dispName;
        public override string DispName
        {
            get { return _dispName; }
            set { SetProperty(ref _dispName, value); }
        }


        public ImageSource _icon;
        public override ImageSource Icon
        {
            get
            {
                if (!_iconInitialized)
                {
                    _iconInitialized = true;

                    Task.Run(async () =>
                    {
                        for (int i = 0; i < 2; ++i)
                        {
                            try
                            {
                                ////Debug.WriteLine($"{Name}: Icon load...");
                                _icon = FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.Directory, 16.0, false, false);
                                if (_icon != null)
                                {
                                    _icon?.Freeze();
                                    ////Debug.WriteLine($"{Name}: Icon done.");
                                    RaisePropertyChanged(nameof(Icon));
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            await Task.Delay(500);
                        }
                    });
                }

                return _icon ?? FileIconCollection.Current.CreateDefaultFolderIcon(16.0);
            }
        }

        public override string Path => Name + '\\';

        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set { SetProperty(ref _isReady, value); }
        }

        private async Task InitializeAsync()
        {
            IsDelayCreation = true;

            await Task.Run(() =>
            {
                var volumeLabel = _driveInfo.DriveType.ToDispString();
                DispName = string.Format("{0} ({1})", volumeLabel, Name);

                // NOTE: ドライブによってはこのプロパティの取得に時間がかかる
                IsReady = _driveInfo.IsReady;

                try
                {
                    if (_driveInfo.IsReady)
                    {
                        volumeLabel = string.IsNullOrEmpty(_driveInfo.VolumeLabel) ? _driveInfo.DriveType.ToDispString() : _driveInfo.VolumeLabel;
                        DispName = string.Format("{0} ({1})", volumeLabel, Name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                switch (_driveInfo.DriveType)
                {
                    case DriveType.Fixed:
                    case DriveType.Removable:
                    case DriveType.Ram:
                        IsDelayCreateInheritance = false;
                        break;
                    default:
                        IsDelayCreateInheritance = true;
                        break;
                }

                RefreshIcon();
            });
        }

        public void Refresh()
        {
            RefreshChildren();
            var async = InitializeAsync();
        }

        public override void RefreshIcon()
        {
            _iconInitialized = false;
            RaisePropertyChanged(nameof(Icon));
        }

        protected override void OnException(DirectoryNode sender, NotifyCrateDirectoryChildrenExcepionEventArgs e)
        {
            if (sender is DirectoryNode directory)
            {
                ToastService.Current.Show("FolderList", new Toast($"({Name}) " + e.Exception.Message));

                if (e.IsRefresh)
                {
                    if (!_driveInfo.IsReady)
                    {
                        Refresh();
                    }
                }
            }
        }

        protected override void OnChildrenChanged(DirectoryNode sender, EventArgs e)
        {
            if (!IsReady)
            {
                IsReady = _driveInfo.IsReady;
                RefreshIcon();
            }
        }

    }

}

