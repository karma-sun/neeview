using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;

namespace NeeView.Windows.Data
{
    /// <summary>
    /// 値の遅延反映
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelayValue<T> where T : IComparable
    {
        #region Fields

        private T _value;
        private T _delayValue;
        private DateTime _delayTime = DateTime.MaxValue;
        private DispatcherTimer _timer;

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクター
        /// </summary>
        public DelayValue()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += Tick;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="value">初期値</param>
        public DelayValue(T value) : this()
        {
            _value = value;
            _delayValue = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// 値が反映されたときのイベント
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion

        #region Properties

        /// <summary>
        /// 現在値
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { SetValue(value, 0, true); }
        }
        #endregion

        #region Methods

        /// <summary>
        /// 遅延値設定
        /// TODO: isForceの意味が？
        /// </summary>
        /// <param name="value">目的値</param>
        /// <param name="ms">反映遅延時間</param>
        /// <param name="isForce">同じ値でも実行する</param>
        public void SetValue(T value, double ms = 0.0, bool isForce = false)
        {
            if (!isForce && EqualityComparer<T>.Default.Equals(_delayValue, value)) return;

            _delayValue = value;

            if (ms <= 0.0)
            {
                Flush();
            }
            else
            {
                _delayTime = DateTime.Now + TimeSpan.FromMilliseconds(ms);
                _timer.Start();
            }
        }

        /// <summary>
        /// 目的値を現在値に反映
        /// </summary>
        private void Flush()
        {
            _timer.Stop();

            if (!EqualityComparer<T>.Default.Equals(_delayValue, _value))
            {
                _value = _delayValue;
                ValueChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// タイマー処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object sender, EventArgs e)
        {
            if (_delayTime <= DateTime.Now)
            {
                Flush();
            }
        }

        /// <summary>
        /// 開発用：詳細状態取得
        /// </summary>
        /// <returns></returns>
        public string ToDetail()
        {
            return _timer.IsEnabled ? $"{_value} ({_delayValue}, {(_delayTime - DateTime.Now).TotalMilliseconds}ms)" : $"{_value}";
        }

        #endregion
    }
}
