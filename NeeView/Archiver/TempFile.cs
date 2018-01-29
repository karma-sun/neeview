// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// テンポラリファイル
    /// </summary>
    public class TempFile : FileProxy, ITrash
    {
        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path"></param>
        public TempFile(string path) : base(path)
        {
            // テンポラリフォルダー以外は非対応
            Debug.Assert(path.StartsWith(Temporary.TempDirectory));

            UpdateLastAccessTime();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 最終アクセス日時
        /// </summary>
        public DateTime LastAccessTime { get; private set; }

        #endregion

        /// <summary>
        /// 最終アクセス日時更新
        /// </summary>
        public void UpdateLastAccessTime()
        {
            this.LastAccessTime = DateTime.Now;
        }

        #region ITrash Support

        //
        public bool IsDisposed => _disposedValue;

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        //
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // マネージ状態を破棄します (マネージ オブジェクト)。
                    this.LastAccessTime = default(DateTime);
                }

                //  アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                try
                {
                    if (Path != null && Path.StartsWith(Temporary.TempDirectory)) // 念入りチェック
                    {
                        if (File.Exists(Path)) File.Delete(Path);
                        Path = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                _disposedValue = true;
            }
        }

        // 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
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
            // 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
