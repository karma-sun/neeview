using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び(互換用)
    /// </summary>
    [Obsolete, DataContract(Name = "FolderOrder")]
    public enum FolderOrderV1
    {
        [EnumMember]
        FileName,
        [EnumMember]
        TimeStamp,
        [EnumMember]
        Size,
        [EnumMember]
        Random,
    }

    /// <summary>
    /// フォルダーの並び
    /// </summary>
    [DataContract(Name = "FolderOrderV2")]
    public enum FolderOrder
    {
        [EnumMember]
        [AliasName]
        FileName,

        [EnumMember]
        [AliasName]
        FileNameDescending,

        [EnumMember]
        [AliasName]
        Path,

        [EnumMember]
        [AliasName]
        PathDescending,

        [EnumMember]
        [AliasName]
        FileType,

        [EnumMember]
        [AliasName]
        FileTypeDescending,

        [EnumMember]
        [AliasName]
        TimeStamp,

        [EnumMember]
        [AliasName]
        TimeStampDescending,

        [EnumMember]
        [AliasName]
        EntryTime,

        [EnumMember]
        [AliasName]
        EntryTimeDescending,

        [EnumMember]
        [AliasName]
        Size,

        [EnumMember]
        [AliasName]
        SizeDescending,

        [EnumMember]
        [AliasName]
        Random,
    }

    public static class FolderOrderExtension
    {
        [Obsolete]
        public static FolderOrder ToV2(this FolderOrderV1 mode)
        {
            switch (mode)
            {
                default:
                case FolderOrderV1.FileName:
                    return FolderOrder.FileName;
                case FolderOrderV1.TimeStamp:
                    return FolderOrder.TimeStampDescending;
                case FolderOrderV1.Size:
                    return FolderOrder.SizeDescending;
                case FolderOrderV1.Random:
                    return FolderOrder.Random;
            }
        }

        public static bool IsEntryCategory(this FolderOrder mode)
        {
            switch (mode)
            {
                case FolderOrder.EntryTime:
                case FolderOrder.EntryTimeDescending:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPathCategory(this FolderOrder mode)
        {
            switch (mode)
            {
                case FolderOrder.Path:
                case FolderOrder.PathDescending:
                    return true;
                default:
                    return false;
            }
        }
    }

}
