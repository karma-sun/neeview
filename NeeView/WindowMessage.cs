using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{

    public class DriveChangedEventArgs : EventArgs
    {
        public DriveChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }
    }

    public class MediaChangedEventArgs : EventArgs
    {
        public MediaChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }
    }

    public enum DirectoryChangeType
    {
        Created = 1,
        Deleted = 2,
        Changed = 4,
        Renamed = 8,
        All = 15
    }

    public class DirectoryChangedEventArgs : EventArgs
    {
        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath, string oldFullpath)
        {
            if (changeType == DirectoryChangeType.All) throw new ArgumentOutOfRangeException(nameof(changeType));

            ChangeType = changeType;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));

            if (changeType == DirectoryChangeType.Renamed)
            {
                OldFullPath = oldFullpath ?? throw new ArgumentNullException(nameof(oldFullpath));

                if (Path.GetDirectoryName(OldFullPath) != Path.GetDirectoryName(FullPath))
                {
                    throw new ArgumentException("Not same directory");
                }
            }
        }

        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath) : this(changeType, fullPath, null)
        {
            if (changeType == DirectoryChangeType.Renamed) throw new InvalidOperationException();
        }


        public DirectoryChangeType ChangeType { get; set; }
        public string FullPath { get; set; }
        public string OldFullPath { get; set; }
    }


    public class WindowMessage
    {
        #region Win32API

        internal static class NativeMethods
        {
            // ウィンドウメッセージ
            public const int WM_SIZE = 0x0005;
            public const int WM_ENTERSIZEMOVE = 0x0231;
            public const int WM_EXITSIZEMOVE = 0x0232;
            public const int WM_DEVICECHANGE = 0x0219;
            public const int WM_SHNOTIFY = 0x0401;

            // Win32API の PostMessage 関数のインポート
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

            [Flags]
            public enum SHCNRF
            {
                InterruptLevel = 0x1,
                ShellLevel = 0x2,
                RecursiveInterrupt = 0x1000,
                NewDelivery = 0x8000,
            }

            public enum SHCNF
            {
                SHCNF_IDLIST = 0x0000,
                SHCNF_PATHA = 0x0001,
                SHCNF_PRINTERA = 0x0002,
                SHCNF_DWORD = 0x0003,
                SHCNF_PATHW = 0x0005,
                SHCNF_PRINTERW = 0x0006,
                SHCNF_TYPE = 0x00FF,
                SHCNF_FLUSH = 0x1000,
                SHCNF_FLUSHNOWAIT = 0x2000
            }

            [Flags]
            public enum SHCNE
            {
                SHCNE_RENAMEITEM = 0x00000001,
                SHCNE_CREATE = 0x00000002,
                SHCNE_DELETE = 0x00000004,
                SHCNE_MKDIR = 0x00000008,
                SHCNE_RMDIR = 0x00000010,
                SHCNE_MEDIAINSERTED = 0x00000020,
                SHCNE_MEDIAREMOVED = 0x00000040,
                SHCNE_DRIVEREMOVED = 0x00000080,
                SHCNE_DRIVEADD = 0x00000100,
                SHCNE_NETSHARE = 0x00000200,
                SHCNE_NETUNSHARE = 0x00000400,
                SHCNE_ATTRIBUTES = 0x00000800,
                SHCNE_UPDATEDIR = 0x00001000,
                SHCNE_UPDATEITEM = 0x00002000,
                SHCNE_SERVERDISCONNECT = 0x00004000,
                SHCNE_UPDATEIMAGE = 0x00008000,
                SHCNE_DRIVEADDGUI = 0x00010000,
                SHCNE_RENAMEFOLDER = 0x00020000,
                SHCNE_FREESPACE = 0x00040000,
                SHCNE_EXTENDED_EVENT = 0x04000000,
                SHCNE_ASSOCCHANGED = 0x08000000,
                SHCNE_DISKEVENTS = 0x0002381F,
                SHCNE_GLOBALEVENTS = 0x0C0581E0,
                SHCNE_ALLEVENTS = 0x7FFFFFFF,
                SHCNE_INTERRUPT = unchecked((int)0x80000000)
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHChangeNotifyEntry
            {
                public IntPtr pIdl;
                [MarshalAs(UnmanagedType.Bool)] public Boolean Recursively;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SHNOTIFYSTRUCT
            {
                public IntPtr dwItem1;
                public IntPtr dwItem2;
            }

            [DllImport("shell32.dll", SetLastError = true, EntryPoint = "#2", CharSet = CharSet.Auto)]
            public static extern UInt32 SHChangeNotifyRegister(IntPtr hWnd, SHCNRF fSources, SHCNE fEvents, uint wMsg, int cEntries, ref SHChangeNotifyEntry pFsne);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern Int32 SHGetPathFromIDList(IntPtr pIDL, StringBuilder strPath);

            /*
            [DllImport("shell32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);
            */

            public enum DBT
            {
                DBT_DEVICEARRIVAL = 0x8000,
                DBT_DEVICEQUERYREMOVE = 0x8001,
                DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
                DBT_DEVICEREMOVEPENDING = 0x8003,
                DBT_DEVICEREMOVECOMPLETE = 0x8004,
            }

            public enum DBT_DEVTP
            {
                DBT_DEVTYP_OEM = 0x0000,
                DBT_DEVTYP_DEVNODE = 0x0001,
                DBT_DEVTYP_VOLUME = 0x0002,
                DBT_DEVTYP_PORT = 0x0003,
                DBT_DEVTYP_NET = 0x0004,
                DBT_DEVTYP_DEVICEINTERFACE = 0x0005,
                DBT_DEVTYP_HANDLE = 0x0006,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DEV_BROADCAST_VOLUME
            {
                public uint dbcv_size;
                public uint dbcv_devicetype;
                public uint dbcv_reserved;
                public uint dbcv_unitmask;
                public ushort dbcv_flags;
            }

        }

        #endregion

        public static WindowMessage Current { get; } = new WindowMessage();

        private Window _window;

        public WindowMessage()
        {
        }

        public event EventHandler<DriveChangedEventArgs> DriveChanged;
        public event EventHandler<MediaChangedEventArgs> MediaChanged;
        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;
        public event EventHandler EnterSizeMove;
        public event EventHandler ExitSizeMove;

        // ウィンドウプロシージャ初期化
        public void Initialize(Window window)
        {
            if (_window != null) throw new InvalidOperationException();

            var hsrc = HwndSource.FromVisual(window) as HwndSource;
            _window = window;

            var notifyEntry = new NativeMethods.SHChangeNotifyEntry() { pIdl = IntPtr.Zero, Recursively = true };
            var notifyId = NativeMethods.SHChangeNotifyRegister(hsrc.Handle,
                                                  NativeMethods.SHCNRF.ShellLevel,
                                                  NativeMethods.SHCNE.SHCNE_MEDIAINSERTED | NativeMethods.SHCNE.SHCNE_MEDIAREMOVED
                                                  | NativeMethods.SHCNE.SHCNE_MKDIR | NativeMethods.SHCNE.SHCNE_RMDIR | NativeMethods.SHCNE.SHCNE_RENAMEFOLDER,
                                                  NativeMethods.WM_SHNOTIFY,
                                                  1,
                                                  ref notifyEntry);

            hsrc.AddHook(WndProc);
        }

        // ウィンドウプロシージャ
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                switch (msg)
                {
                    case NativeMethods.WM_ENTERSIZEMOVE:
                        EnterSizeMove?.Invoke(this, null);
                        break;
                    case NativeMethods.WM_EXITSIZEMOVE:
                        ExitSizeMove?.Invoke(this, null);
                        break;
                    case NativeMethods.WM_DEVICECHANGE:
                        OnDeviceChange(wParam, lParam);
                        break;
                    case NativeMethods.WM_SHNOTIFY:
                        OnSHNotify(wParam, lParam);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return IntPtr.Zero;
        }


        //
        private void OnDeviceChange(IntPtr wParam, IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
            {
                return;
            }

            var volume = (NativeMethods.DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lParam, typeof(NativeMethods.DEV_BROADCAST_VOLUME));
            var driveName = UnitMaskToDriveName(volume.dbcv_unitmask);
            if (driveName == null)
            {
                return;
            }

            switch ((NativeMethods.DBT)wParam.ToInt32())
            {
                case NativeMethods.DBT.DBT_DEVICEARRIVAL:
                    ////Debug.WriteLine("DBT_DEVICEARRIVAL");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, true));
                    break;
                case NativeMethods.DBT.DBT_DEVICEREMOVECOMPLETE:
                    ////Debug.WriteLine("DBT_DEVICEREMOVECOMPLETE");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, false));
                    break;
            }
        }

        private string UnitMaskToDriveName(uint unitmask)
        {
            for(int i=0; i<32; ++i)
            {
                if ((unitmask >> i & 1) == 1)
                {
                    return ((char)('A' + i)).ToString() + ":\\";
                }
            }

            return null;
        }

        // TODO: 重い処理が多いので、集積かBeginInvokeかする。
        private void OnSHNotify(IntPtr wParam, IntPtr lParam)
        {
            var shNotify = (NativeMethods.SHNOTIFYSTRUCT)Marshal.PtrToStructure(wParam, typeof(NativeMethods.SHNOTIFYSTRUCT));

            var shcne = (NativeMethods.SHCNE)lParam;

            ////Debug.WriteLine(shcne + ": " + shNotify);

            switch (shcne)
            {
                case NativeMethods.SHCNE.SHCNE_MEDIAINSERTED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        MediaChanged?.Invoke(this, new MediaChangedEventArgs(path, true));
                    }
                    break;
                case NativeMethods.SHCNE.SHCNE_MEDIAREMOVED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        MediaChanged?.Invoke(this, new MediaChangedEventArgs(path, false));
                    }
                    break;

                case NativeMethods.SHCNE.SHCNE_MKDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Created, path));
                        }
                    }
                    break;

                case NativeMethods.SHCNE.SHCNE_RMDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Deleted, path));
                        }
                    }
                    break;

                case NativeMethods.SHCNE.SHCNE_RENAMEFOLDER:
                    {
                        var path1 = PIDLToString(shNotify.dwItem1);
                        var path2 = PIDLToString(shNotify.dwItem2);
                        if (!string.IsNullOrEmpty(path1) && path1 != path2)
                        {
                            // path1 is new, path2 is old, maybe.
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Renamed, path2, path1));
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private string PIDLToString(IntPtr dwItem)
        {
            if (dwItem == IntPtr.Zero)
            {
                return null;
            }

            var buff = new StringBuilder(1024);
            NativeMethods.SHGetPathFromIDList(dwItem, buff);
            return buff.ToString(); ;
        }

    }
}
