// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Jobs = NeeLaboratory.Threading.Jobs;

namespace NeeView
{
    /// <summary>
    /// FolderCollection用JobEngine
    /// </summary>
    public class FolderCollectionJobEngine : IDisposable, IEngine
    {
        #region Fields

        private FolderCollection _folderCollection;

        private Jobs.JobEngine _jobEngine;

        #endregion

        #region Constructors

        public FolderCollectionJobEngine(FolderCollection folderCollection)
        {
            _folderCollection = folderCollection;
            _jobEngine = new Jobs.JobEngine();
            _jobEngine.Error += JobEngine_Error;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// JobEngineで例外発生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobEngine_Error(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"JobEngine Exception!: {e.GetException().Message}");
            throw e.GetException();
        }

        /// <summary>
        /// 項目追加
        /// </summary>
        /// <param name="path"></param>
        public void RequestCreate(string path)
        {
            _jobEngine.Enqueue(new CreateJob(this, path, false));
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="path"></param>
        public void RequestDelete(string path)
        {
            _jobEngine.Enqueue(new DeleteJob(this, path, false));

        }

        /// <summary>
        /// 項目名変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="path"></param>
        public void RequestRename(string oldPath, string path)
        {
            _jobEngine.Enqueue(new RenameJob(this, oldPath, path, false));
        }

        #endregion Methods

        #region Jobs

        #region Job.Create

        public class CreateJob : Jobs.IJob
        {
            private FolderCollectionJobEngine _target;
            private string _path;
            private bool _verify;

            public CreateJob(FolderCollectionJobEngine target, string path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                await _target.CreateAsync(_path, _verify);
            }
        }

#pragma warning disable 1998

        private async Task CreateAsync(string path, bool verify)
        {
            Debug.WriteLine($"Create: {path}");
            _folderCollection.CreateItem(path);
        }

#pragma warning restore 1998

        #endregion

        #region Job.Delete

        public class DeleteJob : Jobs.IJob
        {
            private FolderCollectionJobEngine _target;
            private string _path;
            private bool _verify;

            public DeleteJob(FolderCollectionJobEngine target, string path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                await _target.DeleteAsync(_path, _verify);
            }
        }

#pragma warning disable 1998

        private async Task DeleteAsync(string path, bool verify)
        {
            Debug.WriteLine($"Delete: {path}");
            _folderCollection.DeleteItem(path);
        }

#pragma warning restore 1998

        #endregion

        #region Job.Rename

        public class RenameJob : Jobs.IJob
        {
            private FolderCollectionJobEngine _target;
            private string _oldPath;
            private string _path;
            private bool _verify;

            public RenameJob(FolderCollectionJobEngine target, string oldPath, string path, bool verify)
            {
                _target = target;
                _oldPath = oldPath;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                await _target.RenameAsync(_oldPath, _path, _verify);
            }
        }

#pragma warning disable 1998

        private async Task RenameAsync(string oldPath, string path, bool verify)
        {
            Debug.WriteLine($"Rename: {oldPath} => {path}");
            _folderCollection.RenameItem(oldPath, path);
        }

#pragma warning restore 1998

        #endregion

        #endregion Jobs

        #region IDisposable Support

        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _jobEngine.IsEnabled = false;
                    _jobEngine.Dispose();
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
        }


        #endregion

        #region  IEngine Support

        public void StartEngine()
        {
            _jobEngine.IsEnabled = true;
        }

        public void StopEngine()
        {
            _jobEngine.IsEnabled = false;
        }

        #endregion
    }



}
