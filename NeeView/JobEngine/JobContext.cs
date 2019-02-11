using System;

namespace NeeView
{
    /// <summary>
    /// Job環境
    /// Jobワーカータスクで共通のコンテキスト
    /// </summary>
    public class JobContext
    {
        // ジョブリスト
        public PriorityQueue<Job> JobQueue { get; private set; }

        // 排他処理用ロック
        public object Lock { get; private set; }

        // ジョブキュー変更通知
        public event EventHandler JobChanged;

        // コンストラクト
        public JobContext()
        {
            JobQueue = new PriorityQueue<Job>();
            Lock = new object();
        }

        // ジョブキュー変更通知
        public void RaiseJobChanged()
        {
            JobChanged?.Invoke(this, null);
        }
    }
}
