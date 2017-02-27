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
    public class TempFile : FileProxy, IDisposable
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
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // managed
                if (disposing)
                {
                }

                // unmanaged
                if (Path != null && Path.StartsWith(Temporary.TempDirectory)) // 念入りチェック
                {
                    Debug.WriteLine($"remove temp: {Path}");
                    try
                    {
                        if (File.Exists(Path)) File.Delete(Path);
                    }
                    catch { }
                }
                Path = null;

                //
                disposedValue = true;
            }
        }

        ~TempFile()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
