// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// ファイルパス
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public FileProxy(string path)
        {
            this.Path = path;
        }
    }

    /// <summary>
    /// テンポラリファイル
    /// </summary>
    public class TempFile : FileProxy, ITrash
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public TempFile(string path) : base(path)
        {
            // テンポラリフォルダ以外は非対応
            Debug.Assert(path.StartsWith(Temporary.TempDirectory));
        }


        #region IDisposable Support

        //
        public bool IsDisposed => Path == null;

        //
        public virtual void Dispose()
        {
            // unmanaged
            if (Path != null && Path.StartsWith(Temporary.TempDirectory)) // 念入りチェック
            {
                try
                {
                    if (File.Exists(Path)) File.Delete(Path);
                    Path = null;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        ~TempFile()
        {
            Dispose();
        }
        #endregion
    }
}
