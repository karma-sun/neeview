using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Jobs = NeeLaboratory.Threading.Jobs;

namespace NeeView
{
    public class FolderCollectionEngine : IDisposable
    {
        private FolderCollection _folderCollection;
        private Jobs.SingleJobEngine _engine;

        public FolderCollectionEngine(FolderCollection folderCollection)
        {
            _folderCollection = folderCollection;

            _engine = new Jobs.SingleJobEngine();
            _engine.Name = "FolderCollectionJobEngine";
            _engine.JobError += JobEngine_Error;
            _engine.StartEngine();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _engine.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// JobEngineで例外発生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobEngine_Error(object sender, Jobs.JobErrorEventArgs e)
        {
            Debug.WriteLine($"FolderCollection JOB Exception!: {e.Job}: {e.GetException().Message}");
            e.Handled = true;
        }

        /// <summary>
        /// 項目追加
        /// </summary>
        /// <param name="path"></param>
        public void RequestCreate(QueryPath path)
        {
            _engine?.Enqueue(new CreateJob(this, path, false));
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="path"></param>
        public void RequestDelete(QueryPath path)
        {
            _engine?.Enqueue(new DeleteJob(this, path, false));
        }

        /// <summary>
        /// 項目名変更
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="path"></param>
        public void RequestRename(QueryPath oldPath, QueryPath path)
        {
            if (oldPath == path || path == null)
            {
                return;
            }

            _engine?.Enqueue(new RenameJob(this, oldPath, path, false));
        }

        public class CreateJob : Jobs.IJob
        {
            private FolderCollectionEngine _target;
            private QueryPath _path;
            private bool _verify;

            public CreateJob(FolderCollectionEngine target, QueryPath path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Create: {_path}");
                _target._folderCollection.AddItem(_path); // TODO: ファイルシステム以外のFolderCollectionでは不正な操作になる
                await Task.CompletedTask;
            }
        }

        public class DeleteJob : Jobs.IJob
        {
            private FolderCollectionEngine _target;
            private QueryPath _path;
            private bool _verify;

            public DeleteJob(FolderCollectionEngine target, QueryPath path, bool verify)
            {
                _target = target;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Delete: {_path}");
                _target._folderCollection.DeleteItem(_path);
                await Task.CompletedTask;
            }
        }

        public class RenameJob : Jobs.IJob
        {
            private FolderCollectionEngine _target;
            private QueryPath _oldPath;
            private QueryPath _path;
            private bool _verify;

            public RenameJob(FolderCollectionEngine target, QueryPath oldPath, QueryPath path, bool verify)
            {
                _target = target;
                _oldPath = oldPath;
                _path = path;
                _verify = verify;
            }

            public async Task ExecuteAsync()
            {
                ////Debug.WriteLine($"Rename: {_oldPath} => {_path}");
                _target._folderCollection.RenameItem(_oldPath, _path);
                await Task.CompletedTask;
            }
        }

    }
}