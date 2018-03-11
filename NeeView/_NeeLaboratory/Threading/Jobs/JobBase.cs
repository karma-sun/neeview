using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Job実行結果
    /// </summary>
    public enum JobResult
    {
        None,

        /// <summary>
        /// 完了
        /// </summary>
        Completed,

        /// <summary>
        /// キャンセル
        /// </summary>
        Canceled,
    }

    /// <summary>
    /// Job基底
    /// キャンセル、終了待機対応
    /// </summary>
    public abstract class JobBase : IJob
    {
        #region Fields

        /// <summary>
        /// キャンセルトークン
        /// </summary>
        protected CancellationToken _cancellationToken;

        /// <summary>
        /// 実行完了待ち用フラグ
        /// </summary>
        private ManualResetEventSlim _complete = new ManualResetEventSlim(false);

        /// <summary>
        /// 実行結果
        /// </summary>
        private JobResult _result;

        #endregion

        #region Properties

        /// <summary>
        /// 実行結果
        /// </summary>
        public JobResult Result
        {
            get { return _result; }
            private set { _result = value; _complete.Set(); }
        }

        // キャンセル可能フラグ
        public bool CanBeCanceled => _cancellationToken.CanBeCanceled;

        #endregion

        #region Constructoes

        /// <summary>
        /// constructor
        /// </summary>
        public JobBase()
        {
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        public JobBase(CancellationToken token)
        {
            _cancellationToken = token;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Job実行
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if (_complete.IsSet) return;

            // cancel ?
            if (_cancellationToken.IsCancellationRequested)
            {
                this.Result = JobResult.Canceled;
                return;
            }

            // execute
            try
            {
                await ExecuteAsync(_cancellationToken);
                this.Result = JobResult.Completed;
            }
            catch (OperationCanceledException)
            {
                this.Result = JobResult.Canceled;
                Debug.WriteLine($"Job {this}: canceled.");
                OnCanceled();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Job {this}: excepted!!");
                OnException(e);
                throw;
            }
        }

        /// <summary>
        /// Job終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync()
        {
            await Task.Run(() => _complete.Wait());
        }

        /// <summary>
        /// Job終了待機
        /// </summary>
        /// <returns></returns>
        public async Task WaitAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                await Task.Yield();
                _complete.Wait(token);
            });
        }

        /// <summary>
        /// Job実行(abstract)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task ExecuteAsync(CancellationToken token);

        /// <summary>
        /// Jobキャンセル時
        /// </summary>
        protected virtual void OnCanceled()
        {
        }

        /// <summary>
        /// Job例外時
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnException(Exception e)
        {
        }

        #endregion
    }


    public abstract class CancelableJobBase : JobBase
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _canBeCanceled = true;

        public CancelableJobBase() : base()
        {
            _cancellationToken = _tokenSource.Token;
        }

        /// <summary>
        /// キャンセル可能プロパティ
        /// </summary>
        public new bool CanBeCanceled
        {
            get { return _canBeCanceled && base.CanBeCanceled; }
            set { _canBeCanceled = value; }
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }
    }
}
