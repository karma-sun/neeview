using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// JOBの発行者。
    /// </summary>
    public class JobClient
    {
        public JobClient(JobCategory category)
        {
            Debug.Assert(category != null);
            Category = category;
        }

        public string Name { get; set; }

        public JobCategory Category { get; private set; }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }


    /// <summary>
    /// ページコンテンツ JobClient
    /// </summary>
    public class PageContentJobClient : JobClient, IDisposable
    {
        private List<JobSource> _sources = new List<JobSource>();

        public PageContentJobClient(string name, JobCategory category) : base(category)
        {
            Name = name;
            JobEngine.Current.RegistClient(this);
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="pages">読み込むページ</param>
        public void Order(List<Page> pages)
        {
            if (_disposedValue) return;

            var orders = pages
                .Where(e => !e.Content.IsLoaded)
                .Select(e => new JobOrder(this.Category, e))
                .ToList();

            _sources = JobEngine.Current.Order(this, orders);
        }

        /// <summary>
        /// JOBの完了を待つ
        /// </summary>
        /// <param name="pages">完了待ちをするページ</param>
        public async Task WaitAsync(List<Page> pages, int millisecondsTimeout, CancellationToken token)
        {
            if (_disposedValue) return;

            var tasks = pages
                .Select(e => _sources.FirstOrDefault(a => a.Key == e)?.WaitAsync(millisecondsTimeout, token))
                .Where(e => e != null)
                .ToList();

            await Task.WhenAll(tasks);
        }

        public void CancelOrder()
        {
            if (_disposedValue) return;

            JobEngine.Current.Order(this, new List<JobOrder>());
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    JobEngine.Current.UnregistClient(this);
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


    /// <summary>
    /// ページサムネイル JobClient
    /// </summary>
    public class PageThumbnailJobClient : JobClient, IDisposable
    {
        public PageThumbnailJobClient(string name, JobCategory category) : base(category)
        {
            Name = name;
            JobEngine.Current.RegistClient(this);
        }

        /// <summary>
        /// JOB要求
        /// </summary>
        /// <param name="pages">読み込むページ</param>
        public void Order(List<Page> pages)
        {
            if (_disposedValue) return;

            var orders = pages
                .Where(e => e != null && !e.Thumbnail.IsValid)
                .Select(e => new JobOrder(this.Category, e))
                .ToList();

            JobEngine.Current.Order(this, orders);
        }

        public void CancelOrder()
        {
            if (_disposedValue) return;

            JobEngine.Current.Order(this, new List<JobOrder>());
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    JobEngine.Current.UnregistClient(this);
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
