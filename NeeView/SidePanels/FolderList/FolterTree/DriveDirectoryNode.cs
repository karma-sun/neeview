using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public DriveDirectoryNode(DriveInfo drive) : base(null, drive.Name)
        {
            Initialize(drive);
        }


        private string _driveName;
        public string DriveName
        {
            get { return _driveName; }
            set { SetProperty(ref _driveName, value); }
        }

        public override string Key => Name.TrimEnd(LoosePath.Separator);


        private void Initialize(DriveInfo drive)
        {
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

            DriveName = driveName;

            DelayCreateChildren();

            switch(drive.DriveType)
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

            var drive = DriveInfo.GetDrives().FirstOrDefault(e => e.Name == this.Name);
            if (drive != null)
            {
                Initialize(drive);
            }

            RaisePropertyChanged(nameof(Icon));
        }

        protected override void OnException(DirectoryNode sender, NotifyCrateDirectoryChildrenExcepionEventArgs e)
        {
            if (sender is DirectoryNode directory)
            {
                FolderTreeModel.Current.ShowToast(e.Exception.Message);

                if (e.IsRefresh)
                {
                    var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == this.Name);
                    if (driveInfo != null)
                    {
                        if (!driveInfo.IsReady)
                        {
                            Refresh();
                        }
                    }
                }
            }
        }

    }

}

