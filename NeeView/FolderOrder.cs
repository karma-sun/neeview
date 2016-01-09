﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// フォルダの並び
    /// </summary>
    public enum FolderOrder
    {
        FileName,
        TimeStamp,
        Random,
    }

    public static class FolderOrderByExtension
    {
        public static FolderOrder GetToggle(this FolderOrder mode)
        {
            return (FolderOrder)(((int)mode + 1) % Enum.GetNames(typeof(FolderOrder)).Length);
        }

        public static string ToDispString(this FolderOrder mode)
        {
            switch (mode)
            {
                case FolderOrder.FileName: return "フォルダ列はファイル名順";
                case FolderOrder.TimeStamp: return "フォルダ列は日付順";
                case FolderOrder.Random: return "フォルダ列はランダム";
                default:
                    throw new NotSupportedException();
            }
        }
    }

}
