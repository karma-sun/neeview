using NeeLaboratory.ComponentModel;
using NeeView.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// TODO: Pageに限定することでBOX化を避ける?

namespace NeeView
{
    /// <summary>
    /// JOBスケジューラー
    /// </summary>
    public class JobScheduler : BindableBase 
    {
        public Dictionary<JobClient, List<JobSource>> _clients = new Dictionary<JobClient, List<JobSource>>();

        public event EventHandler QueueChanged;

        public object Lock { get; } = new object();


        private List<JobSource> _queue = new List<JobSource>();
        public List<JobSource> Queue
        {
            get { return _queue; }
            set { SetProperty(ref _queue, value); }
        }


        public void RaiseQueueChanged()
        {
            lock (Lock)
            {
                QueueChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Client登録
        /// </summary>
        public void RegistClent(JobClient client)
        {
            lock (Lock)
            {
                if (!_clients.ContainsKey(client))
                {
                    _clients.Add(client, new List<JobSource>());
                }
            }
        }

        public void UnregistClient(JobClient client)
        {
            lock (Lock)
            {
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="sender">発行元</param>
        /// <param name="orders">JOBオーダー。並び順がそのまま優先度</param>
        public List<JobSource> Order(JobClient sender, List<JobOrder> orders)
        {
            lock (Lock)
            {
                // TODO: 同じリクエストだったらなにもしない、とか、ここでする？

                // 対象カテゴリのJOBの取得
                var collection = Queue.Where(e => e.Category == sender.Category).ToList();

                // オーダーがcollectionにない場合はそのJOBを作成、追加。
                List<JobSource> sources = new List<JobSource>();
                foreach (var order in orders)
                {
                    var source = collection.FirstOrDefault(e => e.Key == order.Key);
                    if (source == null)
                    {
                        source = order.CreateJobSource();
                    }

                    sources.Add(source);
                }

                // そのクライアントのオーダーとして記憶
                Debug.Assert(_clients.ContainsKey(sender));
                _clients[sender] = sources;

                // 新しいQueue
                var queue = _clients.OrderByDescending(e => e.Key.Category.Priority).SelectMany(e => e.Value).ToList();
                ////Debug.WriteLine($"New: {queue.Count}");

                // 管理対象外のJOBにはキャンセル命令発行
                var removes = Queue.Except(queue).ToList();
                foreach (var remove in removes)
                {
                    ////Debug.WriteLine($"JobScheduler.Cancel: {remove}: {remove.GetHashCode()}");
                    remove.Cancel();
                    // TODO: Disposeする？
                }

                // Queue更新
                Queue = queue;
                QueueChanged?.Invoke(this, null);

                return sources;
            }
        }

        /// <summary>
        /// 次に処理するJOBを取得
        /// </summary>
        /// <returns></returns>
        public Job FetchNextJob(int minPriority)
        {
            lock (Lock)
            {
                var source = Queue.FirstOrDefault(e => !e.IsProcessed && minPriority <= e.Category.Priority);
                if (source != null)
                {
                    source.IsProcessed = true;
                    ////Debug.WriteLine($"JobScheduler.Processed: {source}");
                    return source.Job;
                }

                return null;
            }
        }
    }

}
