﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ファイルプロキシ
    /// </summary>
    public class FileProxy
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public FileProxy(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Path { get; protected set; }
    }
}
