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
            [DriveType.Removable] = Properties.Resources.Word_RemovableDrive,
            [DriveType.Fixed] = Properties.Resources.Word_FixedDrive,
            [DriveType.Network] = Properties.Resources.Word_NetworkDrive,
            [DriveType.CDRom] = Properties.Resources.Word_CDRomDrive,
            [DriveType.Ram] = Properties.Resources.Word_RamDrive,
        };

        public static string ToDispString(this DriveType driveType)
        {
            return _driveTypeNames[driveType];
        }
    }
}

