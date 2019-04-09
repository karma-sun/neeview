using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 遅延実行
    /// コマンドを遅延実行する。遅延中に要求された場合は古いコマンドをキャンセルする。
    /// </summary>
    public class DelayAction : IDisposable
    {
        /// <summary>
        /// 遅延実行要求
        /// </summary>
        private bool _isRequested;

        /// <summary>
        /// 最後のGC要求時間
        /// </summary>
        private DateTime _lastRequestTime;

        /// <summary>
        /// 遅延実行のためのタイマー
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// 遅延時間
        /// </summary>
        private TimeSpan _delay;

        /// <summary>
        /// 実行本体
        /// </summary>
        private Action _action;

        /// <summary>
        /// lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public DelayAction(Dispatcher dispatcher, TimeSpan interval, Action action, TimeSpan delay)
        {
            // timer for delay
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = interval;
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);

            _action = action;
            _delay = delay;
        }

        /// <summary>
        /// 実行要求
        /// </summary>
        public void Request()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _lastRequestTime = DateTime.Now;
                _isRequested = true;
                _timer.Start();
            }
        }

        /// <summary>
        /// 実行キャンセル
        /// </summary>
        public void Cancel()
        {
            lock (_lock)
            {
                _timer.Stop();
                _isRequested = false;
            }
        }

        /// <summary>
        /// 遅延されている命令を即時実行する
        /// </summary>
        public void Flush()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _delay = TimeSpan.Zero;
            }

            DispatcherTimer_Tick(this, null);
        }


        /// <summary>
        /// timer callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            bool isExecute = false;

            lock (_lock)
            {
                if ((DateTime.Now - _lastRequestTime) >= _delay)
                {
                    _timer.Stop();
                    if (_isRequested)
                    {
                        _isRequested = false;
                        isExecute = true;
                    }
                }
            }

            if (isExecute)
            {
                _action?.Invoke();
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
