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
        [AliasName("@EnumFolderOrderFileName")]
        FileName,

        [EnumMember]
        [AliasName("@EnumFolderOrderFileNameDescending")]
        FileNameDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderPath")]
        Path,

        [EnumMember]
        [AliasName("@EnumFolderOrderPathDescending")]
        PathDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderFileType")]
        FileType,

        [EnumMember]
        [AliasName("@EnumFolderOrderFileTypeDescending")]
        FileTypeDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderTimeStamp")]
        TimeStamp,

        [EnumMember]
        [AliasName("@EnumFolderOrderTimeStampDescending")]
        TimeStampDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderEntryTime")]
        EntryTime,

        [EnumMember]
        [AliasName("@EnumFolderOrderEntryTimeDescending")]
        EntryTimeDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderSize")]
        Size,

        [EnumMember]
        [AliasName("@EnumFolderOrderSizeDescending")]
        SizeDescending,

        [EnumMember]
        [AliasName("@EnumFolderOrderRandom")]
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

        public static FolderOrder GetToggle(this FolderOrder mode)
        {
            return (FolderOrder)(((int)mode + 1) % Enum.GetNames(typeof(FolderOrder)).Length);
        }

        public static bool IsBookmarkOnly(this FolderOrder mode)
        {
            switch(mode)
            {
                case FolderOrder.Path:
                case FolderOrder.PathDescending:
                case FolderOrder.EntryTime:
                case FolderOrder.EntryTimeDescending:
                    return true;
                default:
                    return false;
            }
        }
    }
}
