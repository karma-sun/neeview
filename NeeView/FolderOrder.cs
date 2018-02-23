// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダーの並び
    /// </summary>
    [DataContract]
    public enum FolderOrder
    {
        [EnumMember(Value = "FileName")]
        [AliasName("名前昇順")]
        FileNameAscending,

        [EnumMember]
        [AliasName("名前降順")]
        FileNameDescending,

        [EnumMember]
        [AliasName("日付昇順")]
        TimeStampAscending,

        [EnumMember(Value = "TimeStamp")]
        [AliasName("日付降順")]
        TimeStampDescending,

        [EnumMember]
        [AliasName("サイズ昇順")]
        SizeAscending,

        [EnumMember(Value = "Size")]
        [AliasName("サイズ降順")]
        SizeDescending,

        [EnumMember]
        [AliasName("シャッフル")]
        Random,
    }

    public static class FolderOrderExtension
    {
        public static FolderOrder GetToggle(this FolderOrder mode)
        {
            return (FolderOrder)(((int)mode + 1) % Enum.GetNames(typeof(FolderOrder)).Length);
        }
    }
}
