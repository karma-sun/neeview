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
    [Obsolete, DataContract(Name ="FolderOrder")]
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
    [DataContract(Name ="FolderOrderV2")]
    public enum FolderOrder
    {
        [EnumMember]
        [AliasName("名前昇順")]
        FileName,

        [EnumMember]
        [AliasName("名前降順")]
        FileNameDescending,

        [EnumMember]
        [AliasName("日付昇順")]
        TimeStamp,

        [EnumMember]
        [AliasName("日付降順")]
        TimeStampDescending,

        [EnumMember]
        [AliasName("サイズ昇順")]
        Size,

        [EnumMember]
        [AliasName("サイズ降順")]
        SizeDescending,

        [EnumMember]
        [AliasName("シャッフル")]
        Random,
    }

    public static class FolderOrderExtension
    {
        [Obsolete]
        public static FolderOrder ToV2(this FolderOrderV1 mode)
        {
            switch(mode)
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
    }
}
