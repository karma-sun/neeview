using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<DriveType, string> _driveTypeNames = new Dictionary<DriveType, string>
        {
            [DriveType.Unknown] = "",
            [DriveType.NoRootDirectory] = "",
            [DriveType.Removable] = Properties.Resources.WordRemovableDrive,
            [DriveType.Fixed] = Properties.Resources.WordFixedDrive,
            [DriveType.Network] = Properties.Resources.WordNetworkDrive,
            [DriveType.CDRom] = Properties.Resources.WordCDRomDrive,
            [DriveType.Ram] = Properties.Resources.WordRamDrive,
        };

        private DriveInfo _driveInfo;
        private bool _iconInitialized;


        public DriveDirectoryNode(DriveInfo drive) : base(null, drive.Name)
        {
            Initialize(drive);
        }


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

                    Task.Run(() =>
                    {
                        try
                        {
                            ////Debug.WriteLine($"{Name}: Icon load...");
                            _icon = FileIconCollection.Current.CreateFileIcon(Path, IO.FileIconType.Directory, 16.0, false, false);
                            _icon?.Freeze();
                            ////Debug.WriteLine($"{Name}: Icon done.");
                            RaisePropertyChanged(nameof(Icon));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    });
                }

                return _icon ?? FileIconCollection.Current.CreateDefaultFolderIcon(16.0);
            }
        }

        public override string Key => Name.TrimEnd(LoosePath.Separator);


        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set { SetProperty(ref _isReady, value); }
        }

        private void Initialize(DriveInfo drive)
        {
            _driveInfo = drive;
            IsReady = _driveInfo.IsReady;

            var driveName = string.Format("{0} ({1})", _driveTypeNames[drive.DriveType], drive.Name.TrimEnd('\\'));

            if (drive.IsReady)
            {
                try
                {
                    driveName = string.Format("{0} ({1})", string.IsNullOrEmpty(drive.VolumeLabel) ? _driveTypeNames[drive.DriveType] : drive.VolumeLabel, drive.Name.TrimEnd('\\'));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            DispName = driveName;

            DelayCreateChildren();

            switch (drive.DriveType)
            {
                case DriveType.Fixed:
                case DriveType.Removable:
                case DriveType.Ram:
                    break;
                default:
                    IsDelayCreateChildremAll = true;
                    break;
            }
        }

        public void Refresh()
        {
            ResetChildren(true);

            Initialize(_driveInfo);
            RefreshIcon();
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
                FolderTreeModel.Current.ShowToast($"({_driveInfo?.Name.TrimEnd('\\')}) " + e.Exception.Message);

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

