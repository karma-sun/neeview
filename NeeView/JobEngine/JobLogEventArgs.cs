using System;

namespace NeeView
{
    /// <summary>
    /// 開発用：ログイベント引数
    /// </summary>
    public class JobLogEventArgs : EventArgs
    {
        public string Log { get; set; }
        public JobLogEventArgs(string log) => Log = log;
    }
}
