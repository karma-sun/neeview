﻿using System;
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
            Debug.Assert(path.StartsWith(Temporary.Current.TempDirectory));

            UpdateLastAccessTime();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 最終アクセス日時
        /// </summary>
        public DateTime LastAccessTime { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// 最終アクセス日時更新
        /// </summary>
        public void UpdateLastAccessTime()
        {
            this.LastAccessTime = DateTime.Now;
        }

        #endregion

        #region ITrash Support

        public bool IsDisposed => _disposedValue;

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.LastAccessTime = default;
                }

                try
                {
                    if (Path != null && Path.StartsWith(Temporary.Current.TempDirectory)) // 念入りチェック
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

        ~TempFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
