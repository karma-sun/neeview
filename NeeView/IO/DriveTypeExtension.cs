using System.Collections.Generic;
using System.IO;

namespace NeeView.IO
{
    public static class DriveTypeExtension
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

        public static string ToDispString(this DriveType driveType)
        {
            return _driveTypeNames[driveType];
        }
    }
}

