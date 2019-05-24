using System;
using System.Diagnostics;

namespace NeeView
{
    public class SubProcess : IDisposable
    {
        private string _filename;
        private Process _process;

        public SubProcess(string path)
        {
            _filename = path;
        }

        public Process Process => _process;

        public void Start()
        {
            if (IsActive) return;

            var psInfo = new ProcessStartInfo();
            psInfo.FileName = _filename;
            //psInfo.CreateNoWindow = true; // コンソール・ウィンドウを開かない
            //psInfo.UseShellExecute = false; // シェル機能を使用しない
            _process = Process.Start(psInfo);
        }

        public bool IsActive => _process != null && !_process.HasExited;

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                if (!_process.HasExited)
                {
                    _process.Kill();
                }

                _disposedValue = true;
            }
        }

        ~SubProcess()
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
