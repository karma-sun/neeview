// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// File I/O
    /// </summary>
    public class FileIO
    {
        public static FileIO Current { get; } = new FileIO();

        //
        public FileIO()
        {
        }

        // ファイル削除
        public void RemoveFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }
        }


    }
}
