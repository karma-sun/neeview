using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace NeeView.Susie
{
    public static class SusiePluginServerUtility
    {
        public static string CreateServerName(Process process)
        {
            return $"nv{process.Id}.rpc";
        }
    }

    public static class SusiePluginCommandId
    {
        public const int None = 0x0000;
        public const int Initialize = 0x0001;
        public const int GetPlugin = 0x0002;
        public const int SetPlugin = 0x0003;
        public const int SetPluginOrder = 0x0004;
        public const int GetImagePlugin = 0x0005;
        public const int GetArchivePlugin = 0x0006;
        public const int ShowConfigulationDlg = 0x0007;
        public const int GetImage = 0x0008;
        public const int GetArchiveEntries = 0x0009;
        public const int ExtractArchiveEntry = 0x000A;
        public const int ExtractArchiveEntryToFolder = 0x000B;

        public const int Error = -1;
    }



    [DataContract]
    public class SusiePluginCommandResult
    {
        public SusiePluginCommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        [DataMember]
        public bool IsSuccess { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandInitialize
    {
        public SusiePluginCommandInitialize(string pluginFolder, List<SusiePluginSetting> settings)
        {
            PluginFolder = pluginFolder;
            Settings = settings;
        }

        [DataMember]
        public string PluginFolder { get; set; }

        [DataMember]
        public List<SusiePluginSetting> Settings { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandGetPlugin
    {
        public SusiePluginCommandGetPlugin(List<string> pluginNames)
        {
            PluginNames = pluginNames;
        }

        [DataMember]
        public List<string> PluginNames { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandGetPluginResult
    {
        public SusiePluginCommandGetPluginResult(List<SusiePluginInfo> pluginInfos)
        {
            PluginInfos = pluginInfos;
        }

        [DataMember]
        public List<SusiePluginInfo> PluginInfos { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandSetPlugin
    {
        public SusiePluginCommandSetPlugin(List<SusiePluginSetting> settings)
        {
            Settings = settings;
        }

        [DataMember]
        public List<SusiePluginSetting> Settings { get; set; }
    }



    [DataContract]
    public class SusiePluginCommandSetPluginOrder
    {
        public SusiePluginCommandSetPluginOrder(List<string> order)
        {
            Order = order;
        }

        [DataMember]
        public List<string> Order { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandShowConfigulationDlg
    {
        public SusiePluginCommandShowConfigulationDlg(string pluginName, int hWnd)
        {
            PluginName = pluginName;
            HWnd = hWnd;
        }

        [DataMember]
        public string PluginName { get; set; }

        [DataMember]
        public int HWnd { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandGetArchivePlugin
    {
        public SusiePluginCommandGetArchivePlugin(string fileName, bool isCheckExtension)
        {
            FileName = fileName;
            IsCheckExtension = isCheckExtension;
        }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool IsCheckExtension { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandGetArchivePluginResult
    {
        public SusiePluginCommandGetArchivePluginResult(SusiePluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        [DataMember]
        public SusiePluginInfo PluginInfo { get; set; }
    }

        [DataContract]
    public class SusiePluginCommandGetImagePlugin
    {
        public SusiePluginCommandGetImagePlugin(string fileName, bool isCheckExtension)
        {
            FileName = fileName;
            IsCheckExtension = isCheckExtension;
        }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool IsCheckExtension { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandGetImagePluginResult
    {
        public SusiePluginCommandGetImagePluginResult(SusiePluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        [DataMember]
        public SusiePluginInfo PluginInfo { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandGetImage
    {
        public SusiePluginCommandGetImage(string pluginName, string fileName, bool isCheckExtension)
        {
            PluginName = pluginName;
            FileName = fileName;
            IsCheckExtension = isCheckExtension;
        }

        [DataMember]
        public string PluginName { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool IsCheckExtension { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandGetImageResult
    {
        public SusiePluginCommandGetImageResult(SusiePluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        [DataMember]
        public SusiePluginInfo PluginInfo { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandGetArchiveEntries
    {
        public SusiePluginCommandGetArchiveEntries(string pluginName, string fileName)
        {
            PluginName = pluginName;
            FileName = fileName;
        }

        [DataMember]
        public string PluginName { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandGetArchiveEntriesResult
    {
        public SusiePluginCommandGetArchiveEntriesResult(List<SusieArchiveEntry> entries)
        {
            Entries = entries;
        }

        [DataMember]
        public List<SusieArchiveEntry> Entries { get; set; }
    }


    [DataContract]
    public class SusiePluginCommandExtractArchiveEntry
    {
        public SusiePluginCommandExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            PluginName = pluginName;
            FileName = fileName;
            Position = position;
        }

        [DataMember]
        public string PluginName { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public int Position { get; set; }
    }

    [DataContract]
    public class SusiePluginCommandExtractArchiveEntryToFolder
    {
        public SusiePluginCommandExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            PluginName = pluginName;
            FileName = fileName;
            Position = position;
            ExtractFolder = extractFolder;
        }

        [DataMember]
        public string PluginName { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public string ExtractFolder { get; set; }
    }
}
